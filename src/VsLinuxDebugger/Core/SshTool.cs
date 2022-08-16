using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Renci.SshNet;
using Renci.SshNet.Common;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace VsLinuxDebugger.Core
{
  public class SshTool : IDisposable
  {
    private readonly string _tarGzFileName = "vsldBuildContents.tar.gz";

    private bool _isConnected = false;
    private LaunchBuilder _launch;
    private UserOptions _opts;
    private ScpClient _scp;
    private SftpClient _sftp;
    private SshClient _ssh;

    public SshTool(UserOptions opts, LaunchBuilder launch)
    {
      _opts = opts;
      _launch = launch;
    }

    public string Bash(string command)
    {
      try
      {
        if (!_isConnected)
        {
          // attempt to reconnect
          if (!Connect())
            return "Cannot connect to remote host to execute Bash command.";
        }

        Logger.Output($"BASH> {command}");
        var cmd = _ssh.RunCommand(command);

        return cmd.Result;
      }
      catch (Exception)
      {
        throw;
      }
    }

    /// <summary>Send Bash Commands via Shell Stream.</summary>
    /// <param name="command">Command to send.</param>
    /// <param name="isSudo">Send command with SUDO.</param>
    /// <returns>Results.</returns>
    public string BashStream(string command, bool isSudo = false)
    {
      var result = string.Empty;

      // RegEx for pattern matching terminal prompt
      //  "[$>]"       BASIC
      //  @"\][#$>]"   FAILS - Usecase: "[user@mach]$
      //  "([$#>:])"
      var prompt = new Regex("([$#>:])");
      var modes = new Dictionary<TerminalModes, uint>();
      // modes.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

      using (var stream = _ssh.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
      {
        // Can only run this once.
        ////ForwardedPortRemote fwdPort = new ForwardedPortRemote(6000, _opts.HostIp, 6000);
        ////_ssh.AddForwardedPort(fwdPort);
        ////fwdPort.Start();
        //// ... fwdPort.Stop();
        stream.Write(command + "\n");

        if (isSudo)
        {
          Logger.Output($"BASH-SUDO: TRUE");
          result = stream.Expect("password");
          Logger.Output($"BASH-SUDO: {result}");

          stream.Write(_opts.UserPass + "\n");
          result = stream.Expect(prompt);
          Logger.Output($"BASH-SUDO: {result}");
        }
      }

      return result;
    }

    /// <summary>Cleans the contents of the deployment path.</summary>
    /// <param name="fullScrub">Clear entire base deployment folder (TRUE) or just our project.</param>
    public void CleanDeploymentFolder(bool fullScrub = false)
    {
      // Whole deployment folder and hidden files
      // rm -rf xxx/*      == Contents of the folder but not the folder itself
      // rm -rf xxx/{*,.*} == All hidden files and folders
      var filesAndFolders = "{*,.*}";

      if (fullScrub)
      {
        Bash($"rm -rf \"{_opts.RemoteDeployBasePath}/{filesAndFolders}\"");
      }
      else
      {
        Bash($"rm -rf {_launch.RemoteDeployProjectFolder}/{filesAndFolders}");
      }
    }

    public bool Connect()
    {
      PrivateKeyFile keyFile = null;

      try
      {
        if (_opts.UserPrivateKeyEnabled)
        {
          if (string.IsNullOrEmpty(_opts.UserPrivateKeyPassword))
            keyFile = new PrivateKeyFile(_opts.UserPrivateKeyPath);
          else
            keyFile = new PrivateKeyFile(_opts.UserPrivateKeyPath, _opts.UserPrivateKeyPassword);
        }
      }
      catch (Exception ex)
      {
        Logger.Output($"Private key error - {ex.Message}");
        Logger.Output("Issue obtaining private key. Please check your settings to ensure a valid file exists (Tools > Options).");
        return false;
      }

      try
      {
        _ssh = (_opts.UserPrivateKeyEnabled && File.Exists(_opts.UserPrivateKeyPath))
          ? new SshClient(_opts.HostIp, _opts.HostPort, _opts.UserName, keyFile)
          : new SshClient(_opts.HostIp, _opts.HostPort, _opts.UserName, _opts.UserPass);

        _ssh.Connect();
      }
      catch (Exception ex)
      {
        Logger.Output($"Unable to connect to SSH. {ex.Message}");
        return false;
      }

      try
      {
        var sftpClient = (keyFile == null)
            ? new SftpClient(_opts.HostIp, _opts.HostPort, _opts.UserName, _opts.UserPass)
            : new SftpClient(_opts.HostIp, _opts.HostPort, _opts.UserName, keyFile);

        sftpClient.Connect();
        _sftp = sftpClient;
      }
      catch (Exception)
      {
        _scp = (keyFile == null)
          ? new ScpClient(_opts.HostIp, _opts.HostPort, _opts.UserName, _opts.UserPass)
          : new ScpClient(_opts.HostIp, _opts.HostPort, _opts.UserName, keyFile);

        _scp.Connect();
      }

      var _connectionInfo = _ssh.ConnectionInfo;

      Logger.Output($"Connected to {_connectionInfo.Username}@{_connectionInfo.Host}:{_connectionInfo.Port} via SSH and {(_sftp != null ? "SFTP" : "SCP")}");

      _isConnected = true;

      // SFTP may need to change to our home directory?
      //// ChangeWorkingDirectory(workingDirectory);
      return true;
    }

    public void Dispose()
    {
      _ssh?.Dispose();
      _sftp?.Dispose();
      _scp?.Dispose();
    }

    public void MakeDeploymentFolder()
    {
      // do we need SUDO?
      Bash($"mkdir -p {_opts.RemoteDeployBasePath}");
      //// Bash($"mkdir -p {_opts.RemoteDeployDebugPath}");
      //// Bash($"mkdir -p {_opts.RemoteDeployReleasePath}");

      var group = string.IsNullOrEmpty(_opts.UserGroupName)
        ? string.Empty
        : $":{_opts.UserGroupName}";

      // Update ownership so it's not "root"
      Bash($"sudo chown -R {_opts.UserName}{group} {_opts.RemoteDeployBasePath}");
    }

    /// <summary>
    /// Instals VS Debugger if it doesn't exist already
    /// </summary>
    public void TryInstallVsDbg()
    {
      string arch = Bash("uname -m").Trim('\n');

      var curlExists = Bash("which curl ; echo $?");
      if (curlExists.Equals("1\n"))
      {
        // TODO: Need to pass in password.
        var curlInstall = BashStream("sudo apt install curl");
        Logger.Output($"BASH-RET: {curlInstall}");
      }

      //// OLD: var ret = Bash("[ -d ~/.vsdbg ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/.vsdbg");
      var ret = Bash($"[ -d {_opts.RemoteVsDbgBasePath} ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l {_opts.RemoteVsDbgBasePath}");
      Logger.Output($"Returned: {ret}");
    }

    public void UploadFile(Stream input, string path)
    {
      if (_sftp != null)
      {
        _sftp.UploadFile(input, path);
      }
      else
      {
        throw new NotImplementedException();
        //// _scp.Upload(input, _scpDestinationDirectory + "/" + path);
      }
    }

    public async Task<string> UploadFilesAsync()
    {
      try
      {
        // Clean output folder just incase
        //// Bash($@"rm -rf {_launch.RemoteDeployFolder}");

        // TODO: Rev1 - Iterate through each file and upload it via SCP client or SFTP.
        // TODO: Rev2 - Compress _localHost.OutputDirFullName, upload ZIP, and unzip it.
        // TODO: Rev3 - Allow for both SFTP and SCP as a backup. This separating connection to a new disposable class.
        //// Logger.Output($"Connected to {_connectionInfo.Username}@{_connectionInfo.Host}:{_connectionInfo.Port} via SSH and {(_sftpClient != null ? "SFTP" : "SCP")}");

        Bash($@"mkdir -p {_launch.RemoteDeployProjectFolder}");

        var srcDirInfo = new DirectoryInfo(_launch.OutputDirFullPath);
        if (!srcDirInfo.Exists)
          throw new DirectoryNotFoundException($"Directory '{_launch.OutputDirFullPath}' not found!");

        // Compress files to upload as single `tar.gz`.
        var destTarGz = LinuxPath.Combine(_launch.RemoteDeployProjectFolder, _tarGzFileName);
        Logger.Output($"Destination Tar.GZ: '{destTarGz}'");

        var success = await PayloadCompressAndUploadAsync(_sftp, srcDirInfo, destTarGz);

        // Decompress file
        await PayloadDecompressAsync(destTarGz, false);

        return string.Empty;
      }
      catch (Exception ex)
      {
        return ex.ToString();
      }
    }

    private void ChangeWorkingDirectory(string destinationDirectory)
    {
      var cmd = _ssh.RunCommand($"mkdir -p \"{destinationDirectory}\"");
      if (cmd.ExitStatus != 0)
      {
        throw new Exception(cmd.Error);
      }

      if (_sftp != null)
      {
        _sftp.ChangeDirectory(destinationDirectory);
        //// _scpDestinationDirectory = "";
      }
      else
      {
        throw new NotImplementedException();
        //// _scpDestinationDirectory = destinationDirectory;
      }

      Logger.Output($"Working directory changed to '{destinationDirectory}'");
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
          {
            // Don't include "launch.json"
            if (files[i].EndsWith("launch.json"))
              continue;

            yield return files[i];
          }
        }
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

      Logger.Output($"Local file cache created");
      return localFileCache;
    }

    /// <summary>Compress build contents and upload to remote host.</summary>
    /// <param name="sftp">SFTP connection.</param>
    /// <param name="srcDirInfo">Build (source) contents directory info.</param>
    /// <param name="pathBuildTarGz">Upload path and filename of build's tar.gz file.</param>
    /// <returns></returns>
    private async Task<bool> PayloadCompressAndUploadAsync(SftpClient sftp, DirectoryInfo srcDirInfo, string pathBuildTarGz)
    {
      var success = false;
      var localFiles = GetLocalFiles(srcDirInfo);

      // TODO: Delta remote files against local files for changes.
      using (Stream tarGzStream = new MemoryStream())
      {
        var outputMsg = string.Empty;

        await Task.Run(() =>
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
                      outputMsg += $"Exception: {ioEx.Message}{Environment.NewLine}";
                    }
                    catch (Exception ex)
                    {
                      outputMsg += $"Exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
                    }
                  }

                  outputMsg += $"Update file count: {updateFileCount}; File Size: [{updateFileSize} bytes] of Total Files: {allFileCount} [{allFileSize} bytes] need to be updated";
                }
              }
            }

            success = true;
          }
          catch (Exception ex)
          {
            outputMsg += $"Error while compressing file contents. {ex.Message}\n{ex.StackTrace}";
          }
        });

        Logger.Output(outputMsg);

        // Upload the file
        if (success)
        {
          try
          {
            var tarGzSize = tarGzStream.Length;

            await Task.Run(() =>
            {
              tarGzStream.Seek(0, SeekOrigin.Begin);

              //TODO: Use this --> UploadFile(tarGzStream, pathBuildTarGz);
              sftp.UploadFile(tarGzStream, pathBuildTarGz);
            });

            Logger.Output($"Uploaded '{_tarGzFileName}' [{tarGzSize,13:n0} bytes].");
            success = true;
          }
          catch (Exception ex)
          {
            Logger.Output($"Error while uploading file. {ex.Message}\n{ex.StackTrace}");
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
    private async Task<bool> PayloadDecompressAsync(string pathBuildTarGz, bool removeTarGz = true)
    {
      try
      {
        var decompressOutput = string.Empty;

        var cmd = "set -e";
        cmd += $";cd \"{_launch.RemoteDeployProjectFolder}\"";
        cmd += $";tar -zxf \"{_tarGzFileName}\"";
        ////cmd += $";tar -zxf \"{pathBuildTarGz}\"";

        if (removeTarGz)
          cmd += $";rm \"{pathBuildTarGz}\"";

        await Task.Run(() =>
        {
          decompressOutput = Bash(cmd);
        });

        Logger.Output($"Payload Decompress results: '{decompressOutput}' (blank=OK)");

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}
