using System.Linq;
using System.Net;

namespace Xeno.VsLinuxDebug.Common
{
  public static class Helpers
  {
    public static IPAddress ResolveHost(string hostNameOrAddress)
    {
      IPAddress result;
      if (IPAddress.TryParse(hostNameOrAddress, out result))
        return result;
      else
        return Dns.GetHostEntry(hostNameOrAddress).AddressList.First();
    }
  }
}
