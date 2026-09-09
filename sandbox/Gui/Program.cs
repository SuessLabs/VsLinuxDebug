using System.Diagnostics.CodeAnalysis;
using Avalonia;
using ReactiveUI.Avalonia;

namespace Gui;

internal class Program
{
  // Avalonia configuration, don't remove; also used by visual designer.
  public static AppBuilder BuildAvaloniaApp() => AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .With(new X11PlatformOptions
    {
      EnableMultiTouch = true,
      UseDBusMenu = false,
    })
    .With(new Win32PlatformOptions
    {
      RenderingMode = [Win32RenderingMode.AngleEgl, Win32RenderingMode.Software],
    })
    .UseSkia()
    .UseReactiveUI(rxui => { })
    .LogToTrace();

  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.
  [ExcludeFromCodeCoverage]
  public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
}
