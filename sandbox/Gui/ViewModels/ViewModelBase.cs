using ReactiveUI;

namespace Gui.ViewModels;

public class ViewModelBase : ReactiveObject
{
  private string _title = string.Empty;

  /// <summary>Gets or sets the title of the View.</summary>
  public string Title
  {
    get => _title;
    set => this.RaiseAndSetIfChanged(ref _title, value);
  }
}
