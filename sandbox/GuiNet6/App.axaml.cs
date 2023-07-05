using Avalonia;
using Avalonia.Markup.Xaml;
using GuiNet6.ViewModels;
using GuiNet6.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace GuiNet6;

public class App : PrismApplication
{
  /// <summary>App entry point.</summary>
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
    base.Initialize();
  }

  /// <summary>Prism Module Registration.</summary>
  /// <remarks><![CDATA[https://prismlibrary.com/docs/modules.html]]></remarks>
  /// <param name="moduleCatalog">Module Catalog.</param>
  protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
  {
    base.ConfigureModuleCatalog(moduleCatalog);

    // Wire-up modules for Region Manager
    //// moduleCatalog.AddModule<UserLoginModule>();
  }

  /// <summary>User interface entry point, called after Register and ConfigureModules.</summary>
  /// <returns>Startup View.</returns>
  protected override IAvaloniaObject CreateShell()
  {
    return this.Container.Resolve<ShellWindow>();
  }

  /// <summary>Called after Initialize.</summary>
  protected override void OnInitialized()
  {
    // Register Views to Region it will appear in. Don't register them in the ViewModel.
    var regionManager = Container.Resolve<IRegionManager>();
    regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(DashboardView));
    regionManager.RegisterViewWithRegion(RegionNames.SidebarRegion, typeof(SidebarView));
  }

  /// <summary>Register views and Services.</summary>
  /// <param name="containerRegistry">IOC Container.</param>
  protected override void RegisterTypes(IContainerRegistry containerRegistry)
  {
    // Services
    // ...

    // Views - Generic views
    containerRegistry.Register<ShellWindow>();
    containerRegistry.Register<SidebarView>();

    // Views - Region Navigation
    containerRegistry.RegisterForNavigation<DashboardView, DashboardViewModel>();
    containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
  }
}
