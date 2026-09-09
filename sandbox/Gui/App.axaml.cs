using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Gui.ViewModels;
using Gui.Views;

namespace Gui;

public class App : Application
{
  /// <summary>App entry point.</summary>
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
#if DEBUG
    this.AttachDeveloperTools();
#endif
  }

  /// <summary>Called once the Avalonia framework has finished initializing.</summary>
  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      // Compose views/view-models directly - no DI container, no region manager.
      var shellViewModel = new ShellWindowViewModel();
      var shellWindow = new ShellWindow
      {
        DataContext = shellViewModel,
      };

      desktop.MainWindow = shellWindow;
    }

    base.OnFrameworkInitializationCompleted();
  }
}
