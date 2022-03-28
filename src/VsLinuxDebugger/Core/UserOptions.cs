namespace VsLinuxDebugger.Core
{
  public class UserOptions
  {
    public string HostIp { get; set; }
    public int HostPort { get; set; }

    public bool LocalPlinkEnabled { get; set; }
    public string LocalPLinkPath { get; set; }

    public bool RemoteDebugDisplayGui { get; set; }
    public string RemoteDeployBasePath { get; set; }
    public string RemoteDeployDebugPath { get; set; }
    public string RemoteDeployReleasePath { get; set; }
    public string RemoteDotNetPath { get; set; }
    public string RemoteVsDbgPath { get; set; }

    public bool UseCommandLineArgs { get; set; }
    public bool UsePublish { get; set; }

    public string UserGroupName { get; set; }
    public string UserName { get; set; }
    public string UserPass { get; set; }
    public bool UserPrivateKeyEnabled { get; set; }
    public string UserPrivateKeyPath { get; set; }
  }
}
