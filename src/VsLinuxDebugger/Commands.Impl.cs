using System;
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
      public const int CmdBuildDeployLaunch = 0x1006;

      public const int CmdDebugOnly = 0x1003;
      ////public const int CmdPublishOnly = 0x1006;
      ////public const int CmdPublishDebug= 0x1007;

      public const int CmdShowLog = 0x1004;
      public const int CmdShowSettings = 0x1005;

      public const int LinuxRemoteMainMenu = 0x1000;
      public const int RemoteMainMenuGroupLevel1 = 0x1100;
      public const int RemoteMainMenuGroupLevel2 = 0x1200;
    }

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
    ////    case CommandIds.CmdBuildDeployLaunch: return "Build, Deploy and Launch";
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
      AddMenuItem(cmd, CommandIds.CmdBuildDeployLaunch, SetMenuTextAndVisibility, OnBuildDeployLaunchAsync);

      ////AddMenuItem(cmd, CommandIds.CmdPublishDebug, SetMenuTextAndVisibility, OnPublishDebugAsyc);
      AddMenuItem(cmd, CommandIds.CmdDebugOnly, SetMenuTextAndVisibility, OnDebugOnlyAsync);

      AddMenuItem(cmd, CommandIds.CmdShowLog, SetMenuTextAndVisibility, OnShowLog);
      AddMenuItem(cmd, CommandIds.CmdShowSettings, SetMenuTextAndVisibility, OnShowSettingsAsync);
    }

    private async Task<bool> ExecuteBuildAsync(BuildOptions buildOptions)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var success = true;

      var options = ToUserOptions();
      using (var dbg = new RemoteDebugger(options))
      {
        if (!dbg.IsProjectValid())
        {
          Logger.Output("No C# startup project/solution loaded.");
          success = false;
        }

        if (success && !await dbg.BeginAsync(buildOptions))
        {
          Logger.Output("Failed to perform actions.");
          success = false;
        }
      }

      return success;
    }

    private async void OnBuildDeployAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Deploy);
    }

    private async void OnBuildDeployDebugAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Deploy | BuildOptions.Debug);
    }

    private async void OnBuildDeployLaunchAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Deploy | BuildOptions.Launch);
    }

    private async void OnDebugOnlyAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Debug);
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

      Instance._package.ShowOptionPage(typeof(OptionsPage));
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

        if (cmd.CommandID.ID == CommandIds.CmdShowLog
          || cmd.CommandID.ID == CommandIds.CmdDebugOnly
          ////|| cmd.CommandID.ID == CommandIds.CmdShowSettings
          || cmd.CommandID.ID == CommandIds.CmdBuildDeployLaunch)
        {
          cmd.Enabled = false;
        }
        else
        {
          cmd.Enabled = true;
        }
      }
    }

    private UserOptions ToUserOptions()
    {
      return new UserOptions
      {
        DeleteLaunchJsonAfterBuild = Settings.DeleteLaunchJsonAfterBuild,

        HostIp = Settings.HostIp,
        HostPort = Settings.HostPort,

        LocalPLinkPath = Settings.LocalPLinkPath,
        LocalSwitchLinuxDbgOutput = Settings.LocalSwitchLinuxDbgOutput,

        RemoteDebugDisplayGui = Settings.RemoteDebugDisplayGui,
        RemoteDeployBasePath = Settings.RemoteDeployBasePath,
        RemoteDotNetPath = Settings.RemoteDotNetPath,
        RemoteVsDbgBasePath = Settings.RemoteVsDbgBasePath,

        UseCommandLineArgs = Settings.UseCommandLineArgs,
        //// UsePublish = Settings.UsePublish,

        UserPrivateKeyEnabled = Settings.UserPrivateKeyEnabled,
        UserPrivateKeyPath = Settings.UserPrivateKeyPath,
        UserPrivateKeyPassword = Settings.UserPrivateKeyPassword,
        UserName = Settings.UserName,
        UserPass = Settings.UserPass,
        UserGroupName = Settings.UserGroupName,
      };
    }
  }
}
