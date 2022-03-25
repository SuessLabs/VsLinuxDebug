using System;
using System.IO;

namespace VsLinuxDebugger.Core
{
  internal class LaunchSettings
  {
    internal const string AdapterFileName = "launch.json";

    private readonly string _dotNetPath;
    private readonly string _ipAddress;
    private readonly string _outputFolder;
    private readonly string _userName;
    private readonly string _userPass;
    private readonly string _vsDbgPath;

    internal LaunchSettings(string ipAddress, string userName, string password, string vsDbgPath, string dotNetPath, string outputFolder)
    {
      _ipAddress = ipAddress;
      _userName = userName;
      _userPass = password;
      _vsDbgPath = vsDbgPath;
      _dotNetPath = dotNetPath;
      _outputFolder = outputFolder;
    }

    internal static string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    internal static string SshKeyPath => Path.Combine(HomePath, ".ssh\\id_rsa");
  }
}
