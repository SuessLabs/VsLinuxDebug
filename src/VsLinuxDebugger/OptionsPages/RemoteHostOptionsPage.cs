using System.ComponentModel;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>Remote host connection endpoint (IP, port, group).</summary>
  public class RemoteHostOptionsPage : UIElementDialogPage, INotifyPropertyChanged
  {
    private string _hostIp = "127.0.0.1";
    private int _hostPort = 22;
    private string _userGroupName = "";

    public event PropertyChangedEventHandler PropertyChanged;

    [Category("Remote Host")]
    [DisplayName("Host IP Address")]
    [Description("Host IP Address. On VMs using 'NAT', set IP to '127.0.0.1' and forward Port 22. PCs and VMs 'Bridged', have their own IP.")]
    public string HostIp
    {
      get => _hostIp;
      set { _hostIp = value; OnPropertyChanged(nameof(HostIp)); }
    }

    [Category("Remote Host")]
    [DisplayName("Host Port Number")]
    [Description("Remote Host Port Number (SSH Default is 22)")]
    public int HostPort
    {
      get => _hostPort;
      set { _hostPort = value; OnPropertyChanged(nameof(HostPort)); }
    }

    [Category("Remote Host")]
    [DisplayName("User Group Name (optional)")]
    [Description("Remote Machine Group Name. For basic setups (i.e. RaspberryPI) it's the same as UserName.")]
    public string UserGroupName
    {
      get => _userGroupName;
      set { _userGroupName = value; OnPropertyChanged(nameof(UserGroupName)); }
    }

    protected override UIElement Child => new RemoteHostOptionsControl(this);

    private void OnPropertyChanged(string propertyName)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
