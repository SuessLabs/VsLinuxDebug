using System.ComponentModel;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using VsLinuxDebugger.Core;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>How the deployed debuggee is started, attached to, and displayed.</summary>
  public class RemoteLaunchOptionsPage : UIElementDialogPage, INotifyPropertyChanged
  {
    private string _remoteEnvironmentVariables = string.Empty;
    private string _remotePreDeployCommands = string.Empty;
    private string _remotePostDeployCommands = string.Empty;
    private bool _attachToRunningProcess = false;
    private string _remotePidCommand = string.Empty;
    private bool _useSudoForDebugger = false;
    private string _sudoCommand = Constants.DefaultSudoCommand;
    private bool _remoteDebugDisplayGui = false;
    private string _remoteDebugDisplayNumber = ":0";

    public event PropertyChangedEventHandler PropertyChanged;

    [Category("Remote Launch")]
    [DisplayName("Environment Variables")]
    [Description("'KEY=VALUE' pairs passed to the debuggee, one per line.")]
    public string RemoteEnvironmentVariables
    {
      get => _remoteEnvironmentVariables;
      set { _remoteEnvironmentVariables = value; OnPropertyChanged(nameof(RemoteEnvironmentVariables)); }
    }

    [Category("Remote Launch")]
    [DisplayName("Pre-Deploy Commands")]
    [Description("Shell commands run before upload, one per line, i.e. 'sudo systemctl stop myapp.service'.")]
    public string RemotePreDeployCommands
    {
      get => _remotePreDeployCommands;
      set { _remotePreDeployCommands = value; OnPropertyChanged(nameof(RemotePreDeployCommands)); }
    }

    [Category("Remote Launch")]
    [DisplayName("Post-Deploy Commands")]
    [Description("Shell commands run after upload, one per line, i.e. 'sudo systemctl start myapp.service'.")]
    public string RemotePostDeployCommands
    {
      get => _remotePostDeployCommands;
      set { _remotePostDeployCommands = value; OnPropertyChanged(nameof(RemotePostDeployCommands)); }
    }

    [Category("Remote Launch")]
    [DisplayName("Attach to Already-Running Process")]
    [Description("Attach to the process started by the commands above instead of vsdbg launching a new one.")]
    public bool AttachToRunningProcess
    {
      get => _attachToRunningProcess;
      set { _attachToRunningProcess = value; OnPropertyChanged(nameof(AttachToRunningProcess)); }
    }

    [Category("Remote Launch")]
    [DisplayName("PID Command")]
    [Description("Shell command whose output is the PID to attach to, i.e. 'pgrep -f myapp'.")]
    public string RemotePidCommand
    {
      get => _remotePidCommand;
      set { _remotePidCommand = value; OnPropertyChanged(nameof(RemotePidCommand)); }
    }

    [Category("Remote Launch")]
    [DisplayName("Use Sudo for Debugger")]
    [Description("Launch vsdbg elevated. Needed when the debuggee has ambient capabilities vsdbg must match to attach.")]
    public bool UseSudoForDebugger
    {
      get => _useSudoForDebugger;
      set { _useSudoForDebugger = value; OnPropertyChanged(nameof(UseSudoForDebugger)); }
    }

    [Category("Remote Launch")]
    [DisplayName("Sudo Command")]
    [Description("Elevation command used when 'Use Sudo for Debugger' is enabled. (Default: `sudo -n`)")]
    public string SudoCommand
    {
      get => _sudoCommand;
      set { _sudoCommand = value; OnPropertyChanged(nameof(SudoCommand)); }
    }

    [Category("Remote Launch")]
    [DisplayName("Debug Display GUI")]
    [Description("Show the app's window on the remote machine's X11 display.")]
    public bool RemoteDebugDisplayGui
    {
      get => _remoteDebugDisplayGui;
      set { _remoteDebugDisplayGui = value; OnPropertyChanged(nameof(RemoteDebugDisplayGui)); }
    }

    [Category("Remote Launch")]
    [DisplayName("X11 Display Number (optional)")]
    [Description("Defaults to ':0'.")]
    public string RemoteDebugDisplayNumber
    {
      get => _remoteDebugDisplayNumber;
      set { _remoteDebugDisplayNumber = value; OnPropertyChanged(nameof(RemoteDebugDisplayNumber)); }
    }

    protected override UIElement Child => new RemoteLaunchOptionsControl(this);

    private void OnPropertyChanged(string propertyName)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
