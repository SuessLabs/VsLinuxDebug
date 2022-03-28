using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using VSLinuxDebugger.OptionsPage;
using VSLinuxDebugger.OptionsPages;
using Task = System.Threading.Tasks.Task;

namespace VSLinuxDebugger
{
  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the
  /// IVsPackage interface and uses the registration attributes defined in the framework to
  /// register itself and its components with the shell. These attributes tell the pkgdef creation
  /// utility what data to put into .pkgdef file.
  /// </para>
  /// <para>
  /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
  /// </para>
  /// </remarks>
  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
  [Guid(PackageGuidString)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [ProvideOptionPage(typeof(RemoteOptionsPage), "Linux Debugger", "Remote Machine", 0, 0, true)]
  [ProvideOptionPage(typeof(LocalOptionsPage), "Linux Debugger", "Local Machine", 0, 0, true)]
  public sealed class VSLinuxDebuggerPackage : AsyncPackage
  {
    private RemoteOptionsPage RemotePage => (RemoteOptionsPage)GetDialogPage(typeof(RemoteOptionsPage));

    private LocalOptionsPage LocalPage => (LocalOptionsPage)GetDialogPage(typeof(LocalOptionsPage));

    public string IP => RemotePage.IP;

    public int HostPort => 22;

    public string UserName => RemotePage.UserName;

    public string UserPass => RemotePage.UserPass;

    public string GroupName => RemotePage.GroupName;

    public bool UseSshKeyFile => RemotePage.UseSshKeyFile;

    public bool UsePlinkForDebugging => LocalPage.UsePLinkForDebugging;

    public string VsDbgPath => RemotePage.VsDbgPath;
    public string DotnetPath => RemotePage.DotNetPath;
    public string AppFolderPath => RemotePage.AppFolderPath;
    public string DebugFolderPath => $"{AppFolderPath}/TMP";  //// AppFolderPath + "/debug";
    public string ReleaseFolderPath => $"{AppFolderPath}/TMP"; //// AppFolderPath + "/release";
    public bool Publish => LocalPage.Publish;
    public bool UseCommandLineArgs => LocalPage.UseCommandLineArgs;
    public bool NoDebug => LocalPage.NoDebug;

    /// <summary>
    /// VSLinuxDebuggerPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "27375819-9dd0-4292-a612-2300250b06b8";

    #region Package Members

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      // When initialized asynchronously, the current thread may be a background thread at this point.
      // Do any initialization that requires the UI thread after switching to the UI thread.
      await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
      await RemoteDebugCommand.InitializeAsync(this).ConfigureAwait(false);
    }

    #endregion Package Members
  }
}
