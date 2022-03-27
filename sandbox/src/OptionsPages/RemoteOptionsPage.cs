using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace VSLinuxDebugger.OptionsPage
{
  public class RemoteOptionsPage : DialogPage
  {
    [Category("Remote Machine Settings")]
    [DisplayName("IP Address")]
    [Description("Remote IP Address")]
    public string IP { get; set; } = "192.168.1.205";

    [Category("Remote Machine")]
    [DisplayName("Host Port Number (22)")]
    [Description("Remote Host Port Number (SSH Default is 22)")]
    public uint HostPort { get; set; } = 22;

    [Category("Remote Machine")]
    [DisplayName("User Name")]
    [Description("Remote Machine User Name")]
    public string UserName { get; set; } = "pi";

    [Category("Remote Machine")]
    [DisplayName("User Password")]
    [Description("Remote Machine User Password")]
    public string UserPass { get; set; } = "raspberry";

    [Category("Remote Machine")]
    [DisplayName("Use SSSH Key File")]
    [Description("Use SSH Key for connecting to remote machine.")]
    public bool UseSshKeyFile { get; set; } = false;

    [Category("Remote Machine")]
    [DisplayName("Group Name")]
    [Description("Remote Machine Group Name. For RaspberryPI  you may use, 'pi'.")]
    public string GroupName { get; set; } = "";

    [Category("Remote Machine")]
    [DisplayName("Visual Studio Debugger Path")]
    [Description("Remote Machine Visual Studio Debugger Path")]
    public string VsDbgPath { get; set; } = "~/.vs-debugger/vs2022";

    ////public string VsDbgPath { get; set; } = "~/.vsdbg/vsdbg";

    [Category("Remote Machine")]
    [DisplayName(".Net Path")]
    [Description("Remote Machine .Net Path")]
    public string DotNetPath { get; set; } = "~/.dotnet/dotnet";

    [Category("Remote Machine")]
    [DisplayName("Project folder")]
    [Description("Folder for to transfer files to. For HOME folder, use './VLSDbg' and not '~/VLSDbg'")]
    public string AppFolderPath { get; set; } = $"./VSLinuxDbg"; // "VSLDebugger"
  }
}
