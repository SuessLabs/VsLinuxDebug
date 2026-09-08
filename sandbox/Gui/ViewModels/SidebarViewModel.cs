using System;
using System.Windows.Input;
using ReactiveUI;

namespace Gui.ViewModels;

public class SidebarViewModel : ViewModelBase
{
  private readonly Action<ViewModelBase> _navigate;

  public SidebarViewModel(Action<ViewModelBase> navigate)
  {
    _navigate = navigate;

    Title = "Navigation";

    CmdDashboard = ReactiveCommand.Create(() => _navigate(new DashboardViewModel()));
    CmdSettings = ReactiveCommand.Create(() => _navigate(new SettingsViewModel()));
  }

  public ICommand CmdDashboard { get; }

  public ICommand CmdSettings { get; }
}
