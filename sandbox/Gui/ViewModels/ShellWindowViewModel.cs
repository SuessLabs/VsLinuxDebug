using ReactiveUI;

namespace Gui.ViewModels;

public class ShellWindowViewModel : ViewModelBase
{
  private ViewModelBase _currentPage;

  public ShellWindowViewModel()
  {
    Title = "Sample Avalonia - Navigation";

    Sidebar = new SidebarViewModel(NavigateTo);

    var dashboard = new DashboardViewModel();
    _currentPage = dashboard;
  }

  /// <summary>Gets the sidebar view model hosted in this shell.</summary>
  public SidebarViewModel Sidebar { get; }

  /// <summary>Gets or sets the view model currently displayed in the shell's content area.</summary>
  public ViewModelBase CurrentPage
  {
    get => _currentPage;
    set => this.RaiseAndSetIfChanged(ref _currentPage, value);
  }

  /// <summary>Switches the content area to the given view model.</summary>
  /// <param name="viewModel">View model to display.</param>
  private void NavigateTo(ViewModelBase viewModel)
  {
    CurrentPage = viewModel;
  }
}
