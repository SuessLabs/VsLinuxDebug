using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsLinuxDebugger.Extensions;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

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
    /// <param name="cancellationToken">Cancelled when the user hits Stop. Checked between each
    /// major step; a SSH command already in flight is instead let to finish unless
    /// <see cref="UserOptions.ForceKillOnStop"/> is enabled, in which case the connection is
    /// torn down immediately, mid-command.</param>
    /// <returns>True on success.</returns>
    public async Task<bool> BeginAsync(BuildOptions buildOptions, CancellationToken cancellationToken = default)
    {
      try
      {
        await Task.Yield();

        if (!Initialize())
        {
          return false;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (buildOptions.HasFlag(BuildOptions.Build))
        {
          BuildBegin();

          using (cancellationToken.Register(() => CancelBuild()))
          {
#pragma warning disable VSTHRD003 // _buildTask is a TaskCompletionSource completed by a
            // DTE build-event callback (BuildEvents_OnBuildDone), not a cold task started
            // elsewhere; awaiting it here cannot deadlock.
            await _buildTask.Task;
#pragma warning restore VSTHRD003
          }

          cancellationToken.ThrowIfCancellationRequested();

          // Work completed
          if (!_buildSuccessful)
          {
            Logger.Output("Build was not successful.");
            return false;
          }
        }

        if (buildOptions.HasFlag(BuildOptions.Deploy))
        {
          if (!await PublishAsync(cancellationToken))
          {
            Logger.Output("Publish was not successful.");
            return false;
          }
        }

        cancellationToken.ThrowIfCancellationRequested();

        var remoteInfo = GetRemoteConnectionInfo();

        using (var ssh = new SshTool(remoteInfo))
        using (cancellationToken.Register(() =>
        {
          if (_options.ForceKillOnStop)
            ssh.Dispose();
        }))
        {
          var success = await ssh.ConnectAsync();
          if (!success)
          {
            Logger.Output("Could not connect to remote device.");
            return false;
          }

          var vsDbgFolder = LinuxPath.Combine(_options.RemoteVsDbgBasePath, Constants.VS2022);

          await ssh.TryInstallVsDbgAsync(vsDbgFolder);

          cancellationToken.ThrowIfCancellationRequested();

          if (buildOptions.HasFlag(BuildOptions.Deploy))
          {
            // Only touch the deployment folder (and run the pre/post-deploy commands) when
            // we're actually about to overwrite it -- doing this unconditionally used to
            // delete the executable backing an already-running attached process on every
            // "Attach Only" run, and re-run the post-deploy commands even when unchanged.
            await ssh.MakeDeploymentFolderAsync(_options.RemoteDeployBasePath);
            await ssh.CleanFolderAsync(_launchBuilder.RemoteDeployProjectFolder);

            await RunConfiguredCommandsAsync(ssh, _options.RemotePreDeployCommands);

            await ssh.UploadFilesAsync(_launchBuilder.PublishDirFullPath, _launchBuilder.RemoteDeployProjectFolder);

            // Transfer (tar/scp) from Windows does not preserve the exec bit, and `dotnet
            // publish` always produces a native apphost executable (even framework-dependent).
            await ssh.BashAsync($"chmod +x \"{_launchBuilder.RemoteDeployExecutableFilePath}\"");

            await RunConfiguredCommandsAsync(ssh, _options.RemotePostDeployCommands);
          }
          ////else if (buildOptions.HasFlag(BuildOptions.Publish))
          ////{
          ////  // This is PUBLISH not our 'deployer'
          ////}

          cancellationToken.ThrowIfCancellationRequested();

          // The following replaces -->> if (_options.RemoteDebugDisplayGui)
          if (buildOptions.HasFlag(BuildOptions.Launch) && !_options.AttachToRunningProcess)
          {
            var cmd = $"DISPLAY=:0 \"{_launchBuilder.RemoteDeployExecutableFilePath}\" &";
            // RET: "[1] 31974"
            var retPid = ssh.BashStream(cmd, "[");
            Logger.Output($"Launch command returned: {retPid}");
          }

          cancellationToken.ThrowIfCancellationRequested();

          if (buildOptions.HasFlag(BuildOptions.Debug))
          {
            if (_options.AttachToRunningProcess)
            {
              // The debuggee is started/supervised externally (i.e. by systemd via the
              // configured pre/post-deploy commands); attach to that exact process instead
              // of launching a second, unmanaged instance of it -- a launched instance would
              // not inherit the supervisor's EnvironmentFile and ambient capabilities, and
              // would leave two copies of the program running.
              var mainPid = (await ssh.BashAsync(_options.RemotePidCommand)).Trim();

              if (int.TryParse(mainPid, out var pid) && pid > 0)
              {
                BuildDebugAttacher(pid);
              }
              else
              {
                Logger.Output($"Could not determine the running PID via '{_options.RemotePidCommand}' (got '{mainPid}'). Is the process actually running?");
              }
            }
            else
            {
              BuildDebugAttacher();
            }
          }
        }

        BuildCleanup();
      }
      catch (OperationCanceledException)
      {
        Logger.Output("Stopped.");
        BuildCleanup();
        return false;
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

    private void CancelBuild()
    {
      ThreadHelper.JoinableTaskFactory.Run(async () =>
      {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        try
        {
          if (_dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress)
            _dte.ExecuteCommand("Build.Cancel");
        }
        catch { }

        // BuildEvents_OnBuildDone still fires after a cancel; unblock the awaiter either way.
        _buildTask?.TrySetResult(false);
      });
    }

    /// <summary>Runs each non-blank line of <paramref name="commands"/> as a separate shell
    /// command on the remote machine, in order.</summary>
    private async Task RunConfiguredCommandsAsync(SshTool ssh, string commands)
    {
      if (string.IsNullOrWhiteSpace(commands))
        return;

      foreach (var line in commands.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
      {
        var command = line.Trim();
        if (command.Length > 0)
          await ssh.BashAsync(command);
      }
    }

    private void BuildCleanup()
    {
      // Not really needed
      if (_launchBuilder.DeleteLaunchJsonAfterBuild && File.Exists(_launchJsonPath))
        File.Delete(_launchJsonPath);

      //// BuildEvents.OnBuildDone -= BuildEvents_OnBuildDoneAsync;
      //// BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
    }

    /// <summary>Runs `dotnet publish` for <see cref="UserOptions.RemoteRuntimeIdentifier"/> into
    /// <see cref="LaunchBuilder.PublishDirFullPath"/>. Always publishing (rather than uploading a
    /// plain build's output folder) is what makes the deployed executable match what a manual
    /// `dotnet publish`/VS Publish produces: even framework-dependent, `dotnet publish -r <rid>`
    /// generates a native apphost executable (no bare `.dll` launch needed), whereas a plain build
    /// does not reliably produce one. It also sidesteps the SDK nesting RID-specific output under
    /// an extra subfolder that this extension would otherwise have to keep guessing at.</summary>
    /// <returns>True if `dotnet publish` exited successfully.</returns>
    private async Task<bool> PublishAsync(CancellationToken cancellationToken)
    {
      if (Directory.Exists(_launchBuilder.PublishDirFullPath))
        Directory.Delete(_launchBuilder.PublishDirFullPath, recursive: true);

      var selfContained = _options.UseSelfContainedDeployment ? "true" : "false";
      var args = $"publish \"{_launchBuilder.ProjectFileFullPath}\" " +
        $"-c \"{_launchBuilder.ProjectConfigName}\" " +
        $"-r \"{_options.RemoteRuntimeIdentifier}\" " +
        $"--self-contained {selfContained} " +
        $"-o \"{_launchBuilder.PublishDirFullPath}\"";

      Logger.Output($"PUBLISH> dotnet {args}");

      var startInfo = new ProcessStartInfo("dotnet", args)
      {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        WorkingDirectory = Path.GetDirectoryName(_launchBuilder.ProjectFileFullPath),
      };

      using (var process = new Process { StartInfo = startInfo })
      using (cancellationToken.Register(() =>
      {
        try
        {
          if (!process.HasExited)
            process.Kill();
        }
        catch { }
      }))
      {
        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await Task.Run(() => process.WaitForExit());

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;

        if (!string.IsNullOrWhiteSpace(stdOut))
          Logger.Output(stdOut);
        if (!string.IsNullOrWhiteSpace(stdErr))
          Logger.Output(stdErr);

        cancellationToken.ThrowIfCancellationRequested();

        return process.ExitCode == 0;
      }
    }

    /// <summary>
    /// Start debugging using the remote visual studio server adapter
    /// </summary>
    /// <param name="attachToPid">When set, attach to this already-running remote process
    /// instead of launching a new one.</param>
    private void BuildDebugAttacher(int? attachToPid = null)
    {
      ////_launchJsonPath = _launchBuilder.GenerateLaunchJson();

      _launchJsonPath = attachToPid.HasValue
        ? _launchBuilder.GenerateAttachLaunchJson(attachToPid.Value, vsdbgLogging: true)
        : _launchBuilder.GenerateLaunchJson(vsdbgLogging: true);
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
        CertificatePath = _options.UserCertificatePath,
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
