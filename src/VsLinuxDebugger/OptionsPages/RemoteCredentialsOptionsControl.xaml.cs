using System.Windows.Controls;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>WPF UI for <see cref="RemoteCredentialsOptionsPage"/>. Binds directly to the
  /// page instance; the password vs. private-key rows are shown/hidden via visibility
  /// bindings on <see cref="RemoteCredentialsOptionsPage.UserPrivateKeyEnabled"/>.</summary>
  public partial class RemoteCredentialsOptionsControl : UserControl
  {
    public RemoteCredentialsOptionsControl(RemoteCredentialsOptionsPage page)
    {
      InitializeComponent();
      DataContext = page;
    }
  }
}
