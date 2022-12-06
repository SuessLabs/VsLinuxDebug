using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GuiNet5.Views
{
  public partial class SettingsView : UserControl
  {
    public SettingsView()
    {
      InitializeComponent();
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
