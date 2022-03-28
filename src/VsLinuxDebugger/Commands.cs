using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace VsLinuxDebugger
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed partial class Commands
  {
    /// <summary>Command ID.</summary>
    public const int CommandId = 0x0100;

    /// <summary>Command menu group (command set GUID).</summary>
    public static readonly Guid CommandSet = new Guid("da478db6-b5f9-4b11-ab42-4e08c5d1db07");

    /// <summary>VS Package that provides this command, not null.</summary>
    private readonly AsyncPackage _package;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Commands"/> class.
    ///   Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private Commands(AsyncPackage package, OleMenuCommandService commandService)
    {
      _package = package ?? throw new ArgumentNullException(nameof(package));
      //// _vsExtension = IVsPackageExtensionProvider ?? throw new ArgumentNullException(nameof(vsExtension));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      InstallMenu(commandService);

      //// var menuCommandID = new CommandID(CommandSet, CommandId);
      //// var menuItem = new MenuCommand(this.Execute, menuCommandID);
      //// commandService.AddCommand(menuItem);
    }

    /// <summary>Gets the instance of the command.</summary>
    public static Commands Instance { get; private set; }

    /// <summary>Gets the service provider from the owner package.</summary>
    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this._package;

    private DebuggerPackage Settings => _package as DebuggerPackage;

    /// <summary>Initializes the singleton instance of the command.</summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
      // Switch to the main thread - the call to AddCommand in SshDebugCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new Commands(package, commandService);
    }

    private OleMenuCommand AddMenuItem(OleMenuCommandService mcs, int cmdCode, EventHandler check, EventHandler action)
    {
      var commandID = new CommandID(CommandSet, cmdCode);
      var menuCommand = new OleMenuCommand(action, commandID);
      if (check != null)
        menuCommand.BeforeQueryStatus += check;

      mcs.AddCommand(menuCommand);

      return menuCommand;
    }

    /////// <summary>
    ///////   This function is the callback used to execute the command when the menu item is clicked.
    ///////   See the constructor to see how the menu item is associated with this function using
    ///////   OleMenuCommandService service and MenuCommand class.
    /////// </summary>
    /////// <param name="sender">Event sender.</param>
    /////// <param name="e">Event args.</param>
    ////private void Execute(object sender, EventArgs e)
    ////{
    ////  ThreadHelper.ThrowIfNotOnUIThread();
    ////  string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
    ////  string title = "SshDebugCommand";
    ////
    ////  // Show a message box to prove we were here
    ////  VsShellUtilities.ShowMessageBox(
    ////      this._package,
    ////      message,
    ////      title,
    ////      OLEMSGICON.OLEMSGICON_INFO,
    ////      OLEMSGBUTTON.OLEMSGBUTTON_OK,
    ////      OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    ////}

    private sealed class CommandIds
    {
      public const int CmdDebugOnly = 0x1003;
      public const int CmdDeployAndDebug = 0x1001;
      public const int CmdDeployOnly = 0x1002;
      public const int CmdShowLog = 0x1004;
      public const int CmdShowSettings = 0x1005;
      public const int LinuxRemoteMainMenu = 0x1000;
      public const int RemoteMainMenuGroupLevel1 = 0x1100;
      public const int RemoteMainMenuGroupLevel2 = 0x1200;
    }
  }
}
