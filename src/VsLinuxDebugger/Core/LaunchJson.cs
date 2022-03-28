using System.Collections.Generic;

namespace VsLinuxDebugger.Core
{
  public class LaunchJson
  {
    public LaunchJson()
    {
      Configurations = new();
    }

    public string Version => "0.2.0";

    public string Adapter { get; set; }

    public string AdapterArgs { get; set; }

    public List<LaunchJsonConfig> Configurations { get; set; }
  }
}
