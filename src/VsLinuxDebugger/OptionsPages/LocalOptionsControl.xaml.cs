using System.Windows.Controls;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>WPF UI for <see cref="LocalOptionsPage"/>.</summary>
  public partial class LocalOptionsControl : UserControl
  {
    public LocalOptionsControl(LocalOptionsPage page)
    {
      InitializeComponent();
      DataContext = page;
    }
  }
}
