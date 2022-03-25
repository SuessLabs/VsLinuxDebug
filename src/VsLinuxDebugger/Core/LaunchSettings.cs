using System;
using System.IO;

namespace VsLinuxDebugger.Core
{
  internal class LaunchSettings
  {
    internal const string AdapterFileName = "launch.json";
    internal static string DotNetPath;
    internal static string IpAddress;
    internal static string OutputFolder;
    internal static string UserName;
    internal static string UserPass;
    internal static string VsDbgPath;

    internal static string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    internal LaunchSettings(string ipAddress, string userName, string password, string vsDbgPath, string dotNetPath, string outputFolder)
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

    internal string ToJson()
    {
      var launch = new Launch();
      return System.Text.Json.JsonSerializer.Serialize(launch);
    }

    private class Launch
    {
      public string Adapter => "ssh.exe";

      public string AdapterArgs => $"-i {SshKeyPath} {UserName}@{IpAddress} {VsDbgPath} --interpreter=vscode";

      public string Version => "0.2.0";
    }
  }
}
