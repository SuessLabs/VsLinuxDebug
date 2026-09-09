using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsLinuxDebugger.Core;
using Xeno.VsLinuxDebug.OptionsPages;

namespace VsLinuxDebugger
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Its annoying")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD200:Avoid async void methods", Justification = "Its annoying")]
  internal sealed partial class Commands
  {
    /// <summary>VS Menu Command IDs. This must be insync with .vsct values.</summary>
    private sealed class CommandIds
    {
      public const int CmdBuildDeployOnly = 0x1001;
      public const int CmdBuildDeployDebug = 0x1002;

      public const int CmdDebugOnly = 0x1003;
      ////public const int CmdPublishOnly = 0x1006;
      ////public const int CmdPublishDebug= 0x1007;

      public const int CmdShowLog = 0x1004;
      public const int CmdShowSettings = 0x1005;
      public const int CmdStop = 0x1006;

      public const int LinuxRemoteMainMenu = 0x1000;
      public const int RemoteMainMenuGroupLevel1 = 0x1100;
      public const int RemoteMainMenuGroupLevel2 = 0x1200;
    }

    /// <summary>Non-null while a build/deploy/debug operation is in progress; cancelled by
    /// <see cref="OnStop"/>.</summary>
    private static CancellationTokenSource _runningOperation;

    /////// <summary>Override standard button text with.</summary>
    /////// <param name="commandId">Command Id.</param>
    /////// <returns>Text to display.</returns>
    ////public string GetMenuText(int commandId)
    ////{
    ////  switch (commandId)
    ////  {
    ////    case CommandIds.CmdBuildDeployOnly: return "Build and Deploy";
    ////    case CommandIds.CmdBuildDeployDebug: return "Build, Deploy and Debug";
    ////
    ////    case CommandIds.CmdDebugOnly: return "Debug Only";
    ////    ////case CommandIds.CmdPublishOnly: return "Publish Only";
    ////    ////case CommandIds.CmdPublishDebug: return "Publish and Debug";
    ////    case CommandIds.CmdShowLog: return "Show Log";
    ////    case CommandIds.CmdShowSettings: return "Settings";
    ////    default: return $"Unknown CommandId ({commandId})";
    ////  }
    ////}

    /// <summary>Wire-up menu item to event handlers</summary>
    /// <remarks>See `DebuggerPackage.vsct` for menu item builder.</remarks>
    /// <param name="cmd">Command invoked by user.</param>
    private void CreateVsMenu(OleMenuCommandService cmd)
    {
      AddMenuItem(cmd, CommandIds.CmdBuildDeployOnly, SetMenuTextAndVisibility, OnBuildDeployAsync);
      AddMenuItem(cmd, CommandIds.CmdBuildDeployDebug, SetMenuTextAndVisibility, OnBuildDeployDebugAsync);

      ////AddMenuItem(cmd, CommandIds.CmdPublishDebug, SetMenuTextAndVisibility, OnPublishDebugAsyc);
      AddMenuItem(cmd, CommandIds.CmdDebugOnly, SetMenuTextAndVisibility, OnDebugOnlyAsync);

      AddMenuItem(cmd, CommandIds.CmdShowLog, SetMenuTextAndVisibility, OnShowLog);
      AddMenuItem(cmd, CommandIds.CmdShowSettings, SetMenuTextAndVisibility, OnShowSettingsAsync);

      // OleMenuCommand.Enabled defaults to true and is only recomputed by
      // BeforeQueryStatus once the menu is actually opened/queried -- without this, Stop
      // shows enabled from VS startup until the first time the menu is touched.
      var stopCmd = AddMenuItem(cmd, CommandIds.CmdStop, SetMenuTextAndVisibility, OnStop);
      stopCmd.Enabled = false;
    }

    private async Task<bool> ExecuteBuildAsync(BuildOptions buildOptions)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var success = true;

      using (var cts = new CancellationTokenSource())
      {
        _runningOperation = cts;

        var options = ToUserOptions();
        using (var dbg = new RemoteDebugger(options))
        {
          if (!dbg.IsProjectValid())
          {
            Logger.Output("No C# startup project/solution loaded.");
            success = false;
          }

          if (success && !await dbg.BeginAsync(buildOptions, cts.Token))
          {
            if (!cts.IsCancellationRequested)
              Logger.Output("Failed to perform actions.");
            success = false;
          }
        }

        _runningOperation = null;
      }

      return success;
    }

    private void OnStop(object sender, EventArgs e)
    {
      _runningOperation?.Cancel();
    }

    private async void OnBuildDeployAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Deploy);
    }

    private async void OnBuildDeployDebugAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Deploy | BuildOptions.Debug);
    }

    private async void OnDebugOnlyAsync(object sender, EventArgs e)
    {
      // No Build, no Deploy/Publish: just attach to what's already running/deployed.
      await ExecuteBuildAsync(BuildOptions.Debug);
    }

    private void OnShowLog(object sender, EventArgs e)
    {
      // Not implemented yet
      if (sender is OleMenuCommand cmd)
        cmd.Enabled = false;

      MessageBox("Not implemented");
    }

    private async void OnShowSettingsAsync(object sender, EventArgs e)
    {
      // Not implemented yet
      if (sender is OleMenuCommand cmd)
        cmd.Enabled = false;

      await Task.Yield();

      Instance._package.ShowOptionPage(typeof(RemoteHostOptionsPage));
    }

    private void SetMenuTextAndVisibility(object sender, EventArgs e)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      if (sender is OleMenuCommand cmd)
      {
        // TODO: Enhance by displaying IP Address
        ////var settings = SettingsManager.Instance.Load();
        //// cmd.Text = $"{GetMenuText(cmd.CommandID.ID)} ({settings.HostIp})";
        //// cmd.Enabled = _extension.IsStartupProjectAvailable();

        var isRunning = _runningOperation != null;

        //// || cmd.CommandID.ID == CommandIds.CmdShowSettings
        if (cmd.CommandID.ID == CommandIds.CmdShowLog)
        {
          cmd.Enabled = false;
        }
        else if (cmd.CommandID.ID == CommandIds.CmdStop)
        {
          cmd.Enabled = isRunning;
        }
        else
        {
          cmd.Enabled = !isRunning;
        }
      }
    }

    private UserOptions ToUserOptions()
    {
      DebuggerPackage VsixPackage = _package as DebuggerPackage;

      return new UserOptions
      {
        DeleteLaunchJsonAfterBuild = VsixPackage.LocalOptions.DeleteLaunchJsonAfterBuild,

        HostIp = VsixPackage.RemoteHostOptions.HostIp,
        HostPort = VsixPackage.RemoteHostOptions.HostPort,

        LocalPLinkPath = VsixPackage.LocalOptions.PLinkPath,
        LocalSwitchLinuxDbgOutput = VsixPackage.LocalOptions.SwitchLinuxDbgOutput,
        ForceKillOnStop = VsixPackage.LocalOptions.ForceKillOnStop,

        RemoteDebugDisplayGui = VsixPackage.RemoteLaunchOptions.RemoteDebugDisplayGui,
        RemoteDebugDisplayNumber = VsixPackage.RemoteLaunchOptions.RemoteDebugDisplayNumber,
        RemoteDeployBasePath = VsixPackage.RemoteDebuggerOptions.RemoteDeployBasePath,
        RemoteEnvironmentVariables = VsixPackage.RemoteLaunchOptions.RemoteEnvironmentVariables,
        RemotePreDeployCommands = VsixPackage.RemoteLaunchOptions.RemotePreDeployCommands,
        RemotePostDeployCommands = VsixPackage.RemoteLaunchOptions.RemotePostDeployCommands,
        AttachToRunningProcess = VsixPackage.RemoteLaunchOptions.AttachToRunningProcess,
        RemotePidCommand = VsixPackage.RemoteLaunchOptions.RemotePidCommand,
        RemoteVsDbgBasePath = VsixPackage.RemoteDebuggerOptions.RemoteVsDbgRootPath,

        SudoCommand = VsixPackage.RemoteLaunchOptions.SudoCommand,
        UseSudoForDebugger = VsixPackage.RemoteLaunchOptions.UseSudoForDebugger,

        UseSelfContainedDeployment = VsixPackage.RemoteDebuggerOptions.UseSelfContainedDeployment,
        RemoteRuntimeIdentifier = VsixPackage.RemoteDebuggerOptions.RemoteRuntimeIdentifier,
        //// UsePublish = Settings.UsePublish,

        UserPrivateKeyEnabled = VsixPackage.RemoteCredentialsOptions.UserPrivateKeyEnabled,
        UserPrivateKeyPath = VsixPackage.RemoteCredentialsOptions.UserPrivateKeyPath,
        UserPrivateKeyPassword = VsixPackage.RemoteCredentialsOptions.UserPrivateKeyPassword,
        UserCertificatePath = VsixPackage.RemoteCredentialsOptions.UserCertificatePath,
        UserName = VsixPackage.RemoteCredentialsOptions.UserName,
        UserPass = VsixPackage.RemoteCredentialsOptions.UserPass,
        UserGroupName = VsixPackage.RemoteHostOptions.UserGroupName,
        UseSSHExeEnabled = VsixPackage.LocalOptions.UseSSHExeEnabled
      };
    }
  }
}
