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
  [ProvideOptionPage(typeof(OptionsPage), "Linux Debugger", "General", 0, 0, true)]
  public sealed partial class DebuggerPackage : AsyncPackage
  {
    /// <summary>Package GUID string.</summary>
    public const string PackageGuidString = "19f87f23-7a2c-4279-ac7c-c9267776bbf9";

    public bool DeleteLaunchJsonAfterBuild => _optionsPage.DeleteLaunchJsonAfterBuild;

    public string HostIp => _optionsPage.HostIp;
    public int HostPort => _optionsPage.HostPort;

    public string LocalPLinkPath => _optionsPage.PLinkPath;

    public bool RemoteDebugDisplayGui => _optionsPage.RemoteDebugDisplayGui;
    public string RemoteDeployBasePath => _optionsPage.RemoteDeployBasePath;
    public string RemoteDotNetPath => _optionsPage.RemoteDotNetPath;
    public string RemoteVsDbgPath => _optionsPage.RemoteVsDbgPath;

    public bool UseCommandLineArgs => _optionsPage.UseCommandLineArgs;
    //// public bool UsePublish => _optionsPage.Publish;

    public string UserGroupName => _optionsPage.UserGroupName;
    public string UserName => _optionsPage.UserName;
    public string UserPass => _optionsPage.UserPass;
    public bool UserPrivateKeyEnabled => _optionsPage.UserPrivateKeyEnabled;
    public string UserPrivateKeyPath => _optionsPage.UserPrivateKeyPath;
    public string UserPrivateKeyPassword => _optionsPage.UserPrivateKeyPassword;

    private OptionsPage _optionsPage => (OptionsPage)GetDialogPage(typeof(OptionsPage));

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
      await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
      await Commands.InitializeAsync(this);

      Logger.Init(this, OutputWindowType.Custom);
      Logger.Output("InitializeAsync");
    }

    #endregion Package Members
  }
}
