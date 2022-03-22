# [Remote Debuger](https://github.com/SuessLabs/RemoteDebug.git)

Remotely deploy and debug your C# apps onto your Raspberry Pi over the network.

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

* [KSWoll's VS-Mono](https://github.com/kswoll/vs-mono) project.
* [VS Mono Debugger](https://github.com/GordianDotNet/VSMonoDebugger) - VS2017, VS2019
