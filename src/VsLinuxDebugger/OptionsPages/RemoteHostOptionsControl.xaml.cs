using System.Windows.Controls;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>WPF UI for <see cref="RemoteHostOptionsPage"/>. Binds directly to the page
  /// instance, which owns persistence via <see cref="Microsoft.VisualStudio.Shell.DialogPage"/>'s
  /// existing reflection-based Load/SaveSettingsToStorage.</summary>
  public partial class RemoteHostOptionsControl : UserControl
  {
    public RemoteHostOptionsControl(RemoteHostOptionsPage page)
    {
      InitializeComponent();
      DataContext = page;
    }
  }
}
