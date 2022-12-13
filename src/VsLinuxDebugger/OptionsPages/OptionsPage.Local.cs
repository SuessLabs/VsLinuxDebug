using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using VsLinuxDebugger;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  public partial class OptionsPage : DialogPage
  {
    private const string Local = "Local Settings";
    private bool _autoSwitchLinuxDbgOutput = false;

    //// [Category("Build Options")]
    //// [DisplayName("Use Command Line Arguments")]
    //// public bool DeployWithoutDebugging { get; set; } = false;

    ////[Category(Experimental)]
    ////[DisplayName("Use Publish")]
    ////[Description("Publish the solution instead of building. Apply setting for ASP.NET/Blazor projects.")]
    ////public bool Publish { get; set; } = false;

    ////[Category(Local)]
    ////[DisplayName("PLink: Enable Plink instead of SSH")]
    //// <summary>
    //// [Category(Local)]
    //// </summary> to TRUE to debug with PLINK.EXE and FALSE for SSH.")]
    //// <summary>
    //// [Category(Local)]
    //// </summary>Enabled { get; set; } = false;

    [Category(Local)]
    [DisplayName("PLink Local Path (blank to use embedded)")]
    [Description(@"Full path to local PLINK.EXE file. (i.e. 'C:\temp\putty\plink.exe')")]
    public string PLinkPath { get; set; } = "";

    [Category(Local)]
    [DisplayName("Delete 'launch.json' after build.")]
    [Description(@"The `launch.json` is generated in your build folder. You may keep this for debugging.")]
    public bool DeleteLaunchJsonAfterBuild { get; set; } = false;

    [Category(Local)]
    [DisplayName("Switch to LinuxDbg Output on Build")]
    [Description("Automatically show output for Linux Debugger on build (default = false).")]
    public bool SwitchLinuxDbgOutput
    {
      get => _autoSwitchLinuxDbgOutput;
      set
      {
        Logger.AutoSwitchToLinuxDbgOutput = value;
        _autoSwitchLinuxDbgOutput = value;
      }
    }
  }
}
