namespace VsLinuxDebugger.Core
{
  public static class Constants
  {
    /// <summary>Filename of Visual Studio Debugger.</summary>
    public const string AppVSDbg = "vsdbg";

    public const string DefaultDotNetPath = "dotnet";
    public const string VS2022 = "vs2022";
    public const string DefaultVsdbgBasePath = "~/.vs-debugger";
    public const string LaunchJson = "launch.json";

    /// <summary>Default command used to elevate the debugger when <c>UseSudoForDebugger</c> is enabled.
    /// No '-E' (preserve environment): many sudoers configs don't allow it, causing 'sorry, you are
    /// not allowed to preserve the environment'. Users who need it can still add it themselves.</summary>
    public const string DefaultSudoCommand = "sudo -n";

    public const string PackageTarGz = "vsldBuildContents.tar.gz";
  }
}
