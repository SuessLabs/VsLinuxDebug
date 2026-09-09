using System.Windows;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>Generic modal editor for a multi-line string field (env vars, pre/post-deploy
  /// commands). A real top-level WPF Window, not hosted by UIElementDialogPage's native Win32
  /// dialog -- so Enter inserts a newline normally instead of being intercepted by
  /// IsDialogMessage and triggering the default button (which the Options page's own
  /// multi-line TextBoxes can't avoid, since that interception happens at the native message
  /// level before WPF ever sees the keystroke).</summary>
  public partial class MultilineEditDialog : Window
  {
    public string Text { get; private set; }

    public MultilineEditDialog(string title, string helpText, string text)
    {
      InitializeComponent();

      Title = title;
      HelpTextBlock.Text = helpText;
      Text = text ?? string.Empty;

      EditTextBox.Text = Text;
      Loaded += (s, e) =>
      {
        EditTextBox.Focus();
        EditTextBox.CaretIndex = EditTextBox.Text.Length;
      };
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
      Text = EditTextBox.Text;
      DialogResult = true;
    }
  }
}
