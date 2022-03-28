using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VSLinuxDebugger
{
  internal class LocalHost
  {
    internal LocalHost(string remoteUserName, string remoteUserPass, string remoteIP, string remoteVsDbgPath, string remoteDotnetPath, string remoteDebugFolderPath, bool useSshKey = false, bool usePlink = false)
    {
      _remoteUserName = remoteUserName;
      _remoteUserPass = remoteUserPass;
      _remoteUseSshKey = useSshKey;
      _remoteUsePLink = usePlink;
      _remoteIP = remoteIP;
      _remoteHostPort = 22;
      _remoteVsDbgPath = remoteVsDbgPath;
      _remoteDotnetPath = remoteDotnetPath;
      _remoteDebugFolderPath = remoteDebugFolderPath;
    }

    private readonly string _remoteUserName;
    private readonly string _remoteUserPass;
    private readonly bool _remoteUseSshKey;
    private readonly bool _remoteUsePLink;
    private readonly string _remoteIP;
    private readonly uint _remoteHostPort;
    private readonly string _remoteVsDbgPath;
    private readonly string _remoteDotnetPath;
    private readonly string _remoteDebugFolderPath;

    internal static string DEBUG_ADAPTER_HOST_FILENAME => "launch.json";
    internal static string HOME_DIR_PATH => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    internal static string SSH_KEY_PATH => Path.Combine(HOME_DIR_PATH, ".ssh\\id_rsa"); // TODO: Make path dynamic

    internal string DebugAdapterHostFilePath { get; set; }
    internal string ProjectName { get; set; }
    internal string Assemblyname { get; set; }
    internal string ProjectFullName { get; set; }
    internal string SolutionFullName { get; set; }
    internal string SolutionDirPath { get; set; }
    internal string ProjectConfigName { get; set; }
    internal string OutputDirName { get; set; }
    internal string OutputDirFullName { get; set; }

    /// <summary>Space delimited string containing command line arguments</summary>
    internal string CommandLineArguments { get; set; } = String.Empty;

    public string GetExtensionDirectory()
    {
      try
      {
        var uri = new Uri(typeof(VSLinuxDebuggerPackage).Assembly.CodeBase, UriKind.Absolute);
        return Path.GetDirectoryName(uri.LocalPath);
      }
      catch
      {
        return null;
      }
    }

    /// <summary>Generates a temporary json file and returns its path</summary>
    /// <returns>Full path to the generated json file</returns>
    internal string ToJson()
    {
      var sshPassword = !_remoteUseSshKey
        ? $"-pw {_remoteUserPass}"
        : $"-i {SSH_KEY_PATH}";

      var sshEndpoint = $"{_remoteUserName}@{_remoteIP}:{_remoteHostPort}";

      dynamic json = new JObject();
      json.version = "0.2.0";

      if (!_remoteUsePLink)
      {
        json.adapter = "ssh.exe";
        json.adapterArgs = $"{sshPassword} {sshEndpoint} {_remoteVsDbgPath} --interpreter=vscode";
      }
      else
      {
        //// "%LocalAppData%\\microsoft\\visualstudio\\16.0_c1d3f8c1\\extensions\\cruad2hg.efs\\plink.exe";
        var plinkPath = Path.Combine(GetExtensionDirectory(), "plink.exe").Trim('"');
        json.adapter = plinkPath;
        json.adapter = @"C:\work\tools\PuTTY\PLINK.EXE";  // For testing only
        json.adapterArgs = $"-ssh -pw {_remoteUserPass} {_remoteUserName}@{_remoteIP} -batch -T {_remoteVsDbgPath} --interpreter=vscode";
        //// json.adapterArgs = $"-ssh -pw {_remoteUserPass} {_remoteUserName}@{_remoteIP}:22  -batch -T {_remoteVsDbgPath} --interpreter=vscode";
      }

      json.configurations = new JArray() as dynamic;
      dynamic config = new JObject();
      config.project = "default";
      config.type = "coreclr";
      config.request = "launch";
      config.program = _remoteDotnetPath;

      ////var jarrObj = new JArray($"./{Assemblyname}.dll");
      var jarrObj = new JArray($"{Assemblyname}.dll");
      if (CommandLineArguments.Length > 0)
      {
        foreach (var arg in CommandLineArguments.Split(' '))
        {
          jarrObj.Add(arg);
        }
      }

      config.args = jarrObj;
      config.cwd = _remoteDebugFolderPath;
      config.stopAtEntry = "false";
      config.console = "internalConsole";
      json.configurations.Add(config);

      string tempJsonPath = Path.Combine(Path.GetTempPath(), DEBUG_ADAPTER_HOST_FILENAME);
      File.WriteAllText(tempJsonPath, Convert.ToString(json));

      return tempJsonPath;

      /*
        {
          "version": "0.2.0",
          "adapter": "%LocalAppData%\\microsoft\\visualstudio\\16.0_c1d3f8c1\\extensions\\cruad2hg.efs\\plink.exe",
          "adapterArgs": "-pw {_remoteUserPass} {_remoteUserName}@{_remoteIP}:22  -batch -T vsdbg --interpreter=vscode",
          "configurations": [
            {
              "name": ".NET Core Launch (console)",
              "type": "coreclr",
              "request": "launch",
              "preLaunchTask": "build",
              "program": "dotnet",
              "args": [
                "{Assemblyname}.dll",
                ""
              ],
              "cwd": "./MonoDebugTemp/",
              "console": "internalConsole",
              "stopAtEntry": true
            }
          ]
        }
       */
    }
  }
}
