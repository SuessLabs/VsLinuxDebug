using System.ComponentModel;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using VsLinuxDebugger;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>Local-machine settings.</summary>
  public class LocalOptionsPage : UIElementDialogPage, INotifyPropertyChanged
  {
    private bool _useSSHExeEnabled = true;
    private string _plinkPath = "";
    private bool _deleteLaunchJsonAfterBuild = false;
    private bool _autoSwitchLinuxDbgOutput = true;
    private bool _forceKillOnStop = false;

    public event PropertyChangedEventHandler PropertyChanged;

    [Category("Local Settings")]
    [DisplayName("Use SSH.exe instead of PLINK")]
    [Description("Use SSH.exe (supports OpenSSH keys directly) instead of PLINK (needs a '.ppk' key).")]
    public bool UseSSHExeEnabled
    {
      get => _useSSHExeEnabled;
      set { _useSSHExeEnabled = value; OnPropertyChanged(nameof(UseSSHExeEnabled)); }
    }

    [Category("Local Settings")]
    [DisplayName("PLink Local Path (blank to use embedded)")]
    [Description("Full path to local PLINK.EXE, used when 'Use SSH.exe' is unchecked.")]
    public string PLinkPath
    {
      get => _plinkPath;
      set { _plinkPath = value; OnPropertyChanged(nameof(PLinkPath)); }
    }

    [Category("Local Settings")]
    [DisplayName("Delete 'launch.json' after build.")]
    [Description(@"The `launch.json` is generated in your build folder. You may keep this for debugging.")]
    public bool DeleteLaunchJsonAfterBuild
    {
      get => _deleteLaunchJsonAfterBuild;
      set { _deleteLaunchJsonAfterBuild = value; OnPropertyChanged(nameof(DeleteLaunchJsonAfterBuild)); }
    }

    [Category("Local Settings")]
    [DisplayName("Switch to LinuxDbg Output on Build")]
    [Description("Automatically show output for Linux Debugger on build (default = true).")]
    public bool SwitchLinuxDbgOutput
    {
      get => _autoSwitchLinuxDbgOutput;
      set
      {
        Logger.AutoSwitchToLinuxDbgOutput = value;
        _autoSwitchLinuxDbgOutput = value;
        OnPropertyChanged(nameof(SwitchLinuxDbgOutput));
      }
    }

    [Category("Local Settings")]
    [DisplayName("Force kill on Stop (interrupt mid-command)")]
    [Description("When Stop is used to cancel an in-progress build/deploy, also immediately " +
      "close the SSH connection even if a remote command is currently running (default = false, " +
      "which instead lets the current step finish before stopping, avoiding a half-uploaded or " +
      "half-restarted remote state).")]
    public bool ForceKillOnStop
    {
      get => _forceKillOnStop;
      set { _forceKillOnStop = value; OnPropertyChanged(nameof(ForceKillOnStop)); }
    }

    protected override UIElement Child => new LocalOptionsControl(this);

    private void OnPropertyChanged(string propertyName)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
