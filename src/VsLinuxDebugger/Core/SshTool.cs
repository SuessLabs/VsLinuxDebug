using System;
using System.IO;
using Renci.SshNet;

namespace VsLinuxDebugger.Core
{
  public class SshTool : IDisposable
  {
    private Renci.SshNet.ScpClient _scpClient;
    private Renci.SshNet.SftpClient _sftpClient;
    private Renci.SshNet.SshClient _sshClient;

    private UserOptions _opts;
    private bool _isConnected;

    public SshTool(UserOptions opts)
    {
      _opts = opts;
    }

    public string Bash(string command)
    {

      return "";
    }

    public bool Connect()
    {
      PrivateKeyFile keyFile = null;
      try
      {
        keyFile = new PrivateKeyFile(_opts.UserPrivateKeyPath);
        //// keyFile = new PrivateKeyFile(_opts.UserKeyFilePath, password);
      }
      catch (Exception)
      {
      }

      try
      {
        _sshClient = (_opts.UserPrivateKeyEnabled && File.Exists(_opts.UserPrivateKeyPath))
          ? new SshClient(_opts.HostIp, _opts.HostPort, _opts.UserName, keyFile)
          : new SshClient(_opts.HostIp, _opts.HostPort, _opts.UserName, _opts.UserPass);

        _sshClient.Connect();
      }
      catch(Exception ex)
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
        _sftpClient = sftpClient;
      }
      catch (Exception)
      {
        _scpClient = (keyFile == null)
          ? new ScpClient(_opts.HostIp, _opts.HostPort, _opts.UserName, _opts.UserPass)
          : new ScpClient(_opts.HostIp, _opts.HostPort, _opts.UserName, keyFile);

        _scpClient.Connect();
      }

      var _connectionInfo = _sshClient.ConnectionInfo;

      LogOutput($"Connected to {_connectionInfo.Username}@{_connectionInfo.Host}:{_connectionInfo.Port} via SSH and {(_sftpClient != null ? "SFTP" : "SCP")}");

      _isConnected = true;

      //// ChangeWorkingDirectory(workingDirectory);
      return true;
    }

    public void MakeDeploymentFolder()
    {
      Bash($"sudo mkdir -p {_opts.RemoteDeployBasePath}");
      Bash($"sudo mkdir -p {_opts.RemoteDeployDebugPath}");
      Bash($"sudo mkdir -p {_opts.RemoteDeployReleasePath}");

      var group = string.IsNullOrEmpty(_opts.UserGroupName)
        ? string.Empty
        : $":{_opts.UserGroupName}";

      // Update ownership so it's not "root"
      Bash($"sudo chown -R {_opts.UserName}{group} {_opts.RemoteDeployBasePath}");

    }

    public void Clean() => Bash($"sudo rm -rf {_opts.RemoteDeployBasePath}/*");

    public void Dispose()
    {
      _sshClient?.Dispose();
      _sftpClient?.Dispose();
      _scpClient?.Dispose();
    }

    /// <summary>
    /// Instals VS Debugger if it doesn't exist already
    /// </summary>
    public void TryInstallVsDbg()
    {
      string arch = Bash("uname -m").Trim('\n');

      // It seems like the latest versions of getvsdbgsh actually figures out the right version itself
      Bash("[ -d ~/.vsdbg ] || curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/.vsdbg");
    }

    private void LogOutput(string message)
    {
      // TODO: Send to VS Output Window
      Console.WriteLine($">> {message}");
    }
  }
}
