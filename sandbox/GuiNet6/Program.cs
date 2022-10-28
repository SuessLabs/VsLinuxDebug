using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.ReactiveUI;

namespace GuiNet6
{
  internal class Program
  {
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() => AppBuilder
      .Configure<App>()
      .UsePlatformDetect()
      .With(new X11PlatformOptions
      {
        EnableMultiTouch = true,
        UseDBusMenu = true,
      })
      .With(new Win32PlatformOptions
      {
        EnableMultitouch = true,
        AllowEglInitialization = true,
      })
      .UseSkia()
      .UseReactiveUI()
      .LogToTrace();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
  }
}
