using System;
using System.IO;
using System.Text.Json;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using VsLinuxDebugger.Core.Remote;

namespace VsLinuxDebugger.Core
{
  // TODO: Combine with UserOptions to make life easier.
  public class LaunchBuilder
  {
    public const string AdapterFileName = "launch.json";

    private UserOptions _opts;

    public LaunchBuilder(DTE2 dte, Project dteProject, UserOptions o)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      _opts = o;

      AssemblyName = dteProject.Properties.Item("AssemblyName").Value.ToString();
      ProjectConfigName = dteProject.ConfigurationManager.ActiveConfiguration.ConfigurationName;
      ProjectFileFullPath = dteProject.FullName;
      ProjectName = dteProject.Name;
      SolutionFileFullPath = dte.Solution.FullName;
      SolutionDirPath = Path.GetDirectoryName(dte.Solution.FullName);
      OutputDirName = dteProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
      OutputDirFullPath = Path.Combine(Path.GetDirectoryName(dteProject.FullName), OutputDirName);
    }

    /// <summary>Project assembly name. I.E. "ConsoleApp1"</summary>
    public string AssemblyName { get; set; }

    public string CommandLineArgs { get; set; } = string.Empty;

    /// <summary>Remove the `launch.json` file after building. Keep it around for debugging.</summary>
    public bool DeleteLaunchJsonAfterBuild => _opts.DeleteLaunchJsonAfterBuild;

    /// <summary>Full output folder path. I.E. "C:\\path\\Repos\\Porj\\bin\\Debug\\net6.0\\".</summary>
    public string OutputDirFullPath { get; set; }

    /// <summary>Partial path to the output directory. I.E. "bin\\Debug\\net6.0".</summary>
    public string OutputDirName { get; set; }

    /// <summary>Configuration build type. I.E. "Debug".</summary>
    public string ProjectConfigName { get; set; }

    /// <summary>Full project output path. I.E. "C:\\path\\Repos\\Proj\\ConsoleApp1.csproj"</summary>
    public string ProjectFileFullPath { get; set; }

    /// <summary>Project name (not always the same as AssemblyName). I.E. "Console App1"</summary>
    public string ProjectName { get; set; }

    /// <summary>Full path to the remote assembly. (i.e. `/home/USER/VLSDbg/Proj/ConsoleApp1.dll`)</summary>
    public string RemoteDeployAppPath => LinuxPath.Combine(RemoteDeployFolder, $"{AssemblyName}.dll");

    /// <summary>Folder of our remote assembly. (i.e. `/home/USER/VLSDbg/Proj`)</summary>
    public string RemoteDeployFolder =>
      LinuxPath.Combine(_opts.RemoteDeployBasePath, ProjectName);

    public string RemoteDotNetPath => _opts.RemoteDotNetPath;

    public string RemoteHostIp => _opts.HostIp;

    public int RemoteHostPort => _opts.HostPort;

    public string RemoteUserName => _opts.UserName;

    public string RemoteUserPass => _opts.UserPass;

    public string RemoteVsDbgPath => _opts.RemoteVsDbgPath;

    /// <summary>Solution folder path. I.E. "C:\\path\Repos\"</summary>
    public string SolutionDirPath { get; set; }

    /// <summary>Full solution output path. I.E. "C:\\path\Repos\Proj.sln"</summary>
    public string SolutionFileFullPath { get; set; }

    /// <summary>Generates the project's `launch.json` file.</summary>
    /// <returns>Returns the local path to the file.</returns>
    public string GenerateLaunchJson(bool vsdbgLogging = false)
    {
      string adapter, adapterArgs;

      //// var sshEndpoint = $"{_opts.UserName}@{_opts.HostIp}:{_opts.HostPort}";
      var sshEndpoint = $"{_opts.UserName}@{_opts.HostIp}";

      var vsdbgLogPath = "";
      if (vsdbgLogging)
        vsdbgLogPath = $" --engineLogging={LinuxPath.Combine(RemoteDeployFolder, "_vsdbg.log")}";

      if (!_opts.LocalPlinkEnabled)
      {
        //// SSH Key alt-args:
        //// $"-i \"{_opts.UserPrivateKeyPath}\" -o \"StrictHostKeyChecking no\" {RemoteUserName}@{RemoteHostIp} {_opts.RemoteVsDbgPath} --interpreter=vscode {vsdbgLogPath}")
        var sshPassword = !_opts.UserPrivateKeyEnabled
          ? $"-pw {_opts.UserPass}"
          : $"-i {_opts.UserPrivateKeyPath} -o \"StrictHostKeyChecking no\"";

        adapter = "ssh.exe";
        adapterArgs = $"{sshPassword} {sshEndpoint} {_opts.RemoteVsDbgPath} --interpreter=vscode {vsdbgLogPath}";
      }
      else
      {
        // TODO: Consider packing PLink.exe
        //// "%LocalAppData%\\microsoft\\visualstudio\\16.0_c1d3f8c1\\extensions\\cruad2hg.efs\\plink.exe";
        //// var plinkPath = Path.Combine(GetExtensionDirectory(), "plink.exe").Trim('"');

        adapter = _opts.LocalPLinkPath;
        adapterArgs = $"-ssh -pw {RemoteUserPass} {RemoteUserName}@{RemoteHostIp} -batch -T {RemoteVsDbgPath} {vsdbgLogPath}";

        //// adapterArgs = $"-ssh -pw {RemoteUserPass} {RemoteUserName}@{RemoteHostIp} -batch -T {RemoteVsDbgPath} --interpreter=vscode {vsdbgLogPath}";
        //// adapterArgs = $"-ssh -pw {_options.UserPass} {_options.UserName}@{_options.HostIp}:{_options.HostPort} -batch -T {_options.RemoteVsDbgPath} --interpreter=vscode";
      }

      var obj = new Launch(
          RemoteDotNetPath,
          $"{AssemblyName}.dll", /// RemoteDeployAppPath,
          RemoteDeployFolder,
          default,
          false)
      {
        Adapter = adapter,
        AdapterArgs = adapterArgs,
      };

      var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
      });

      // Create out file
      var outputPath = Path.Combine(OutputDirFullPath, "launch.json");

      try
      {
        File.WriteAllText(outputPath, json);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error writing 'launch.json' to path, '{outputPath}'!\n{ex.Message}");
        outputPath = string.Empty;
      }

      return outputPath;
    }
  }
}
