using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using VsLinuxDebugger.Core;

namespace VsLinuxDebugger
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Its annoying")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD200:Avoid async void methods", Justification = "Its annoying")]
  internal sealed partial class Commands
  {
    /// <summary>Override standard button text with.</summary>
    /// <param name="commandId">Command Id.</param>
    /// <returns>Text to display.</returns>
    public string GetMenuText(int commandId)
    {
      switch (commandId)
      {
        case CommandIds.CmdBuildDeployOnly: return "Build and Deploy";
        case CommandIds.CmdBuildDeployDebug: return "Build, Deploy and Debug";
        case CommandIds.CmdDebugOnly: return "Debug Only";
        ////case CommandIds.CmdPublishOnly: return "Publish Only";
        ////case CommandIds.CmdPublishDebug: return "Publish and Debug";
        case CommandIds.CmdShowLog: return "Show Log";
        case CommandIds.CmdShowSettings: return "Settings";
        default: return $"Unknown CommandId ({commandId})";
      }
    }

    private async Task<bool> ExecuteBuildAsync(BuildOptions buildOptions)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var options = ToUserOptions();
      var dbg = new RemoteDebugger(options);

      if (!dbg.IsProjectValid())
      {
        Console.WriteLine("No C# startup project/solution loaded.");
        return false;
      }

      if (!await dbg.BeginAsync(buildOptions))
      {
        Console.WriteLine("Failed to perform actions.");
        return false;
      }

      return true;
    }

    private void CreateVsMenu(OleMenuCommandService cmd)
    {
      AddMenuItem(cmd, CommandIds.CmdBuildDeployOnly, SetMenuTextAndVisibility, OnBuildDeployAsync);
      AddMenuItem(cmd, CommandIds.CmdBuildDeployDebug, SetMenuTextAndVisibility, OnBuildDeployDebugAsync);
      ////AddMenuItem(cmd, CommandIds.CmdPublishDebug, SetMenuTextAndVisibility, OnPublishDebugAsyc);
      AddMenuItem(cmd, CommandIds.CmdDebugOnly, SetMenuTextAndVisibility, OnDebugOnlyAsync);

      AddMenuItem(cmd, CommandIds.CmdShowLog, SetMenuTextAndVisibility, OnShowLog);
      AddMenuItem(cmd, CommandIds.CmdShowSettings, SetMenuTextAndVisibility, OnShowSettingsAsync);
    }

    private async void OnDebugOnlyAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Debug);
    }

    private async void OnBuildDeployDebugAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Deploy | BuildOptions.Debug);
    }

    private async void OnBuildDeployAsync(object sender, EventArgs e)
    {
      await ExecuteBuildAsync(BuildOptions.Build | BuildOptions.Deploy);
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
      MessageBox("Not implemented");
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
          || cmd.CommandID.ID == CommandIds.CmdShowSettings)
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

        LocalPlinkEnabled = Settings.LocalPlinkEnabled,
        LocalPLinkPath = Settings.LocalPLinkPath,

        RemoteDebugDisplayGui = Settings.RemoteDebugDisplayGui,
        RemoteDeployBasePath = Settings.RemoteDeployBasePath,
        ////RemoteDeployDebugPath = Settings.RemoteDeployDebugPath,
        ////RemoteDeployReleasePath = Settings.RemoteDeployReleasePath,
        RemoteDotNetPath = Settings.RemoteDotNetPath,
        RemoteVsDbgPath = Settings.RemoteVsDbgPath,

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
