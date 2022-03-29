using System;
using System.IO;
using System.Text.Json;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace VsLinuxDebugger.Core
{
  public class LaunchBuilder
  {
    public const string AdapterFileName = "launch.json";

    private UserOptions _options;

    public LaunchBuilder(DTE2 dte, Project dteProject, UserOptions o)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      _options = o;

      AssemblyName = dteProject.Properties.Item("AssemblyName").Value.ToString();
      ProjectConfigName = dteProject.ConfigurationManager.ActiveConfiguration.ConfigurationName;
      ProjectFileFullPath = dteProject.FullName;
      ProjectName = dteProject.Name;
      SolutionFileFullPath = dte.Solution.FullName;
      SolutionDirPath = Path.GetDirectoryName(dte.Solution.FullName);
      OutputDirName = dteProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
      OutputDirFullPath = Path.Combine(Path.GetDirectoryName(dteProject.FullName), OutputDirName);
    }

    public string AssemblyName { get; set; }

    public string CommandLineArgs { get; set; } = string.Empty;

    public string OutputDirFullPath { get; set; }

    public string OutputDirName { get; set; }

    public string ProjectConfigName { get; set; }

    public string ProjectFileFullPath { get; set; }

    public string ProjectName { get; set; }

    public string SolutionDirPath { get; set; }

    public string SolutionFileFullPath { get; set; }

    /// <summary>Generates the project's `launch.json` file.</summary>
    /// <returns>Returns the local path to the file.</returns>
    public string GenerateLaunchJson()
    {
      ////Adapter => !_options.LocalPlinkEnabled ? "ssh.exe" : "";
      ////
      ////AdapterArgs => !_options.LocalPlinkEnabled
      ////  ? $"-i {SshKeyPath} {_options.UserName}@{_options.HostIp} {_options.RemoteVsDbgPath} --interpreter=vscode"
      ////  : "";

      var launch = new LaunchJson();
      var launchCfg = new LaunchJsonConfig();

      var opts = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
      };

      var json = JsonSerializer.Serialize(launch, opts);

      throw new NotImplementedException();

      return string.Empty;
    }
  }
}
