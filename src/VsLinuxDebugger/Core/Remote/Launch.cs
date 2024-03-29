﻿using System.Collections.Generic;

// <auto-generated Note="Disabling CodeMaid sorting />
namespace VsLinuxDebugger.Core.Remote
{
  public class Launch
  {
    private string _remoteDotNetPath;
    private string[] _args;
    private string _remoteOutputFolder;
    private string _environmentVariables;
    private bool _stopAtEntry;

    /// <summary>Launch JSON class</summary>
    /// <param name="dotNetPath">Remote machine DotNet path</param>
    /// <param name="remoteAppFileName">Name of app(.dll) or full path on remote machine.</param>
    /// <param name="remoteOutputFolder">Working directory (CWD) where app resides.</param>
    /// <param name="envVariables">Custom environment variables.</param>
    public Launch(string dotNetPath, string remoteAppFileName, string remoteOutputFolder, string envVariables = default, bool stopAtEntry = false)
    {
      ////string appPath = LinuxPath.Combine(remoteDebugFolder, $"{appName}.dll");
      string appToLaunch = remoteAppFileName;

      System.IO.Path.Combine("");
      _remoteDotNetPath = dotNetPath;
      _args = new string[] { appToLaunch };
      _remoteOutputFolder = remoteOutputFolder;
      _environmentVariables = envVariables;
      _stopAtEntry = stopAtEntry;

      Configurations = new List<Configuration>
      {
        new Configuration
        {
          Program = _remoteDotNetPath,
          Args = _args,
          Cwd = _remoteOutputFolder,
          StopAtEntry = _stopAtEntry,
          Env = _environmentVariables,
        }
      };
    }

    public string Version => "0.2.0";

    public string Adapter { get; set; }

    public string AdapterArgs { get; set; }

    public List<Configuration> Configurations { get; set; }
  }
}
