// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello .NET 6, VS Linux Debugger!");

#if DEBUG
// Console.ReadLine();
#endif


Console.WriteLine("Apply breakpoint here!");
System.Diagnostics.Debugger.Break();

Console.WriteLine("All done!");

////Console.WriteLine("Press anykey to exit..");
////var x = Console.ReadLine();

