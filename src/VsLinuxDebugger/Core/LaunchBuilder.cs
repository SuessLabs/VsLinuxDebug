using System;
using System.IO;
using System.Text.Json;

namespace VsLinuxDebugger.Core
{
  internal class LaunchBuilder
  {
    private string _sshPassword;
    private bool _usePlink = false;
    private bool _useSshKey = false;

    internal const string AdapterFileName = "launch.json";
    internal static string DotNetPath;
    internal static string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    internal static string IpAddress;
    internal static string OutputFolder;
    internal static string UserName;
    internal static string UserPass;
    internal static string VsDbgPath;

    internal LaunchBuilder(string ipAddress, string userName, string password, string vsDbgPath, string dotNetPath, string outputFolder)
    {
      IpAddress = ipAddress;
      UserName = userName;
      UserPass = password;
      VsDbgPath = vsDbgPath;
      DotNetPath = dotNetPath;
      OutputFolder = outputFolder;
    }

    internal static string SshKeyPath => Path.Combine(HomePath, ".ssh\\id_rsa");

    internal string CommandLineArgs { get; set; } = string.Empty;

    public string Adapter => !_usePlink ? "ssh.exe" : "";

    public string AdapterArgs => !_usePlink
      ? $"-i {SshKeyPath} {UserName}@{IpAddress} {VsDbgPath} --interpreter=vscode"
      : "";

    internal string ToJson()
    {
      var launch = new LaunchJson();
      var opts = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
      };

      return JsonSerializer.Serialize(launch, opts);
    }

  }
}
