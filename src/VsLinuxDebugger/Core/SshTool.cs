using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Renci.SshNet;
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

        LogOutput($"BASH> {command}");
        var cmd = _ssh.RunCommand(command);

        return cmd.Result;
      }
      catch (Exception)
      {
        throw;
      }
    }

    /// <summary>Cleans the contents of the deployment path.</summary>
    public void CleanDeploymentFolder()
    {
      //// Bash($"sudo rm -rf {_opts.RemoteDeployBasePath}/*");
      Bash($"rm -rf {_opts.RemoteDeployBasePath}/*");
    }

    public bool Connect()
    {
      PrivateKeyFile keyFile = null;
      try
      {
        if (_opts.UserPrivateKeyEnabled)
          keyFile = new PrivateKeyFile(_opts.UserPrivateKeyPath);
        //// keyFile = new PrivateKeyFile(_opts.UserKeyFilePath, password);
      }
      catch (Exception ex)
      {
        LogOutput($"Private key error - {ex.Message}");
        LogOutput("Issue obtaining private key. Please check your settings to ensure a valid file exists (Tools > Options).");
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
        LogOutput($"Unable to connect to SSH. {ex.Message}");
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

      LogOutput($"Connected to {_connectionInfo.Username}@{_connectionInfo.Host}:{_connectionInfo.Port} via SSH and {(_sftp != null ? "SFTP" : "SCP")}");

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
      Bash($"mkdir -p {_opts.RemoteDeployDebugPath}");
      Bash($"mkdir -p {_opts.RemoteDeployReleasePath}");

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

      Bash("[ -d ~/.vsdbg ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/.vsdbg");
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
        Bash($@"mkdir -p {_opts.RemoteDeployDebugPath}");

        // TODO: Rev1 - Iterate through each file and upload it via SCP client or SFTP.
        // TODO: Rev2 - Compress _localHost.OutputDirFullName, upload ZIP, and unzip it.
        // TODO: Rev3 - Allow for both SFTP and SCP as a backup. This separating connection to a new disposable class.
        //// LogOutput($"Connected to {_connectionInfo.Username}@{_connectionInfo.Host}:{_connectionInfo.Port} via SSH and {(_sftpClient != null ? "SFTP" : "SCP")}");

        var srcDirInfo = new DirectoryInfo(_launch.OutputDirFullName);
        if (!srcDirInfo.Exists)
          throw new DirectoryNotFoundException($"Directory '{_launch.OutputDirFullName}' not found!");

        // Compress files to upload as single `tar.gz`.
        // TODO: Use base folder path: var pathTarGz = $"{_opts.RemoteDeployBasePath}/{_tarGzFileName}";

        var destTarGz = $"{_opts.RemoteDeployDebugPath}/{_tarGzFileName}";
        var success = PayloadCompressAndUpload(_sftp, srcDirInfo, destTarGz);

        // Decompress file
        PayloadDecompress(destTarGz, false);

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

      LogOutput($"Working directory changed to '{destinationDirectory}'");
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

    private void LogOutput(string message)
    {
      // TODO: Send to VS Output Window
      Console.WriteLine($">> {message}");
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
        var cmd = "set -e";
        cmd += $";cd \"{_opts.RemoteDeployDebugPath}\"";
        cmd += $";tar -zxf \"{_tarGzFileName}\"";
        ////cmd += $";tar -zxf \"{pathBuildTarGz}\"";

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
  }
}
