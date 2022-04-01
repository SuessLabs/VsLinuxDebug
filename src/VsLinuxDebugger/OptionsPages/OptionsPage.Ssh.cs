using System;
using System.ComponentModel;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  public partial class OptionsPage : DialogPage
  {
    private const string Credientials = "Remote Credientials";

    [Category(Credientials)]
    [DisplayName("Host IP Address")]
    [Description("Host IP Address. On VMs using 'NAT', set IP to '127.0.0.1' and forward Port 22. PCs and VMs 'Bridged', have their own IP.")]
    public string HostIp { get; set; } = "127.0.0.1";

    [Category(Credientials)]
    [DisplayName("Host Port Number (22)")]
    [Description("Remote Host Port Number (SSH Default is 22)")]
    public int HostPort { get; set; } = 22;

    [Category(Credientials)]
    [DisplayName("User Group Name (optional)")]
    [Description("Remote Machine Group Name. For basic setups (i.e. RaspberryPI) it's the same as UserName.")]
    public string UserGroupName { get; set; } = "";

    [Category(Credientials)]
    [DisplayName("User Name")]
    [Description("SSH User name on remote machine.")]
    public string UserName { get; set; } = "pi";

    [Category(Credientials)]
    [DisplayName("User Password")]
    [Description("SSH Password on remote machine.")]
    public string UserPass { get; set; } = "raspberry";

    [Category(Credientials)]
    [DisplayName("SSH Key File Enabled")]
    [Description("Use SSH Key for connecting to remote machine.")]
    public bool UserPrivateKeyEnabled { get; set; } = false;

    [Category(Credientials)]
    [DisplayName("SSH Private Key File (optional)")]
    [Description("Private key file.")]
    public string UserPrivateKeyPath { get; set; } = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
      ".ssh\\id_rsa");

    [Category(Credientials)]
    [DisplayName("SSH Private Key Password (optional)")]
    [Description("Private key password (only if it was set).")]
    public string UserPrivateKeyPassword { get; set; } = "";
  }
}
