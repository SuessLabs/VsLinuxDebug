using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  public class BuildOptionsPage : DialogPage
  {
    [Category("Build Options")]
    [DisplayName("Use Command Line Arguments")]
    public bool DeployWithoutDebugging { get; set; } = false;

    [Category("Build Options")]
    [DisplayName("Use Publish")]
    public bool Publish { get; set; } = false;

    [Category("Build Options")]
    [DisplayName("Use Command Line Arguments")]
    public bool UseCommandLineArgs { get; set; } = false;
  }
}
