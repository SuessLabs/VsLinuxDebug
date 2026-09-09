using System;

namespace VsLinuxDebugger.Core
{
  /// <summary>Combined via bitwise OR (i.e. `Build | Deploy | Debug`) and tested with
  /// <see cref="Enum.HasFlag"/> throughout RemoteDebugger -- values must stay distinct
  /// powers of two, or combinations silently collide (i.e. plain sequential values made
  /// `Debug` (3) already contain the `Deploy` (1) bit, so "Attach Only" (Debug alone)
  /// was silently treated as if Deploy/Publish had been requested too).</summary>
  [Flags]
  public enum BuildOptions
  {
    Build = 1 << 0,
    Deploy = 1 << 1,
    Publish = 1 << 2,
    Debug = 1 << 3,
    /// <summary>Launches application with `DISPLAY:=0`</summary>
    Launch = 1 << 4,
  }
}
