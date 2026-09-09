using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gui.Views;

public partial class ShellWindow : Window
{
  public ShellWindow()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}
