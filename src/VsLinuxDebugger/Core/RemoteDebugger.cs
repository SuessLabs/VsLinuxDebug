using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsLinuxDebugger.Extensions;

namespace VsLinuxDebugger.Core
{
  public class RemoteDebugger : IDisposable
  {
    private const string DebugAdapterHost = "DebugAdapterHost.Launch";
    private const string DebugAdapterHostLogging = "DebugAdapterHost.Logging";
    private const string DebugAdapterHostLoggingOnOutputWindow = "/On /OutputWindow"; 
    private const string DebugAdapterLaunchJson = "/LaunchJson:";

    private bool _buildSuccessful;
    private TaskCompletionSource<bool> _buildTask = null;
    private DTE _dte;
    private LaunchBuilder _launchBuilder;
    private string _launchJsonPath = string.Empty;
    private UserOptions _options;
    //// private SshTool _ssh;

    public RemoteDebugger(UserOptions options)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      _options = options;
      _dte = (DTE)Package.GetGlobalService(typeof(DTE));
      _dte.Events.BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;
      _dte.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
    }

    public static BuildEvents BuildEvents { get; set; }

    /// <summary>Perform operation.</summary>
    /// <param name="buildOptions">Build options.</param>
    /// <returns>True on success.</returns>
    public async Task<bool> BeginAsync(BuildOptions buildOptions)
    {
      try
      {
        await Task.Yield();

        if (!Initialize())
        {
          return false;
        }

        if (buildOptions.HasFlag(BuildOptions.Build))
        {
          BuildBegin();

          await _buildTask.Task;

          // Work completed
          if (!_buildSuccessful)
          {
            Logger.Output("Build was not successful.");
            return false;
          }
        }

        var remoteInfo = GetRemoteConnectionInfo();

        using (var ssh = new SshTool(remoteInfo))
        {
          var success = await ssh.ConnectAsync();
          if (!success)
          {
            Logger.Output("Could not connect to remote device.");
            return false;
          }

          var vsDbgFolder = LinuxPath.Combine(_options.RemoteVsDbgBasePath, Constants.VS2022);

          await ssh.TryInstallVsDbgAsync(vsDbgFolder);
          await ssh.MakeDeploymentFolderAsync(_options.RemoteDeployBasePath);
          await ssh.CleanFolderAsync(_launchBuilder.RemoteDeployProjectFolder);

          if (buildOptions.HasFlag(BuildOptions.Deploy))
          {
            await ssh.UploadFilesAsync(_launchBuilder.OutputDirFullPath, _launchBuilder.RemoteDeployProjectFolder);
          }
          ////else if (buildOptions.HasFlag(BuildOptions.Publish))
          ////{
          ////  // This is PUBLISH not our 'deployer'
          ////}

          // The following replaces -->> if (_options.RemoteDebugDisplayGui)
          if (buildOptions.HasFlag(BuildOptions.Launch))
          {
            var cmd = $"DISPLAY=:0 dotnet \"{_launchBuilder.RemoteDeployAssemblyFilePath}\" &";
            //// var retPid = ssh.Bash(cmd);

            // RET: "[1] 31974"
            var retPid = ssh.BashStream(cmd, "[");
            Logger.Output($"Launch command returned: {retPid}");

            //ssh.BashStream("export DISPLAY=:0");
            //ssh.BashStream($"dotnet \"{_launchBuilder.RemoteDeployAssemblyFilePath}\"");
          }

          if (buildOptions.HasFlag(BuildOptions.Debug) && buildOptions.HasFlag(BuildOptions.Launch))
          {
            ; // TODO: Find ProcId and set Launch.json to `Attach`
          }
          else if (buildOptions.HasFlag(BuildOptions.Debug))
          {
            BuildDebugAttacher();
          }
        }

        BuildCleanup();
      }
      catch (Exception ex)
      {
        Logger.Output($"An error occurred during the build process. {ex.Message}");
        return false;
      }

      return true;
    }

    public void Dispose()
    {
      try
      {
        ThreadHelper.ThrowIfNotOnUIThread();
        _dte.Events.BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
        _dte.Events.BuildEvents.OnBuildDone -= BuildEvents_OnBuildDone;
      }
      catch { }
    }

