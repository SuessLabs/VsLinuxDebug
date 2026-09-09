using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Xeno.VsLinuxDebug.OptionsPages;
using Task = System.Threading.Tasks.Task;

namespace VsLinuxDebugger
{
  /// <summary>This is the class that implements the package exposed by this assembly.</summary>
  /// <remarks>
  ///   <para>
  ///     The minimum requirement for a class to be considered a valid package for Visual Studio
  ///     is to implement the IVsPackage interface and register itself with the shell.
  ///     This package uses the helper classes defined inside the Managed Package Framework (MPF)
  ///     to do it: it derives from the Package class that provides the implementation of the
  ///     IVsPackage interface and uses the registration attributes defined in the framework to
  ///     register itself and its components with the shell. These attributes tell the pkgdef creation
  ///     utility what data to put into .pkgdef file.
  ///   </para>
  ///   <para>
  ///     To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
  ///   </para>
  /// </remarks>
  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
  [Guid(DebuggerPackage.PackageGuidString)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [ProvideOptionPage(typeof(RemoteHostOptionsPage), "Linux Debugger", "Remote Host", 0, 0, true, Sort = 1)]
  [ProvideOptionPage(typeof(RemoteCredentialsOptionsPage), "Linux Debugger", "Remote Credentials", 0, 0, true, Sort = 2)]
  [ProvideOptionPage(typeof(RemoteDebuggerOptionsPage), "Linux Debugger", "Remote Debugger", 0, 0, true, Sort = 3)]
  [ProvideOptionPage(typeof(RemoteLaunchOptionsPage), "Linux Debugger", "Remote Launch", 0, 0, true, Sort = 4)]
  [ProvideOptionPage(typeof(LocalOptionsPage), "Linux Debugger", "Local", 0, 0, true, Sort = 5)]
  public sealed partial class DebuggerPackage : AsyncPackage
  {
    /// <summary>Package GUID string.</summary>
    public const string PackageGuidString = "19f87f23-7a2c-4279-ac7c-c9267776bbf9";

    public RemoteHostOptionsPage RemoteHostOptions => (RemoteHostOptionsPage)GetDialogPage(typeof(RemoteHostOptionsPage));

    public RemoteCredentialsOptionsPage RemoteCredentialsOptions => (RemoteCredentialsOptionsPage)GetDialogPage(typeof(RemoteCredentialsOptionsPage));

    public RemoteDebuggerOptionsPage RemoteDebuggerOptions => (RemoteDebuggerOptionsPage)GetDialogPage(typeof(RemoteDebuggerOptionsPage));

    public RemoteLaunchOptionsPage RemoteLaunchOptions => (RemoteLaunchOptionsPage)GetDialogPage(typeof(RemoteLaunchOptionsPage));

    public LocalOptionsPage LocalOptions => (LocalOptionsPage)GetDialogPage(typeof(LocalOptionsPage));

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
      await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
      await Commands.InitializeAsync(this);

      Logger.Init(this, OutputWindowType.Custom, LocalOptions.SwitchLinuxDbgOutput);
      Logger.Output("InitializeAsync");
    }
  }
}
