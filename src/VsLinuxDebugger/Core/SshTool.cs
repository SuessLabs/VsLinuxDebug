using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    private SshConnectionInfo _info;
    private ScpClient _scp;
    private SftpClient _sftp;
    private SshClient _ssh;

    public SshTool(SshConnectionInfo info)
    {
      _info = info;
    }

    public SshClient Ssh => _ssh;

    public async Task<string> BashAsync(string command, bool createCommand = false)
    {
      try
      {
        if (!_isConnected)
        {
          // attempt to reconnect
          if (!await ConnectAsync())
            return "Cannot connect to remote host to execute Bash command.";
        }

        Logger.Output($"BASH> {command}");

        SshCommand cmd;

        cmd = await Task.Run(() =>
        {
          if (!createCommand)
          {
            cmd = _ssh.RunCommand(command);
          }
          else
          {
            cmd = _ssh.CreateCommand(command);
            cmd.Execute();
          }

          return cmd;
        });

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

      // TODO: Make async
      using (var stream = _ssh.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
      {
        stream.Write(command + "\n");

        if (!isSudo)
        {
          result = stream.ReadLine();
        }
        else
        {
          Logger.Debug($"BASH-SUDO: TRUE");
          result = stream.Expect("password");
          Logger.Debug($"BASH-SUDO: {result}");

          stream.Write(_info.UserPass + "\n");
          result = stream.Expect(prompt);
          Logger.Debug($"BASH-SUDO: {result}");
        }
      }

      return result;
    }

    /// <summary>Send Bash Commands via Shell Stream.</summary>
    /// <param name="command">Command to send.</param>
    /// <param name="searchText">Custom search text to wait for.</param>
    /// <returns>Results.</returns>
    public string BashStream(string command, string searchText)
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
        stream.Write(command + "\n");

        result = stream.ReadLine();

        Logger.Debug($"BashEx: Waiting for search text, '{searchText}'");
        result = stream.Expect(searchText);
        Logger.Debug($"BashEx: {result}");
      }

      return result;
    }

    /// <summary>Cleans the contents of the deployment path.</summary>
    /// <param name="fullScrub">Clear entire base deployment folder (TRUE) or just our project.</param>
    public async Task CleanFolderAsync(string path)
    {
      // Whole deployment folder and hidden files
      // rm -rf xxx/*      == Contents of the folder but not the folder itself
      // rm -rf xxx/{*,.*} == All hidden files and folders
      var allFilesAndFolders = "{*,.*}";
      await BashAsync($"rm -rf {path}/{allFilesAndFolders}"); // "~/LinuxDbg/MyProg/{*,.*}"

      // Not used
      ////if (fullScrub)
      ////  await BashAsync($"rm -rf \"{_opts.RemoteDeployBasePath}/{allFilesAndFolders}\"");    // "~/LinuxDbg/{*,.*}"
      ////else
      ////  await BashAsync($"rm -rf {_launch.RemoteDeployProjectFolder}/{allFilesAndFolders}"); // "~/LinuxDbg/MyProg/{*,.*}"
    }

    public async Task<bool> ConnectAsync()
    {
      PrivateKeyFile keyFile = null;

      try
      {
        if (_info.PrivateKeyEnabled)
        {
          if (string.IsNullOrEmpty(_info.PrivateKeyPassword))
            keyFile = new PrivateKeyFile(_info.PrivateKeyPath);
          else
            keyFile = new PrivateKeyFile(_info.PrivateKeyPath, _info.PrivateKeyPassword);
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
        _ssh = (_info.PrivateKeyEnabled && File.Exists(_info.PrivateKeyPath))
          ? new SshClient(_info.Host, _info.Port, _info.UserName, keyFile)
          : new SshClient(_info.Host, _info.Port, _info.UserName, _info.UserPass);

        await Task.Run(() => _ssh.Connect());
      }
      catch (Exception ex)
      {
        Logger.Output($"Unable to connect to SSH. {ex.Message}");
        return false;
      }

      try
      {
        var sftpClient = (keyFile == null)
            ? new SftpClient(_info.Host, _info.Port, _info.UserName, _info.UserPass)
            : new SftpClient(_info.Host, _info.Port, _info.UserName, keyFile);

        sftpClient.Connect();
        _sftp = sftpClient;
      }
      catch (Exception)
      {
        _scp = (keyFile == null)
          ? new ScpClient(_info.Host, _info.Port, _info.UserName, _info.UserPass)
          : new ScpClient(_info.Host, _info.Port, _info.UserName, keyFile);

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

    public async Task MakeDeploymentFolderAsync(string remoteBaseFolder)
    {
      // do we need SUDO? Not yet
      await BashAsync($"mkdir -p {remoteBaseFolder}");
      //// Bash($"mkdir -p {_opts.RemoteDeployDebugPath}");
      //// Bash($"mkdir -p {_opts.RemoteDeployReleasePath}");

      var group = string.IsNullOrEmpty(_info.UserGroup)
        ? string.Empty
        : $":{_info.UserGroup}";

      // Update ownership so it's not "root"
      await BashAsync($"sudo chown -R {_info.UserName}{group} {remoteBaseFolder}");
    }

    /// <summary>
    /// Install cURL and VS Debugger if it doesn't exist already.
    /// </summary>
    public async Task TryInstallVsDbgAsync(string vsDbgFolder)
    {
      string arch = (await BashAsync("uname -m")).Trim('\n');

      var curlExists = await BashAsync("which curl ; echo $?");
      if (curlExists.Equals("1\n"))
      {
        // TODO: Need to pass in password.
        var curlInstall = BashStream("sudo apt install curl");
        Logger.Output($"BASH-RET: {curlInstall}");
      }

      //// OLD:  var ret = Bash("[ -d ~/.vsdbg ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/.vsdbg");
      //// v1.9: var ret = await BashAsync($"[ -d {_opts.RemoteVsDbgBasePath} ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l {_opts.RemoteVsDbgBasePath}");

      // If output path does not exist, execute the following commands "curl .. | bash .."
      // 2022-10-27: Added '-k' to allow for Microsoft's self-signed certificate.
      var ret = await BashAsync($"[ -d {vsDbgFolder} ] || curl -ksSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l {vsDbgFolder}");
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

    /// <summary>Upload files to remote machine.</summary>
    /// <param name="sourceFolder">Local folder to upload from.</param>
    /// <param name="targetFolder">Remote target folder to upload to.</param>
    /// <returns></returns>
    public async Task<string> UploadFilesAsync(string sourceFolder, string targetFolder)
    {
      try
      {
        // Clean output folder just in case
        //// Bash($@"rm -rf {_launch.RemoteDeployFolder}");

        // TODO: Rev1 - Iterate through each file and upload it via SCP client or SFTP.
        // TODO: Rev2 - Compress _localHost.OutputDirFullName, upload ZIP, and unzip it.
        // TODO: Rev3 - Allow for both SFTP and SCP as a backup. This separating connection to a new disposable class.
        //// Logger.Output($"Connected to {_connectionInfo.Username}@{_connectionInfo.Host}:{_connectionInfo.Port} via SSH and {(_sftpClient != null ? "SFTP" : "SCP")}");

        var srcDirInfo = new DirectoryInfo(sourceFolder);
        if (!srcDirInfo.Exists)
          throw new DirectoryNotFoundException($"Directory '{sourceFolder}' not found!");

        await BashAsync($@"mkdir -p {targetFolder}");

        // Compress files to upload as single `tar.gz`.
        var destTarGz = LinuxPath.Combine(targetFolder, _tarGzFileName);
        Logger.Output($"Destination Tar.GZ: '{destTarGz}'");

        var success = await PayloadCompressAndUploadAsync(_sftp, srcDirInfo, destTarGz);

        // Decompress file
        await PayloadDecompressAsync(targetFolder, destTarGz, false);

        Logger.Output($"Upload completed {(success ? "successfully" : "with failure")}.");

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
      Logger.Output($"Getting bin files for transfer...");
      var localFiles = GetLocalFiles(srcDirInfo);

      Logger.Output($"Compressing files for transfer...");

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

            Logger.Output("Uploading...");
            await Task.Run(() =>
            {
              tarGzStream.Seek(0, SeekOrigin.Begin);

              //TODO: Use, this.UploadFile(tarGzStream, pathBuildTarGz);
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
    /// <param name="targetFolder">Remote target folder to upload to.</param>
    /// <param name="pathBuildTarGz">Path to upload to.</param>
    /// <param name="removeTarGz">Remove our build's tar.gz file. Set to FALSE for debugging. (default=true)</param>
    /// <returns>Returns true on success.</returns>
    private async Task<bool> PayloadDecompressAsync(string targetFolder, string pathBuildTarGz, bool removeTarGz = true)
    {
      try
      {
        var cmd = "set -e";
        cmd += $";cd \"{targetFolder}\"";
        cmd += $";tar -zxf \"{_tarGzFileName}\"";
        ////cmd += $";tar -zxf \"{pathBuildTarGz}\"";

        if (removeTarGz)
          cmd += $";rm \"{pathBuildTarGz}\"";

        string decompressOutput = await BashAsync(cmd);
        Logger.Output($"Payload Decompress results: '{decompressOutput}' (blank=OK)");
      }
      catch (Exception)
      {
        return false;
      }

      return true;
    }
  }
}
