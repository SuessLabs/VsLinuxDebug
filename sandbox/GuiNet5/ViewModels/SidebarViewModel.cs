using GuiNet5.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace GuiNet5.ViewModels
{
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

    public DelegateCommand CmdDashboard => new DelegateCommand(() =>
    {
      _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(DashboardView));
    });

    public DelegateCommand CmdSettings => new DelegateCommand(() =>
    {
      _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(SettingsView));
    });
  }
}
