namespace VsLinuxDebugger.Core
{
  public enum BuildOptions
  {
    Build,
    Deploy,
    Publish,
    Debug,
    /// <summary>Launches application with `DISPLAY:=0`</summary>
    Launch,
  }
}
