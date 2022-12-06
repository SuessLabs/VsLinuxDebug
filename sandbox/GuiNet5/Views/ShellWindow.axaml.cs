using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GuiNet5.Views
{
  public partial class ShellWindow : Window
  {
    public ShellWindow()
    {
      InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
