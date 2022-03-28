using System.Text.Json;

namespace VsLinuxDebugger.Core
{
  public class LaunchBuilder
  {
    public const string AdapterFileName = "launch.json";

    private UserOptions _options;

    public LaunchBuilder(UserOptions o)
    {
      _options = o;
    }

    public string AssemblyName { get; set; }

    public string CommandLineArgs { get; set; } = string.Empty;

    public string OutputDirFullName { get; set; }

    public string OutputDirName { get; set; }

    public string ProjectConfigName { get; set; }

    public string ProjectFullName { get; set; }

    public string ProjectName { get; set; }

    public string SolutionDirPath { get; set; }

    public string SolutionFullName { get; set; }

    public string ToJson()
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