    /// <summary>Validate if we have a startup project and that it's for C#.</summary>
    /// <returns>True if valid.</returns>
    public bool IsProjectValid()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      //// var dte = (DTE)Package.GetGlobalService(typeof(DTE));
      var sb = (SolutionBuild2)_dte.Solution.SolutionBuild;
      return sb.StartupProjects != null && ((Array)sb.StartupProjects).Cast<string>().Count() > 0;
    }

    private void BuildBegin()
    {
      // TODO: Disable the menu buttons.
      ThreadHelper.ThrowIfNotOnUIThread();
      BuildEvents = _dte.Events.BuildEvents;

      _buildTask = new TaskCompletionSource<bool>();

      // For some reason, cleanup isn't actually always ran when there has been an error.
      // This removes the fact that if you run a debug attempt, get a file error, that you don't get 2 message boxes, 3 message boxes, etc for each attempt.
      ////BuildEvents.OnBuildDone -= BuildEvents_OnBuildDoneAsync;
      ////BuildEvents.OnBuildDone += BuildEvents_OnBuildDoneAsync;
      ////BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
      ////BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

      _dte.SuppressUI = false;
      _dte.Solution.SolutionBuild.BuildProject(_launchBuilder.ProjectConfigName, _launchBuilder.ProjectFileFullPath);
    }

    private void BuildCleanup()
    {
      // Not really needed
      if (_launchBuilder.DeleteLaunchJsonAfterBuild && File.Exists(_launchJsonPath))
        File.Delete(_launchJsonPath);

      //// BuildEvents.OnBuildDone -= BuildEvents_OnBuildDoneAsync;
      //// BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
    }

    /// <summary>
    /// Start debugging using the remote visual studio server adapter
    /// </summary>
    private void BuildDebugAttacher()
    {
      ////_launchJsonPath = _launchBuilder.GenerateLaunchJson();

      _launchJsonPath = _launchBuilder.GenerateLaunchJson(vsdbgLogging: true);
      if (string.IsNullOrEmpty(_launchJsonPath))
      {
        Logger.Output("Could not generate 'launch.json'. Potential folder creation permissions in project's output directory.");
      }

      Logger.Output("Debugger launching...");
      Logger.Output($"- launch.json path: '{_launchJsonPath}'");
      Logger.Output($"- DebugAdapterHost.Launch /LaunchJson:\"{_launchJsonPath}\"");

      DTE2 dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
      //Enable Logging for the Debugger output
      dte2.ExecuteCommand(DebugAdapterHostLogging, $"{DebugAdapterHostLoggingOnOutputWindow}");

      dte2.ExecuteCommand(DebugAdapterHost, $"{DebugAdapterLaunchJson}\"{_launchJsonPath}\"");

      // launchConfigName = "Debug on Linux";
      // DebugAdapterHost.Launch /LaunchJson:LaunchTester\Properties\launch.json /ConfigurationName:"{launchConfigName}"

      Logger.Output("Debug session complete.");
    }

    private void BuildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      // TODO: Re-enable the menu buttons.
      // Inform system that the task is complete
      _buildTask?.TrySetResult(true);

      var not = !_buildSuccessful ? "not " : "";
      Logger.Output($"Build was {not}successful");
    }

    private void BuildEvents_OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
    {
      Logger.Output($"Project [success={success}]: {project}");

      if (!success)
        BuildCleanup();

      _buildSuccessful = Path.GetFileName(project) == $"{_launchBuilder.ProjectName}.csproj" && success;
    }

    private bool Initialize()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
      var project = dte.Solution.GetStartupProject();

      if (project == null)
        return false;

      _launchBuilder = new LaunchBuilder(dte, project, _options);

      // TODO: Commandline Args
      //// if (_options.UseCommandLineArgs)
      ////   _launchBuilder.CommandLineArgs = ... extract from localSettings.json

      return true;
    }

    private SshConnectionInfo GetRemoteConnectionInfo()
    {
      return new SshConnectionInfo
      {
        Host = _options.HostIp,
        Port = _options.HostPort,
        UserGroup = _options.UserGroupName,
        UserName = _options.UserName,
        UserPass = _options.UserPass,
        PrivateKeyEnabled = _options.UserPrivateKeyEnabled,
        PrivateKeyPath = _options.UserPrivateKeyPath,
        PrivateKeyPassword = _options.UserPrivateKeyPassword,
      };
    }

    private bool IsCSharpProject(Project vsProject)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      try
      {
        return vsProject.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageCSharp;
      }
      catch (Exception ex)
      {
        Logger.Output($"Only C# projects are supported at this time. {ex.Message}");
        return false;
      }
    }
  }
}
