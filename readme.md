# [Remote Debuger](https://github.com/SuessLabs/RemoteDebug.git)

Remotely deploy and debug your .NET C# apps via SSH using Visual Studio.

> WARNING: This is a work in progress!

Visual Studio's "attach to process via SSH" is cute, but it lacks deployment and automatic attaching. This project aims to allow you to do just that when programming for your Linux VM or Raspberry Pi over the network.

## To Do

In order to get this project moving, the following must be done.

* [ ] Create extension project for VS2022 and VS2019.
* [ ] SSH library
* [ ] SFTP library
* [ ] Store settings (globally; per-project)
  * IP, User, Pass, default-folder `"~\.XRemoteDbg\(proj-name)"`
* [ ] TEST: Perform upload to remote machine
* [ ] TEST: Attach to process
* [ ] Figure out how to, OnRun: upload, attach, remote-debug

## References

* [Extension Docs](https://docs.microsoft.com/en-us/visualstudio/extensibility/creating-a-settings-category?view=vs-2022)
* [Extension Sample](https://github.com/microsoft/VSSDK-Extensibility-Samples/tree/master/Options)
* [KSWoll's VS-Mono](https://github.com/kswoll/vs-mono) project.
* [VS Mono Debugger](https://github.com/GordianDotNet/VSMonoDebugger) - VS2017, VS2019
* [VS Mono Debugger](https://github.com/radutomy/VSRemoteDebugger)
