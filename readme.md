# Remote Debuger

Remotely deploy and debug your C# apps onto your Raspberry Pi over the network.

## To Do

In order to get this project moving, the following must be done.

* [ ] Create extension project for VS2019, VS2022.
* [ ] SSH library
* [ ] SFTP library
* [ ] Store settings (globally; per-project)
  * IP, User, Pass, default-folder `"~\.XRemoteDbg\(proj-name)"`
* [ ] TEST: Perform upload to remote machine
* [ ] TEST: Attach to process
* [ ] Figure out how to, OnRu: upload, attach, remote-debug

## References

Many thanks to the efforts of [KSWoll's VS-Mono](https://github.com/kswoll/vs-mono) project.
