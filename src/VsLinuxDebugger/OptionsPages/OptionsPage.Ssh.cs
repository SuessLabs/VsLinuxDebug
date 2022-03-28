using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  public partial class OptionsPage : DialogPage
  {
    private const string Credientials = "Remote Credientials";

    [Category(Credientials)]
    [DisplayName("IP Address")]
    [Description("IP Address")]
    public string HostIp { get; set; } = "127.0.0.1";

    [Category(Credientials)]
    [DisplayName("Host Port Number (22)")]
    [Description("Remote Host Port Number (SSH Default is 22)")]
    public uint HostPort { get; set; } = 22;

    [Category(Credientials)]
    [DisplayName("User Group Name (optional)")]
    [Description("Remote Machine Group Name. For RaspberryPI  you may use, 'pi'.")]
    public string SshGroupName { get; set; } = "";

    [Category(Credientials)]
    [DisplayName("Use SSH Key File")]
    [Description("Use SSH Key for connecting to remote machine.")]
    public bool SshPrivateKeyEnabled { get; set; } = false;

    [Category(Credientials)]
    [DisplayName("SSH Private Key File (optional)")]
    [Description("Private key file.")]
    public string SshPrivateKeyPath { get; set; }

    [Category(Credientials)]
    [DisplayName("User Name")]
    [Description("SSH User name on remote machine.")]
    public string UserName { get; set; } = "pi";

    [Category(Credientials)]
    [DisplayName("Password")]
    [Description("SSH Password on remote machine.")]
    public string UserPass { get; set; } = "raspberry";
  }
}
