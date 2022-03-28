using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        using (var ssh = new SshTool(_options, _launchBuilder))
        {
          if (!ssh.Connect())
          {
            Console.WriteLine("Could not connect to remote device.");
            return false;
          }

          //// _ssh = ssh;

          ssh.TryInstallVsDbg();
          ssh.MakeDeploymentFolder();
          ssh.CleanDeploymentFolder();

          if (buildOptions.HasFlag(BuildOptions.Build))
          {
            BuildBegin();
          }

          if (buildOptions.HasFlag(BuildOptions.Deploy))
          {
            if (_options.RemoteDebugDisplayGui)
              ssh.Bash("export DISPLAY=:0");

            ssh.UploadFilesAsync();
          }
          else if (buildOptions.HasFlag(BuildOptions.Publish))
          {
            // NOT IMPL
          }

          if (buildOptions.HasFlag(BuildOptions.Debug))
          {
            //// AttachToProcess();
          }

          //// _ssh = null;
        }
      }
      catch (Exception ex)
      {
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

    private void AttachToProcess()
    {
      // TODO: Create Launch.JSON file.
    }

    private void BuildBegin()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE)Package.GetGlobalService(typeof(DTE));
      BuildEvents = dte.Events.BuildEvents;

      // For some reason, cleanup isn't actually always ran when there has been an error.
      // This removes the fact that if you run a debug attempt, get a file error, that you don't get 2 message boxes, 3 message boxes, etc for each attempt.
      ////BuildEvents.OnBuildDone -= BuildEvents_OnBuildDoneAsync;
      ////BuildEvents.OnBuildDone += BuildEvents_OnBuildDoneAsync;
      ////BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
      ////BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

      dte.SuppressUI = false;
      dte.Solution.SolutionBuild.BuildProject(_launchBuilder.ProjectConfigName, _launchBuilder.ProjectFullName);
    }

    private void BuildCleanup()
    {
      File.Delete(_launchJsonPath);

      //// BuildEvents.OnBuildDone -= BuildEvents_OnBuildDoneAsync;
      //// BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
    }

    /// <summary>
    /// Start debugging using the remote visual studio server adapter
    /// </summary>
    private void BuildDebug()
    {
      throw new NotImplementedException();

      //// _launchJsonPath = _localhost.ToJson();
      //// 
      //// var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
      //// dte.ExecuteCommand("DebugAdapterHost.Launch", $"/LaunchJson:\"{_launchJsonPath}\"");
    }

    private bool Initialize()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
      var project = dte.Solution.GetStartupProject();

      if (project == null)
        return false;

      _launchBuilder = new LaunchBuilder(_options)
      {
        AssemblyName = project.Properties.Item("AssemblyName").Value.ToString(),
        ProjectConfigName = project.ConfigurationManager.ActiveConfiguration.ConfigurationName,
        ProjectFullName = project.FullName,
        ProjectName = project.Name,
        SolutionFullName = dte.Solution.FullName,
        SolutionDirPath = Path.GetDirectoryName(dte.Solution.FullName),
        OutputDirName = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString(),
        OutputDirFullName = Path.Combine(Path.GetDirectoryName(project.FullName), _launchBuilder.OutputDirName),
      };

      //// if (_options.UseCommandLineArgs)
      ////   _launchBuilder.CommandLineArgs = ... extract from localSettings.json

      return true;
    }

    /*
    public async Task BuildStartupProjectAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var failedBuilds = BuildStartupProject();
      if (failedBuilds > 0)
      {
        Window window = _dte.Windows.Item("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}");//EnvDTE.Constants.vsWindowKindOutput
        OutputWindow outputWindow = (OutputWindow)window.Object;
        outputWindow.ActivePane.Activate();
        outputWindow.ActivePane.OutputString($"{failedBuilds} project(s) failed to build. See error and output window!");

        //// _errorListProvider.Show();

        throw new Exception($"{failedBuilds} project(s) failed to build. See error and output window!");
      }
    }

    private int BuildStartupProject()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      //// var dte = (DTE)Package.GetGlobalService(typeof(DTE));
      var sb = (SolutionBuild2)_dte.Solution.SolutionBuild;

      try
      {
        var startProject = GetStartupProject();
        var activeConfiguration = _dte.Solution.SolutionBuild.ActiveConfiguration as SolutionConfiguration2;
        var activeConfigurationName = activeConfiguration.Name;
        var activeConfigurationPlatform = activeConfiguration.PlatformName;
        var startProjectName = startProject.FullName;

        sb.BuildProject($"{activeConfigurationName}|{activeConfigurationPlatform}", startProject.FullName, true);
      }
      catch (Exception ex)
      {
        // Build complete solution (fallback solution)
        return BuildSolution();
      }

      return sb.LastBuildInfo;
    }

    private int BuildSolution()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      var sb = (SolutionBuild2)_dte.Solution.SolutionBuild;
      sb.Build(true);
      return sb.LastBuildInfo;
    }
    */

    /////// <summary>Build is ready for the next steps.</summary>
    ////private async void BuildEvents_OnBuildDoneAsync(vsBuildScope scope, vsBuildAction action)
    ////{
    ////  if (_buildSuccessful)
    ////  {
    ////    string errormessage = await TransferFiles2Async().ConfigureAwait(true);
    ////
    ////    if (errormessage == "")
    ////    {
    ////      StartDebug();
    ////      BuildCleanup();
    ////    }
    ////    else
    ////    {
    ////      Output($"Transferring files failed: {errormessage}");
    ////    }
    ////  }
    ////}
    ////
    /////// <summary>The build finised sucessfully and no errors were found.</summary>
    ////private void BuildEvents_OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
    ////{
    ////  string debugtext = $"Project: {project} --- Success: {success}\n";
    ////
    ////  if (!success)
    ////  {
    ////    BuildCleanup();
    ////  }
    ////
    ////  _buildSuccessful = Path.GetFileName(project) == _localhost.ProjectName + ".csproj" && success;
    ////}

    private bool IsCSharpProject(Project vsProject)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      try
      {
        return vsProject.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageCSharp;
      }
      catch (Exception ex)
      {
        LogOutput($"Project doesn't support property vsProject.CodeModel.Language! No CSharp project. {ex.Message}");
        return false;
      }
    }

    private void LogOutput(string message)
    {
      // TODO: Send to VS Output Window
      Console.WriteLine($">> {message}");
    }
  }
}
