using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using VsLinuxDebugger.Core;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  public partial class OptionsPage : DialogPage
  {
    private const string Experimental = "Warning Experimental";
    private const string RemoteDebugger = "Remote Debugger";

    [Category(Experimental)]
    [DisplayName("Debug Display GUI")]
    [Description(
      "Display application on remote machine. This is helpful for debugging " +
      "GUI applications on remote devices.")]
    public bool RemoteDebugDisplayGui { get; set; } = false;

    [Category(RemoteDebugger)]
    [DisplayName("Upload to folder")]
    [Description("Folder for to transfer files to. For HOME folder, use './VSLinuxDbg' and not '~/VSLinuxDbg'")]
    public string RemoteDeployBasePath { get; set; } = $"./VSLinuxDbg"; // "LinuxDbg"

    [Category(RemoteDebugger)]
    [DisplayName(".NET executable")]
    [Description("Path of the .NET executable on remote machine. (Samples: `dotnet`, `~/.dotnet/dotnet`)")]
    public string RemoteDotNetPath { get; set; } = Constants.DefaultDotNetPath;

    [Category(RemoteDebugger)]
    [DisplayName("Visual Studio Debugger Path")]
    [Description(
      "Root folder of Visual Studio Debugger. " +
      "(Samples: `~/.vs-debugger/`, `~/.vsdbg`)")]
    public string RemoteVsDbgRootPath { get; set; } = Constants.DefaultVsdbgBasePath;

    [Category(Experimental)]
    [DisplayName("Use Command Line Arguments")]
    [Description(
      "Apply command line arguments from Visual Studio Project Settings. " +
      "(Experimental : Project Settings -> Debugging -> Command Line Arguments)")]
    public bool UseCommandLineArgs { get; set; } = false;
  }
}
