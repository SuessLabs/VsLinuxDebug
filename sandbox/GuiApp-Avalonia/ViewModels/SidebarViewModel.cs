using Learn.PrismAvalonia.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Learn.PrismAvalonia.ViewModels
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

    public DelegateCommand CommandDispense => new DelegateCommand(() =>
    {
      //_eventAggregator.GetEvent<PubSubEvent<LogEvent>>().Publish(new LogEvent("Should not navigate to Dispense directly."));
      //_regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(DispenseView));
    });

    public DelegateCommand CommandLogin => new DelegateCommand(() =>
    {
      ////_eventAggregator.GetEvent<PubSubEvent<LogEvent>>().Publish(new LogEvent("Login screen not implemented"));
    });

    public DelegateCommand CommandSettings => new DelegateCommand(() =>
    {
      _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(SettingsView));
    });
  }
}
