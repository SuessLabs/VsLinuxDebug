using System;
using System.IO;
using System.Text.Json;

namespace VsLinuxDebugger.Core
{
  internal class LaunchBuilder
  {
    internal const string AdapterFileName = "launch.json";

    private UserOptions _options;

    internal LaunchBuilder(UserOptions o)
    {
      _options = o;

    }

    internal string AssemblyName { get; set; }

    internal string OutputDirFullName { get; set; }

    internal string OutputDirName { get; set; }

    internal string ProjectConfigName { get; set; }

    internal string ProjectFullName { get; set; }

    internal string ProjectName { get; set; }

    internal string SolutionDirPath { get; set; }

    internal string SolutionFullName { get; set; }

    internal string CommandLineArgs { get; set; } = string.Empty;

    internal string ToJson()
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

      return JsonSerializer.Serialize(launch, opts);
    }
  }
}
