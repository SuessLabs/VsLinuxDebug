using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using SharpCompress.Common;
using SharpCompress.Writers;
using Process = System.Diagnostics.Process;
using Task = System.Threading.Tasks.Task;

namespace VSLinuxDebugger
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class RemoteDebugCommand
  {
    /// <summary>Command ID.</summary>
    public const int CommandId = 0x0100;

    /// <summary>Command menu group (command set GUID).</summary>
    public static readonly Guid CommandSet = new Guid("5b4eaa99-73ea-49a5-99c3-bd64eecafa37");

    /// <summary>VS Package that provides this command, not null.</summary>
    private readonly AsyncPackage _package;

    private readonly string _tarGzFileName = "vsldBuildContents.tar.gz";

    private VSLinuxDebuggerPackage Settings => _package as VSLinuxDebuggerPackage;
    private LocalHost _localhost;
    private bool _isBuildSucceeded;
    private string _launchJsonPath = string.Empty;

    public static BuildEvents BuildEvents { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteDebugCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private RemoteDebugCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      _package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Execute, menuCommandID);

      commandService.AddCommand(menuItem);
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
      // Switch to the main thread - the call to AddCommand in RemoteDebug's constructor requires the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
      var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)).ConfigureAwait(false) as OleMenuCommandService;
      Instance = new RemoteDebugCommand(package, commandService);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static RemoteDebugCommand Instance { get; private set; }

    /// <summary>
    /// Wrapper around a (alert) messagebox
    /// </summary>
    /// <param name="message"></param>
    private void MsgBox(string message, string title = "Error") => VsShellUtilities.ShowMessageBox(
      _package,
      message,
      title,
      OLEMSGICON.OLEMSGICON_INFO,
      OLEMSGBUTTON.OLEMSGBUTTON_OK,
      OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    private async void Execute(object sender, EventArgs e)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      try
      {
        bool connected = await Task.Run(() => CheckConnWithRemote()).ConfigureAwait(true);
      }
      catch (Exception connection_exception)
      {
        MsgBox($"Cannot connect to {Settings.UserName}:{Settings.IP}.:" + connection_exception.ToString());
        return;
      }

      if (!InitSolution())
      {
        MsgBox("Please select a startup project");
      }
      else
      {
        await Task.Run(() =>
        {
          TryInstallVsDbg();
          MkDir();
          Clean();
        }).ConfigureAwait(true);

        if (!Settings.Publish)
        {
          Bash($"export DISPLAY=:0");
          Build(); // once this finishes it will raise an event; see BuildEvents_OnBuildDone
        }
        else
        {
          int exitcode = await PublishAsync().ConfigureAwait(true);

          if (exitcode != 0)
          {
            MsgBox("File transfer to Remote Machine failed");
          }
          else
          {
            string errormessage = await TransferFiles2Async().ConfigureAwait(true);

            if (errormessage != "")
            {
              MsgBox("Build failed: " + errormessage);
            }
            else
            {
              if (Settings.NoDebug)
              {
                MsgBox("Files successfully transfered to remote machine", "Success");
              }
              else
              {
                Bash($"export DISPLAY=:0");
                Debug();
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Must be called on the UI thread
    /// </summary>
    private bool InitSolution()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
      var project = dte.Solution.GetStartupProject();

      if (project == null)
      {
        return false;
      }

      _localhost = new LocalHost(Settings.UserName, Settings.UserPass, Settings.IP, Settings.VsDbgPath, Settings.DotnetPath, Settings.DebugFolderPath, Settings.UseSshKeyFile, Settings.UsePlinkForDebugging);

      _localhost.ProjectFullName = project.FullName;
      _localhost.ProjectName = project.Name;
      _localhost.Assemblyname = project.Properties.Item("AssemblyName").Value.ToString();
      _localhost.SolutionFullName = dte.Solution.FullName;
      _localhost.SolutionDirPath = Path.GetDirectoryName(_localhost.SolutionFullName);
      _localhost.ProjectConfigName = project.ConfigurationManager.ActiveConfiguration.ConfigurationName;
      _localhost.OutputDirName = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
      _localhost.OutputDirFullName = Path.Combine(Path.GetDirectoryName(project.FullName), _localhost.OutputDirName);

      if (Settings.UseCommandLineArgs)
      {
        _localhost.CommandLineArguments = GetArgs(_localhost.SolutionDirPath);
      }

      string debugtext = $"ProjectFullName: {_localhost.ProjectFullName} \nProjectName: {_localhost.ProjectName} \n" +
        $"SolutionFullName: {_localhost.SolutionFullName} \nSolutionDirPath:{_localhost.SolutionDirPath} \n" +
        $"ProjectConfigname: {_localhost.ProjectConfigName} \nOutputDirName: {_localhost.OutputDirName} \nOutputDirFullName: {_localhost.OutputDirFullName}";

      return true;
    }

    /// <summary>
    /// Checks if a connection with the remote machine can be established
    /// </summary>
    /// <returns></returns>
    private bool CheckConnWithRemote()
    {
      try
      {
        Bash("echo hello");
        return true;
      }
      catch (Exception)
      {
        throw;
      }
    }

    /// <summary>
    /// create debug/release folders and take ownership
    /// </summary>
    private void MkDir()
    {
      Bash($"sudo mkdir -p {Settings.DebugFolderPath}");
      Bash($"sudo mkdir -p {Settings.ReleaseFolderPath}");
      ////Bash($"sudo chown -R {Settings.UserName}:{Settings.GroupName} {Settings.AppFolderPath}");

      var group = string.IsNullOrEmpty(Settings.GroupName) ? string.Empty : $":{Settings.GroupName}";
      Bash($"sudo chown -R {Settings.UserName}{group} {Settings.AppFolderPath}");
    }

    /// <summary>
    /// clean everything in the debug directory
    /// </summary>
    private void Clean() => Bash($"sudo rm -rf {Settings.DebugFolderPath}/*");

    /// <summary>
    /// Instals VS Debugger if it doesn't exist already
    /// </summary>
    private void TryInstallVsDbg()
    {
      string arch = Bash("uname -m").Trim('\n');

      // It seems like the latest versions of getvsdbgsh actually figures out the right version itself
      Bash("[ -d ~/.vsdbg ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/.vsdbg");

      /*switch (arch)
      {
        case "arm7l":
          Bash("[ -d ~/.vsdbg ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -r linux-arm -v latest -l ~/.vsdbg");
          break;

        case "aarch64":
          Bash("[ -d ~/.vsdbg ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -r linux-arm64 -v latest -l ~/.vsdbg");
          break;

        default:
          break;
      }*/
    }

    private void Build()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE)Package.GetGlobalService(typeof(DTE));
      BuildEvents = dte.Events.BuildEvents;
      // For some reason, cleanup isn't actually always ran when there has been an error.
      // This removes the fact that if you run a debug attempt, get a file error, that you don't get 2 message boxes, 3 message boxes, etc for each attempt.
      BuildEvents.OnBuildDone -= BuildEvents_OnBuildDoneAsync;
      BuildEvents.OnBuildDone += BuildEvents_OnBuildDoneAsync;
      BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
      BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;
      dte.SuppressUI = false;
      dte.Solution.SolutionBuild.BuildProject(_localhost.ProjectConfigName, _localhost.ProjectFullName);
    }

    private void LogOutput(string message)
    {
      //// if (Settings.LogVerbose)
      Console.WriteLine(message);
    }

    /// <summary>
    /// Publish the solution. Publishing is done via an external process
    /// </summary>
    /// <returns></returns>
    private async Task<int> PublishAsync()
    {
      using (var process = new Process())
      {
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.Arguments = $@"publish -c {_localhost.ProjectConfigName} -r linux-arm --self-contained=false -o {_localhost.OutputDirFullName} {_localhost.ProjectFullName}";
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        return await process.WaitForExitAsync().ConfigureAwait(true);
      }
    }

    private async Task<string> TransferFiles2Async()
    {
      LogOutput($"Connecting to {Settings.UserName}@{Settings.IP}:{Settings.HostPort}...");

      try
      {
        Bash($@"mkdir -p {Settings.DebugFolderPath}");

        // TODO: Rev1 - Iterate through each file and upload it via SCP client or SFTP.
        // TODO: Rev2 - Compress _localHost.OutputDirFullName, upload ZIP, and unzip it.
        // TODO: Rev3 - Allow for both SFTP and SCP as a backup. This separating connection to a new disposable class.
        //// LogOutput($"Connected to {_connectionInfo.Username}@{_connectionInfo.Host}:{_connectionInfo.Port} via SSH and {(_sftpClient != null ? "SFTP" : "SCP")}");

        // Sample SCP Connection:
        //// LogOutput($"Error: {ex.Message} Is SFTP supported for {username}@{host}:{port}? We are using SCP instead!");
        //// _scpClient = (keyFile == null)
        ////    ? new ScpClient(host, port, username, password)
        ////    : new ScpClient(host, port, username, keyFile);
        //// _scpClient.Connect();

        using (var sftp = !Settings.UseSshKeyFile
            ? new Renci.SshNet.SftpClient(Settings.IP, 22, Settings.UserName, Settings.UserPass)
            : null) //// new SftpClient(Settings.IP, Settings.HostPort, Settings.UserName, Settings.SshKeyFile))
        {
          sftp.Connect();
          LogOutput("Connected to via SSH and SFTP");

          var srcDirInfo = new DirectoryInfo(_localhost.OutputDirFullName);
          if (!srcDirInfo.Exists)
            throw new DirectoryNotFoundException($"Directory '{_localhost.OutputDirFullName}' not found!");

          // Compress files to upload as single `tar.gz`.
          // TODO: Use base folder path: var pathTarGz = $"{Settings.AppFolderPath}/{_tarGzFileName}";
          var pathTarGz = $"{Settings.DebugFolderPath}/{_tarGzFileName}";
          var success = PayloadCompressAndUpload(sftp, srcDirInfo, pathTarGz);

          // Decompress file
          PayloadDecompress(pathTarGz, false);
        };

        return string.Empty;
      }
      catch (Exception ex)
      {
        return ex.ToString();
      }
    }

    /// <summary>Compress build contents and upload to remote host.</summary>
    /// <param name="sftp">SFTP connection.</param>
    /// <param name="srcDirInfo">Build (source) contents directory info.</param>
    /// <param name="pathBuildTarGz">Upload path and filename of build's tar.gz file.</param>
    /// <returns></returns>
    private bool PayloadCompressAndUpload(SftpClient sftp, DirectoryInfo srcDirInfo, string pathBuildTarGz)
    {
      var success = false;
      var localFiles = GetLocalFiles(srcDirInfo);

      // TODO: Delta remote files against local files for changes.
      using (Stream tarGzStream = new MemoryStream())
      {
        try
        {
          using (var tarGzWriter = WriterFactory.Open(tarGzStream, ArchiveType.Tar, CompressionType.GZip))
          {
            using (MemoryStream fileStream = new MemoryStream())
            {
              using (BinaryWriter fileWriter = new BinaryWriter(fileStream))
              {
                fileWriter.Write(localFiles.Count);

                var updateFileCount = 0;
                long updateFileSize = 0;
                var allFileCount = 0;
                long allFileSize = 0;

                foreach (var file in localFiles)
                {
                  allFileCount++;
                  allFileSize += file.Value.Length;

                  // TODO: Add new cache file entry
                  //// UpdateCacheEntry.WriteToStream(newCacheFileWriter, file.Key, file.Value);

                  updateFileCount++;
                  updateFileSize += file.Value.Length;

                  try
                  {
                    tarGzWriter.Write(file.Key, file.Value);
                  }
                  catch (IOException ioEx)
                  {
                    LogOutput($"Exception: {ioEx.Message}");
                  }
                  catch (Exception ex)
                  {
                    LogOutput($"Exception: {ex.Message}\n{ex.StackTrace}");
                  }
                }

                LogOutput($"{updateFileCount,7:n0} [{updateFileSize,13:n0} bytes] of {allFileCount,7:n0} [{allFileSize,13:n0} bytes] files need to be updated");
              }
            }
          }

          success = true;
        }
        catch (Exception ex)
        {
          LogOutput($"Error while compressing file contents. {ex.Message}\n{ex.StackTrace}");
        }

        // Upload the file
        if (success)
        {
          try
          {
            var tarGzSize = tarGzStream.Length;
            tarGzStream.Seek(0, SeekOrigin.Begin);

            sftp.UploadFile(tarGzStream, pathBuildTarGz);

            LogOutput($"Uploaded '{_tarGzFileName}' [{tarGzSize,13:n0} bytes].");
            success = true;
          }
          catch (Exception ex)
          {
            LogOutput($"Error while uploading file. {ex.Message}\n{ex.StackTrace}");
            success = false;
          }
        }
      }

      return success;
    }

    /// <summary>Unpack build contents.</summary>
    /// <param name="pathBuildTarGz">Path to upload to.</param>
    /// <param name="removeTarGz">Remove our build's tar.gz file. Set to FALSE for debugging. (default=true)</param>
    /// <returns>Returns true on success.</returns>
    private bool PayloadDecompress(string pathBuildTarGz, bool removeTarGz = true)
    {
      try
      {
        var cmd = $"set -e;cd \"{Settings.DebugFolderPath}\"";
        cmd += $";tar -zxf \"{pathBuildTarGz}\"";

        if (removeTarGz)
          cmd += $";rm \"{pathBuildTarGz}\"";

        var output = Bash(cmd);
        LogOutput(output);

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>Get all files to transfer.</summary>
    /// <param name="srcDirInfo"></param>
    /// <returns>Collection of file names and their full path.</returns>
    private ConcurrentDictionary<string, FileInfo> GetLocalFiles(DirectoryInfo srcDirInfo)
    {
      var startIndex = srcDirInfo.FullName.Length;
      var localFileCache = new ConcurrentDictionary<string, FileInfo>();
      Parallel.ForEach(GetFiles(srcDirInfo.FullName), file =>
      {
        var cleanedRelativeFilePath = file.Substring(startIndex);
        cleanedRelativeFilePath = cleanedRelativeFilePath.Replace("\\", "/").TrimStart('/');
        localFileCache[cleanedRelativeFilePath] = new FileInfo(file);
      });

      LogOutput($"Local file cache created");
      return localFileCache;
    }

    /// <summary>Get all files, including subdirectories.</summary>
    /// <param name="path">Base path.</param>
    /// <returns>Collection of files in folder path.</returns>
    private IEnumerable<string> GetFiles(string path)
    {
      Queue<string> queue = new Queue<string>();
      queue.Enqueue(path);

      while (queue.Count > 0)
      {
        path = queue.Dequeue();
        try
        {
          foreach (string subDir in Directory.GetDirectories(path))
            queue.Enqueue(subDir);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine(ex);
        }

        string[] files = null;
        try
        {
          files = Directory.GetFiles(path);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine(ex);
        }

        if (files != null)
        {
          for (int i = 0; i < files.Length; i++)
            yield return files[i];
        }
      }
    }

    /// <summary>
    /// Start debugging using the remote visual studio server adapter
    /// </summary>
    private void Debug()
    {
      _launchJsonPath = _localhost.ToJson();
      
      var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
      dte.ExecuteCommand("DebugAdapterHost.Launch", $"/LaunchJson:\"{_launchJsonPath}\"");
    }

    private void Cleanup()
    {
      File.Delete(_launchJsonPath);

      BuildEvents.OnBuildDone -= BuildEvents_OnBuildDoneAsync;
      BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
    }

    private string Bash(string cmd)
    {
      try
      {
        // TODO: Assign port.  new SshClient(host, port, userName, userPass);
        using (var client = Settings.UseSshKeyFile ?
            new SshClient(Settings.IP, Settings.UserName, new PrivateKeyFile[] { new PrivateKeyFile(LocalHost.SSH_KEY_PATH) }) :
            new SshClient(Settings.IP, Settings.UserName, Settings.UserPass))
        {
          client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
          client.Connect();
          var sshcmd = client.RunCommand(cmd);
          client.Disconnect();

          return sshcmd.Result;
        }
      }
      catch (Exception)
      {
        throw;
      }
    }

    /// <summary>
    /// The build is finised sucessfully only when the startup project has been compiled without any errors
    /// </summary>
    private void BuildEvents_OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
    {
      string debugtext = $"Project: {project} --- Success: {success}\n";

      if (!success)
      {
        Cleanup();
      }

      _isBuildSucceeded = Path.GetFileName(project) == _localhost.ProjectName + ".csproj" && success;
    }

    /// <summary>
    /// Build finished. We can now transfer the files to the remote host and start debugging the program
    /// </summary>
    private async void BuildEvents_OnBuildDoneAsync(vsBuildScope scope, vsBuildAction action)
    {
      if (_isBuildSucceeded)
      {
        string errormessage = await TransferFiles2Async().ConfigureAwait(true);
        //// string errormessage = await TransferFilesAsync().ConfigureAwait(true);

        if (errormessage == "")
        {
          if (Settings.NoDebug)
          {
            MsgBox("Files sucessfully transfered to remote machine", "Success");
          }
          else
          {
            Debug();
          }

          Cleanup();
        }
        else
        {
          MsgBox($"Transferring files failed: {errormessage}");
        }
      }
    }

    /// <summary>
    /// Tries to search for a file "launchSettings.json", which is used by Visual Studio to store the command line arguments
    /// </summary>
    /// <param name="slnDirPath"></param>
    /// <remarks>If using multiple debugging profiles, this will not work</remarks>
    /// <returns>The debugging command arguments set in Project Settings -> Debug -> Command Line Arguments</returns>
    private string GetArgs(string slnDirPath)
    {
      string args = String.Empty;
      string[] launchSettingsOccurences = Directory.GetFiles(slnDirPath, "launchSettings.json", SearchOption.AllDirectories);

      if (launchSettingsOccurences.Length == 1)
      {
        var jobj = JObject.Parse(File.ReadAllText(launchSettingsOccurences[0]));

        var commandLineArgsOccurences = jobj.SelectTokens("$..commandLineArgs")
          .Select(t => t.Value<string>())
          .ToList();

        if (commandLineArgsOccurences.Count > 1)
        {
          MsgBox(
            "Multiple debugging profiles detected. Due to Visual Studio API limitations, command line arguments cannot be used. \n" +
            "Please turn off Command Line Arguments in the extension settings page");
        }
        else if (commandLineArgsOccurences.Count == 1)
        {
          args = commandLineArgsOccurences[0];
        }
      }
      else if (launchSettingsOccurences.Length > 1)
      {
        MsgBox("Cannot read command line arguments");
      }

      return args;
    }
  }
}
