using GuiNet6.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace GuiNet6.ViewModels;

public class SidebarViewModel : ViewModelBase
{
  private IEventAggregator _eventAggregator;
  private IRegionManager _regionManager;

  public SidebarViewModel(IRegionManager regionManager, IEventAggregator ea)
  {
    _regionManager = regionManager;
    _eventAggregator = ea;

    Title = "Navigation";
  }

  public DelegateCommand CmdDashboard => new(() =>
  {
    _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(DashboardView));
  });

  public DelegateCommand CmdSettings => new(() =>
  {
    _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(SettingsView));
  });
}
