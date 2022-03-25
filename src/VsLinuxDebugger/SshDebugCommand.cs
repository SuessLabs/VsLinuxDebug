using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace VsLinuxDebugger
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class SshDebugCommand
  {
    /// <summary>Command ID.</summary>
    public const int CommandId = 0x0100;

    /// <summary>Command menu group (command set GUID).</summary>
    public static readonly Guid CommandSet = new Guid("da478db6-b5f9-4b11-ab42-4e08c5d1db07");

    /// <summary>VS Package that provides this command, not null.</summary>
    private readonly AsyncPackage _package;

    private DebuggerPackage Settings => _package as DebuggerPackage;

    /// <summary>
    ///   Initializes a new instance of the <see cref="SshDebugCommand"/> class.
    ///   Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private SshDebugCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this._package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Execute, menuCommandID);
      commandService.AddCommand(menuItem);
    }

    /// <summary>Gets the instance of the command.</summary>
    public static SshDebugCommand Instance { get; private set; }

    /// <summary>Gets the service provider from the owner package.</summary>
    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
    {
      get
      {
        return this._package;
      }
    }

    /// <summary>Initializes the singleton instance of the command.</summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
      // Switch to the main thread - the call to AddCommand in SshDebugCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new SshDebugCommand(package, commandService);
    }

    /// <summary>
    ///   This function is the callback used to execute the command when the menu item is clicked.
    ///   See the constructor to see how the menu item is associated with this function using
    ///   OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void Execute(object sender, EventArgs e)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
      string title = "SshDebugCommand";

      // Show a message box to prove we were here
      VsShellUtilities.ShowMessageBox(
          this._package,
          message,
          title,
          OLEMSGICON.OLEMSGICON_INFO,
          OLEMSGBUTTON.OLEMSGBUTTON_OK,
          OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
  }
}
