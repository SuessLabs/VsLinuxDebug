# [VS .NET SSH Debugger](https://github.com/SuessLabs/RemoteDebug.git)

Remotely deploy and debug your .NET C# apps via SSH to Linux using Visual Studio 2022.

> WARNING: This is a work in progress!

Visual Studio's "attach to process via SSH" is cute, but it lacks deployment and automatic attaching. This project aims to allow you to do just that when programming for your Linux VM or Raspberry Pi over the network.

This project was inspired by [VS Mono Debugger](https://github.com/GordianDotNet/VSMonoDebugger) and their amazing efforts for cross-platform development.

## Overview

Allow developers to build, deploy and debug projects on their remote Linux (Ubuntu, Raspberry PI, etc) devices! Users can choose to use SSH with either a _private key_ or _password_.

### Usage

![VS Menu](docs/ScreenShot-MenuItems.png)

* Build and upload to remote devices (_yes, this is a real pain_)
* Remote debugging (_Work-in-Progress_)

### Customize your connections

![Tools Options](docs/ScreenShot-ToolsOptions.png)

## Action Items

In order to get this project moving, the following must be done.

* [X] Create extension project for VS2022
* [X] SSH, SFTP, and SCP communication
* [X] Store settings (globally; per-project)
  * [X] IP, User, Pass, default-folder `"~/VsLinuxDbg/(proj-name)"`
* [X] Perform upload to remote machine
* [ ] Attach to process

## References

* [Extension Docs](https://docs.microsoft.com/en-us/visualstudio/extensibility/creating-a-settings-category?view=vs-2022)
* [Extension Sample](https://github.com/microsoft/VSSDK-Extensibility-Samples/tree/master/Options)
* [Offroad Debugging](https://github.com/Microsoft/MIEngine/wiki/Offroad-Debugging-of-.NET-Core-on-Linux---OSX-from-Visual-Studio)