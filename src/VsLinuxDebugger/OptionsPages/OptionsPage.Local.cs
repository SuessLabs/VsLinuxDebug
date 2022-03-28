using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  public partial class OptionsPage : DialogPage
  {
    private const string Local = "Local Settings";

    //// [Category("Build Options")]
    //// [DisplayName("Use Command Line Arguments")]
    //// public bool DeployWithoutDebugging { get; set; } = false;

    [Category(Local)]
    [DisplayName("Use Publish")]
    [Description("Publish the solution instead of building. Apply setting for ASP.NET/Blazor projects.")]
    public bool Publish { get; set; } = false;

    [Category(Local)]
    [DisplayName("PLink: Enable Plink instead of SSH")]
    [Description("Set to TRUE to debug with PLINK.EXE and FALSE for SSH.")]
    public bool PLinkEnabled { get; set; } = false;

    [Category(Local)]
    [DisplayName("PLink: Local Path")]
    [Description(@"Full path to local PLINK.EXE file. (i.e. 'C:\temp\plink.exe')")]
    public string PLinkPath { get; set; } = "";
  }
}
