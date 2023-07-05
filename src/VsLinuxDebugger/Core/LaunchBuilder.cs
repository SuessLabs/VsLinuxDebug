using System;
using System.IO;
using System.Text.Json;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using VsLinuxDebugger.Core.Remote;

namespace VsLinuxDebugger.Core
{
  /// <summary>LaunchBuilder class for serialization.</summary>
  public class LaunchBuilder
  {
    public const string AdapterFileName = "launch.json";

    private UserOptions _opts;

    public LaunchBuilder(DTE2 dte, Project dteProject, UserOptions userOptions)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      _opts = userOptions;

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
    public string RemoteDeployAssemblyFilePath => LinuxPath.Combine(RemoteDeployProjectFolder, $"{AssemblyName}.dll");

    /// <summary>Folder of our remote assembly. (i.e. `/home/USER/VLSDbg/Proj`)</summary>
    public string RemoteDeployProjectFolder => LinuxPath.Combine(_opts.RemoteDeployBasePath, ProjectName);

    public string RemoteDotNetPath => _opts.RemoteDotNetPath;

    public string RemoteHostIp => _opts.HostIp;

    public int RemoteHostPort => _opts.HostPort;

    public string RemoteUserName => _opts.UserName;

    public string RemoteUserPass => _opts.UserPass;

    /// <summary>Solution folder path. I.E. "C:\\path\Repos\"</summary>
    public string SolutionDirPath { get; set; }

    /// <summary>Full solution output path. I.E. "C:\\path\Repos\Proj.sln"</summary>
    public string SolutionFileFullPath { get; set; }

    /// <summary>Generates the project's `launch.json` file.</summary>
    /// <returns>Returns the local path to the file.</returns>
    public string GenerateLaunchJson(bool vsdbgLogging = false)
    {
      string adapter, adapterArgs;

      (adapter, adapterArgs) = GetAdapter(vsdbgLogging);

      var obj = new Launch(
          RemoteDotNetPath,
          $"{AssemblyName}.dll", /// RemoteDeployAppPath,
          RemoteDeployProjectFolder,
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
        Logger.Output($"Error writing 'launch.json' to path, '{outputPath}'!\n{ex.Message}");
        outputPath = string.Empty;
      }

      return outputPath;
    }

    private (string adapterPath, string adapterArgs) GetAdapter(bool vsdbgLogging = false)
    {
      // NOTE: Removed ":{RemoteHostPort}" because it failed to launch with PLink
      // var sshEndpoint = $"{_opts.UserName}@{_opts.HostIp}:{_opts.HostPort}";
      var sshEndpoint = $"{RemoteUserName}@{RemoteHostIp}";

      var vsdbgLogPath = "";
      if (vsdbgLogging)
        vsdbgLogPath = $" --engineLogging={LinuxPath.Combine(RemoteDeployProjectFolder, "_vsdbg.log")}";

      ////if (!_opts.LocalPlinkEnabled)
      ////{
      ////  adapter = "ssh.exe";
      ////  adapterArgs = $"{sshPassword} {sshEndpoint} {_opts.RemoteVsDbgPath} --interpreter=vscode {vsdbgLogPath}";
      ////}
      ////else
      ////{

      string plinkPath = string.Empty;

      if (_opts.UseSSHExeEnabled)
      {
        plinkPath = "ssh.exe";
      }
      else
      {
        // Adapter Path:
        // PLink.exe - Use manual path or embedded
        if (!string.IsNullOrEmpty(_opts.LocalPLinkPath) && File.Exists(_opts.LocalPLinkPath))
        {
          plinkPath = _opts.LocalPLinkPath;
        }
        else
        {
          plinkPath = Path.Combine(GetExtensionDirectory(), "plink.exe").Trim('"');
        }
      }
      // Adapter Arguments:
      // NOTE:
      //  1. SSH Private Key ("-i PPK") fails with PLINK. Must use manual password until this is resolved.
      //  2. Strict Host Key Checking is disabled by default; this doesn't need set.
      //
      // REF: https://linuxhint.com/ssh-stricthostkeychecking/
      //      $"-i \"{_opts.UserPrivateKeyPath}\" -o \"StrictHostKeyChecking no\" {RemoteUserName}@{RemoteHostIp} {_opts.RemoteVsDbgPath} --interpreter=vscode {vsdbgLogPath}")
      //
      //// var strictKeyChecking = " -o \"StrictHostKeyChecking no\"";
      ////
      ////var sshPassword = !_opts.UserPrivateKeyEnabled
      ////  ? $"-pw {RemoteUserPass}"
      ////  : $"-i \"{_opts.UserPrivateKeyPath}{strictKeyChecking}\"";
      string sshPassword = "";

      if(_opts.UseSSHExeEnabled)
      {
        sshPassword = ""; //nothing to do, we assume that c:\users\[user]\.ssh\id_rsa exists
      }
      else
      {
        sshPassword = $"-pw {RemoteUserPass}";
      }

      // TODO: Figure out why "-i <keyfile>" isn't working.
      if (string.IsNullOrEmpty(RemoteUserPass))
        Logger.Output("You must provide a User Password to debug.");

      string adapter = plinkPath;
      string adapterArgs = "";
      string displayAdapter = "";
      if(_opts.RemoteDebugDisplayGui)
      {
        displayAdapter = "DISPLAY=:0";
      }
      if (_opts.UseSSHExeEnabled)
      {
        adapterArgs = $"{sshPassword} {sshEndpoint} -T {displayAdapter} {_opts.RemoteVsDbgFullPath} {vsdbgLogPath}";
        //// adapterArgs = $"-ssh {sshPassword} {sshEndpoint} -batch -T {RemoteVsDbgFullPath} --interpreter=vscode {vsdbgLogPath}";
      }
      else
      {
        adapterArgs= $"-ssh {sshPassword} {sshEndpoint} -T {displayAdapter} {_opts.RemoteVsDbgFullPath} {vsdbgLogPath}";
      }

      return (adapter, adapterArgs);
    }

    /// <summary>Attempt to get the extension's local directory.</summary>
    /// <returns>Path of this VSIX or empty string.</returns>
    private string GetExtensionDirectory()
    {
      var path = string.Empty;
      try
      {
        var uri = new Uri(typeof(LaunchBuilder).Assembly.CodeBase, UriKind.Absolute);
        path = Path.GetDirectoryName(uri.LocalPath);
      }
      catch (Exception)
      {
      }

      return path;
    }
  }
}
