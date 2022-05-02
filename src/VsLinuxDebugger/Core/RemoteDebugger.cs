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
  public class RemoteDebugger
  {
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
    }

    public static BuildEvents BuildEvents { get; set; }

    /// <summary>Perform operation.</summary>
    /// <param name="buildOptions">Build uptions</param>
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

        using (var ssh = new SshTool(_options, _launchBuilder))
        {
          if (!ssh.Connect())
          {
            Console.WriteLine("Could not connect to remote device.");
            return false;
          }

          ssh.TryInstallVsDbg();
          ssh.MakeDeploymentFolder();
          ssh.CleanDeploymentFolder();

          if (buildOptions.HasFlag(BuildOptions.Deploy))
          {
            if (_options.RemoteDebugDisplayGui)
              ssh.Bash("export DISPLAY=:0");

            await ssh.UploadFilesAsync();
          }
          else if (buildOptions.HasFlag(BuildOptions.Publish))
          {
            // This is PUBLISH not our 'deployer'
          }

          if (buildOptions.HasFlag(BuildOptions.Debug))
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

      var dte = (DTE)Package.GetGlobalService(typeof(DTE));
      BuildEvents = dte.Events.BuildEvents;

      _buildTask = new TaskCompletionSource<bool>();

      BuildEvents.OnBuildProjConfigDone += (string project, string projectConfig, string platform, string solutionConfig, bool success) =>
      {
        Logger.Output($"Project: {project} --- Success: {success}\n");

        if (!success)
          BuildCleanup();

        _buildSuccessful = Path.GetFileName(project) == $"{_launchBuilder.ProjectName}.csproj" && success;
      };

      BuildEvents.OnBuildDone += (vsBuildScope scope, vsBuildAction action) =>
      {
        // TODO: Re-endable the menu buttons.
        // Inform system that the task is complete
        _buildTask?.TrySetResult(true);

        var not = !_buildSuccessful ? "not" : "";
        Logger.Output($"Build was {not}successful");
      };

      // For some reason, cleanup isn't actually always ran when there has been an error.
      // This removes the fact that if you run a debug attempt, get a file error, that you don't get 2 message boxes, 3 message boxes, etc for each attempt.
      ////BuildEvents.OnBuildDone -= BuildEvents_OnBuildDoneAsync;
      ////BuildEvents.OnBuildDone += BuildEvents_OnBuildDoneAsync;
      ////BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
      ////BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

      dte.SuppressUI = false;
      dte.Solution.SolutionBuild.BuildProject(_launchBuilder.ProjectConfigName, _launchBuilder.ProjectFileFullPath);
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

      DTE2 dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
      dte2.ExecuteCommand("DebugAdapterHost.Launch", $"/LaunchJson:\"{_launchJsonPath}\"");

      // launchConfigName = "Debug on Linux";
      // DebugAdapterHost.Launch /LaunchJson:LaunchTester\Properties\launch.json /ConfigurationName:"{launchConfigName}"

      Logger.Output("Debug session complete.");
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
