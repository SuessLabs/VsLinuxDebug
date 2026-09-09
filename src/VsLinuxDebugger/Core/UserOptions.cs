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

    /// <summary>When Stop cancels an in-progress build/deploy, also immediately close the SSH
    /// connection even mid-command. Default false lets the current remote step finish first.</summary>
    public bool ForceKillOnStop { get; set; } = false;

    public bool RemoteDebugDisplayGui { get; set; }
    public string RemoteDebugDisplayNumber { get; set; }
    public string RemoteDeployBasePath { get; set; }

    /// <summary>Environment variables to pass to the debuggee, one `KEY=VALUE` pair per line.</summary>
    public string RemoteEnvironmentVariables { get; set; }
    /// <summary>Shell commands run on the remote machine before files are uploaded, one per
    /// line, only when Deploy runs (i.e. stopping whatever supervises/holds the debuggee).</summary>
    public string RemotePreDeployCommands { get; set; }

    /// <summary>Shell commands run on the remote machine after files are uploaded, one per
    /// line, only when Deploy runs (i.e. restarting a supervisor so it picks up the new build).</summary>
    public string RemotePostDeployCommands { get; set; }

    /// <summary>When enabled, the debuggee is presumed to be started/supervised externally, so
    /// debugging attaches to its existing process (resolved via <see cref="RemotePidCommand"/>)
    /// instead of vsdbg launching a new one.</summary>
    public bool AttachToRunningProcess { get; set; } = false;

    /// <summary>Shell command, run on the remote machine, whose output is the PID to attach to.
    /// Only used when <see cref="AttachToRunningProcess"/> is enabled.</summary>
    public string RemotePidCommand { get; set; }
    /// <summary>Base path to VSDBG (i.e. `~/.vsdbg`).</summary>
    public string RemoteVsDbgBasePath { get; set; }
    /// <summary>Full path to VS Debugger.</summary>
    public string RemoteVsDbgFullPath => LinuxPath.Combine(RemoteVsDbgBasePath, Constants.VS2022, Constants.AppVSDbg);

    public bool UsePublish { get; set; }

    /// <summary>When enabled, launches the deployed program directly as a native executable
    /// (i.e. a self-contained/AOT publish) instead of via `dotnet &lt;assembly&gt;.dll`.</summary>
    public bool UseSelfContainedDeployment { get; set; } = false;

    /// <summary>.NET Runtime Identifier to publish for when <see cref="UseSelfContainedDeployment"/>
    /// is enabled (i.e. `linux-arm64`).</summary>
    public string RemoteRuntimeIdentifier { get; set; } = "linux-arm64";

    public string UserGroupName { get; set; }
    public string UserName { get; set; }
    public string UserPass { get; set; }
    public bool UserPrivateKeyEnabled { get; set; }
    public string UserPrivateKeyPath { get; set; }
    public string UserPrivateKeyPassword { get; set; }

    /// <summary>Explicit path to an OpenSSH certificate file (i.e. a CA-signed
    /// '&lt;key&gt;-cert.pub'). When blank, the certificate is looked up next to
    /// <see cref="UserPrivateKeyPath"/> using the OpenSSH naming convention.</summary>
    public string UserCertificatePath { get; set; }

    public bool UseSSHExeEnabled { get; set; } = false;

    /// <summary>Command used to elevate the debugger process on the remote machine (i.e. `sudo -n -E`).</summary>
    public string SudoCommand { get; set; } = Constants.DefaultSudoCommand;

    /// <summary>When enabled, launches VSDBG via <see cref="SudoCommand"/> on the remote machine.
    /// Use this when the debuggee runs with elevated/ambient capabilities the debugger must match to attach.</summary>
    public bool UseSudoForDebugger { get; set; } = false;
  }
}
