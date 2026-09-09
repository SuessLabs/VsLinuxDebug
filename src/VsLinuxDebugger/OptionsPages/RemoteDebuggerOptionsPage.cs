using System.ComponentModel;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using VsLinuxDebugger.Core;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>Where and how the project is deployed to the remote machine.</summary>
  public class RemoteDebuggerOptionsPage : UIElementDialogPage, INotifyPropertyChanged
  {
    private string _remoteDeployBasePath = "./VSLinuxDbg";
    private string _remoteVsDbgRootPath = Constants.DefaultVsdbgBasePath;
    private bool _useSelfContainedDeployment = false;
    private string _remoteRuntimeIdentifier = "linux-arm64";

    public event PropertyChangedEventHandler PropertyChanged;

    [Category("Remote Debugger")]
    [DisplayName("Upload to folder")]
    [Description("Folder files are deployed to, as-is. For HOME folder, use './VSLinuxDbg' and not '~/VSLinuxDbg'.")]
    public string RemoteDeployBasePath
    {
      get => _remoteDeployBasePath;
      set { _remoteDeployBasePath = value; OnPropertyChanged(nameof(RemoteDeployBasePath)); }
    }

    [Category("Remote Debugger")]
    [DisplayName("Visual Studio Debugger Path")]
    [Description("Root folder of Visual Studio Debugger. (Samples: `~/.vs-debugger/`, `~/.vsdbg`)")]
    public string RemoteVsDbgRootPath
    {
      get => _remoteVsDbgRootPath;
      set { _remoteVsDbgRootPath = value; OnPropertyChanged(nameof(RemoteVsDbgRootPath)); }
    }

    [Category("Remote Debugger")]
    [DisplayName("Use Self-Contained Deployment")]
    [Description("Publish and deploy a self-contained native executable instead of a plain 'dotnet <assembly>.dll' build.")]
    public bool UseSelfContainedDeployment
    {
      get => _useSelfContainedDeployment;
      set { _useSelfContainedDeployment = value; OnPropertyChanged(nameof(UseSelfContainedDeployment)); }
    }

    [Category("Remote Debugger")]
    [DisplayName("Remote Runtime Identifier")]
    [Description(".NET Runtime Identifier to publish for when Self-Contained Deployment is enabled, i.e. `linux-arm64`.")]
    public string RemoteRuntimeIdentifier
    {
      get => _remoteRuntimeIdentifier;
      set { _remoteRuntimeIdentifier = value; OnPropertyChanged(nameof(RemoteRuntimeIdentifier)); }
    }

    protected override UIElement Child => new RemoteDebuggerOptionsControl(this);

    private void OnPropertyChanged(string propertyName)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
