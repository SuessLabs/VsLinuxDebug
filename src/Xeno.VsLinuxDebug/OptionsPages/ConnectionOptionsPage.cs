using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  public class ConnectionOptionsPage : DialogPage
  {
    public string BaseDeployPath { get; set; } = "~/VsLinuxDebug/";

    [Category("Remote Settings")]
    [DisplayName("IP Address")]
    [Description("IP Address")]
    public string IP { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 22;

    [Category("Remote Settings")]
    [DisplayName("SSH Private Key File (optional)")]
    [Description("Private key file.")]
    public string SshPrivateKey { get; set; }

    [Category("Remote Settings")]
    [DisplayName("SSH User Name")]
    [Description("User name on remote machine.")]
    public string SshUserName { get; set; } = "pi";

    [Category("Remote Settings")]
    [DisplayName("SSH Password")]
    [Description("Password on remote machine.")]
    public string SshUserPass { get; set; } = "raspberry";

    public string VsDbgPath { get; set; } = "./.vs-debugger/vs2022/vsdbg";

    [Category("Remote Settings")]
    [DisplayName(".NET Path")]
    [Description("Path to .NET on remote machine.")]
    public string DotnetPath { get; set; } = "~/.dotnet/dotnet";
  }
}
