using Renci.SshNet;

namespace Xeno.VsLinuxDebug.Services
{
  public class ConnectionService : IConnectionService
  {
    private string _hostName = "";
    private string _userName = "pi";
    private string _userPass = "raspberry";

    public ConnectionService(string hostName, string userName, string userPass)
    {
      Ssh = new SshClient(hostName, userName, userPass);
      Sftp = new SftpClient(hostName, userName, userPass);
    }

    public SshClient Ssh { get; }

    public SftpClient Sftp { get; }

    public bool IsConnected { get; private set; }

    public void Connect()
    {
      if (IsConnected)
        return;

    }
  }
}
