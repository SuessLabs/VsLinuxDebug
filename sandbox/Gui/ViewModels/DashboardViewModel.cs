using System.Windows.Input;
using ReactiveUI;

namespace Gui.ViewModels;

public class DashboardViewModel : ViewModelBase
{
  public DashboardViewModel()
  {
    Title = "Dashboard View!";

    CmdBreakPoint = ReactiveCommand.Create(() =>
    {
      // Force a breakpoint
      System.Diagnostics.Debug.WriteLine("Breakpoint triggering");
      System.Diagnostics.Debugger.Break();
    });
  }

  public ICommand CmdBreakPoint { get; }
}
