# [VS .NET SSH Debugger](https://github.com/SuessLabs/RemoteDebug.git)

Remotely deploy and debug your .NET C# apps via SSH to Linux using Visual Studio.

> WARNING: This is a work in progress!

Visual Studio's "attach to process via SSH" is cute, but it lacks deployment and automatic attaching. This project aims to allow you to do just that when programming for your Linux VM or Raspberry Pi over the network.

This project was inspired by [VS Mono Debugger](https://github.com/GordianDotNet/VSMonoDebugger).

## To Do

In order to get this project moving, the following must be done.

* [X] Create extension project for VS2022 (_VS2019 coming soon_).
* [X] SSH library
* [X] SFTP library
* [ ] Store settings (globally; per-project)
  * IP, User, Pass, default-folder `"~\.XRemoteDbg\(proj-name)"`
* [ ] TEST: Perform upload to remote machine
* [ ] TEST: Attach to process
* [ ] Figure out how to, OnRun: upload, attach, remote-debug

## References

* [Extension Docs](https://docs.microsoft.com/en-us/visualstudio/extensibility/creating-a-settings-category?view=vs-2022)
* [Extension Sample](https://github.com/microsoft/VSSDK-Extensibility-Samples/tree/master/Options)
* [Offroad Debugging](https://github.com/Microsoft/MIEngine/wiki/Offroad-Debugging-of-.NET-Core-on-Linux---OSX-from-Visual-Studio)