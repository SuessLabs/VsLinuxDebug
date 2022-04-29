using System;

namespace LaunchTester
{
  public class Program
  {
    public static void Main(string[] args)
    {
      // Quick Connection Test with PLINK.EXE:
      // 1. Open Command Prompt
      // 2. > c:\PATH-TO\putty\plink -pw XXXXXX damian@192.168.1.205 -batch -T ~/vsdbg/vsdbg
      //
      // Test manual launcher
      // 1. Upload files to remote machine
      // 2. Configure the 'launch.json'
      //    Adapter:      "c:\\work\\tools\\putty\\plink.exe",
      //    AdapterArgs:  "-pw XXXXX damian@192.168.1.205 -batch -T ~/vsdbg/vsdbg",
      //
      //
      // 3. Open, "Comman Window"
      //    Visual Studio > View > Other Windows > Command Window
      // 4. Input:
      //    >DebugAdapterHost.Launch /LaunchJson:LaunchTester\Properties\launch.json /ConfigurationName:".NET Launch"
      //

      // 4. >DebugAdapterHost.Launch /LaunchJson:LaunchTester\Properties\launch.json /ConfigurationName:".NET Launch"
      //    -- Ensure LaunchJson path is correct
      //    -- Ensure ConfigurationName matches a configuration in 'launch.json'
      // 5. FAILURE!
      //    ERROR: "Failed to launch debug adapter..."
      //    Output > Debug: "The program '' has exited with code 4294967295 (0xffffffff)."
      // 6. Figure out why this is failing.
      //
      Console.WriteLine("Hello .NET 5, VS Linux Debugger!");

      Console.WriteLine("Apply breakpoint here!");

      Console.WriteLine("Press anykey to exit..");
      var x = Console.ReadLine();
    }
  }
}
