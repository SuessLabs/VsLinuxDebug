using Microsoft.CodeAnalysis;
using Prism.Commands;

namespace GuiNet6.ViewModels;

public class DashboardViewModel : ViewModelBase
{
  public DashboardViewModel()
  {
    Title = "Dashboard View!";
  }

  public DelegateCommand CmdBreakPoint => new(() =>
  {
    // Force a breakpoint
    System.Diagnostics.Debug.WriteLine("Breakpoint triggering");
    System.Diagnostics.Debugger.Break();
  });
}
