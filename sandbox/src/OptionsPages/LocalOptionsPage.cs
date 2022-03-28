using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace VSLinuxDebugger.OptionsPages
{
  public class LocalOptionsPage : DialogPage
  {
    [Category("Local Machine")]
    [DisplayName("Publish")]
    [Description("Publish the solution instead of building. Apply setting for ASP.NET/Blazor projects.")]
    public bool Publish { get; set; } = false;

    [Category("Local Machine")]
    [DisplayName("Use Command Line Arguments")]
    [Description(
      "Apply command line arguments from Visual Studio Project Settings. " +
      "(Experimental : Project Settings -> Debugging -> Command Line Arguments)")]
    public bool UseCommandLineArgs { get; set; } = false;

    [Category("Local Machine")]
    [DisplayName("Display GUI")]
    [Description(
      "Display application on remote machine. This is helpful for debugging " +
      "GUI applications on remote devices.")]
    public bool DisplayInGui { get; set; } = true;

    [Category("Local Machine")]
    [DisplayName("Use PLink instead of SSH")]
    [Description("Set to TRUE to debug with PLINK.EXE and FALSE for SSH.")]
    public bool UsePLinkForDebugging { get; set; } = false;

    // TODO: Move to menu items.
    [Category("Local Machine")]
    [DisplayName("Build and deploy only.")]
    [Description("Does not perform debugging operation.")]
    public bool NoDebug { get; set; } = false;
  }
}
