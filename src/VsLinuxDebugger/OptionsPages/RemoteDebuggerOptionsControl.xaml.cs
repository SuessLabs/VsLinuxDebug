using System.Windows.Controls;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>WPF UI for <see cref="RemoteDebuggerOptionsPage"/>.</summary>
  public partial class RemoteDebuggerOptionsControl : UserControl
  {
    public RemoteDebuggerOptionsControl(RemoteDebuggerOptionsPage page)
    {
      InitializeComponent();
      DataContext = page;
    }
  }
}
