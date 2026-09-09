using System.Windows;
using System.Windows.Controls;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>WPF UI for <see cref="RemoteLaunchOptionsPage"/>.</summary>
  public partial class RemoteLaunchOptionsControl : UserControl
  {
    private RemoteLaunchOptionsPage _page;

    public RemoteLaunchOptionsControl(RemoteLaunchOptionsPage page)
    {
      InitializeComponent();
      _page = page;
      DataContext = page;
    }

    /// <summary>Opens the shared multi-line editor for whichever field the clicked button's
    /// Tag names, so Enter can insert a newline normally -- see MultilineEditDialog's remarks.</summary>
    private void EditMultilineField_Click(object sender, RoutedEventArgs e)
    {
      var fieldName = (string)((FrameworkElement)sender).Tag;

      string title, helpText, currentText;
      switch (fieldName)
      {
        case nameof(RemoteLaunchOptionsPage.RemoteEnvironmentVariables):
          title = "Environment Variables";
          helpText = "'KEY=VALUE' pairs passed to the debuggee, one per line.";
          currentText = _page.RemoteEnvironmentVariables;
          break;

        case nameof(RemoteLaunchOptionsPage.RemotePreDeployCommands):
          title = "Pre-Deploy Commands";
          helpText = "Shell commands run before upload, one per line, i.e. 'sudo systemctl stop myapp.service'.";
          currentText = _page.RemotePreDeployCommands;
          break;

        case nameof(RemoteLaunchOptionsPage.RemotePostDeployCommands):
          title = "Post-Deploy Commands";
          helpText = "Shell commands run after upload, one per line, i.e. 'sudo systemctl start myapp.service'.";
          currentText = _page.RemotePostDeployCommands;
          break;

        default:
          return;
      }

      var dialog = new MultilineEditDialog(title, helpText, currentText)
      {
        Owner = Window.GetWindow(this),
      };

      if (dialog.ShowDialog() != true)
        return;

      switch (fieldName)
      {
        case nameof(RemoteLaunchOptionsPage.RemoteEnvironmentVariables):
          _page.RemoteEnvironmentVariables = dialog.Text;
          break;

        case nameof(RemoteLaunchOptionsPage.RemotePreDeployCommands):
          _page.RemotePreDeployCommands = dialog.Text;
          break;

        case nameof(RemoteLaunchOptionsPage.RemotePostDeployCommands):
          _page.RemotePostDeployCommands = dialog.Text;
          break;
      }
    }
  }
}
