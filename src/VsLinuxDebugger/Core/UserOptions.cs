namespace VsLinuxDebugger.Core
{
  public class UserOptions
  {
    public bool DeleteLaunchJsonAfterBuild { get; set; }

    public string HostIp { get; set; }
    public int HostPort { get; set; }

    public bool LocalPlinkEnabled { get; set; }
    public string LocalPLinkPath { get; set; }
    public bool LocalSwitchLinuxDbgOutput { get; set; }

    public bool RemoteDebugDisplayGui { get; set; }
    public string RemoteDeployBasePath { get; set; }
    /// <summary>Full path to `dotnet` executable.</summary>
    public string RemoteDotNetPath { get; set; }
    /// <summary>Base path to VSDBG (i.e. `~/.vsdbg`).</summary>
    public string RemoteVsDbgBasePath { get; set; }
    /// <summary>Full path to VS Debugger.</summary>
    public string RemoteVsDbgFullPath => LinuxPath.Combine(RemoteVsDbgBasePath, Constants.VS2022, Constants.AppVSDbg);

    public bool UseCommandLineArgs { get; set; }
    public bool UsePublish { get; set; }

    public string UserGroupName { get; set; }
    public string UserName { get; set; }
    public string UserPass { get; set; }
    public bool UserPrivateKeyEnabled { get; set; }
    public string UserPrivateKeyPath { get; set; }
    public string UserPrivateKeyPassword { get; set; }

    public bool UseSSHExeEnabled { get; set; } = false;
  }
}
