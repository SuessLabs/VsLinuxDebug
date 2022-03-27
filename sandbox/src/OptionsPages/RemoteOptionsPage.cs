using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace VSRemoteDebugger.OptionsPage
{
    public class RemoteOptionsPage : DialogPage
    {
        [Category("Remote Machine Settings")]
        [DisplayName("IP Address")]
        [Description("Remote IP Address")]
        public string IP { get; set; } = "192.168.0.10";

        //// [Category("Remote Machine Settings")]
        //// [DisplayName("Host Port Number (22)")]
        //// [Description("Remote Host Port Number (SSH Default is 22)")]
        //// public uint HostPort { get; set; } = 22;

        [Category("Remote Machine Settings")]
        [DisplayName("User Name")]
        [Description("Remote Machine User Name")]
        public string UserName { get; set; } = "pi";

        [Category("Remote Machine Settings")]
        [DisplayName("User Password")]
        [Description("Remote Machine User Password")]
        public string UserPass { get; set; } = "raspberry";

        [Category("Remote Machine Settings")]
        [DisplayName("Use SSSH Key File")]
        [Description("Use SSH Key for connecting to remote machine.")]
        public bool UseSshKeyFile { get; set; } = false;

        [Category("Remote Machine Settings")]
        [DisplayName("Group Name")]
        [Description("Remote Machine Group Name")]
        public string GroupName { get; set; } = "pi";

        [Category("Remote Machine Settings")]
        [DisplayName("Visual Studio Debugger Path")]
        [Description("Remote Machine Visual Studio Debugger Path")]
        public string VsDbgPath { get; set; } = "~/.vsdbg/vsdbg";

        [Category("Remote Machine Settings")]
        [DisplayName(".Net Path")]
        [Description("Remote Machine .Net Path")]
        public string DotnetPath { get; set; } = "~/.dotnet/dotnet";

        [Category("Remote Machine Settings")]
        [DisplayName("Project folder")]
        [Description("Master folder for the transferred files")]
        public string AppFolderPath { get; set; } = "~/VsRemoteDebugger"; //// "/var/proj";
    }
}
