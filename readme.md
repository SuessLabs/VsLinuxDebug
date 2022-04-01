# [VS .NET SSH Debugger](https://github.com/SuessLabs/RemoteDebug.git)

<image align="right" width="200" height="200" src="https://github.com/SuessLabs/VsLinuxDebug/blob/master/docs/TuxDebug.png" />

Remotely deploy and debug your .NET C# apps via SSH to Linux using Visual Studio 2022.

Get it on the [VS MarketPlace](https://marketplace.visualstudio.com/items?itemName=SuessLabs.VSLinuxDebugger)!

> WARNING: This is a work in progress!

Visual Studio's "attach to process via SSH" is cute, but it lacks deployment and automatic attaching. This project aims to allow you to do just that when programming for your Linux VM or Raspberry Pi over the network.

This project was inspired by [VS Mono Debugger](https://github.com/GordianDotNet/VSMonoDebugger) and their amazing efforts for cross-platform development.

## Overview

Now developers can build, deploy and debug projects on their remote Linux (Ubuntu, Raspberry PI, etc) devices! Customize your SSH connection to use either a _password_ or a _private key_.

### Work in Progress

This project is currently in the early alpha stages, so only Building and Deployment is available. This extension aims to allow you to automatically attach for debugging over the network. For now, that step is still manual. On the plus side, we just saved you 1.5 min of manual upload and `chown -R`.

### Usage

![VS Menu](docs/ScreenShot-MenuItems.png)

* Build and upload to remote devices (_yes, this is a real pain_)
* Remote debugging (_Work-in-Progress_)

### Customize your connections

![Tools Options](docs/ScreenShot-ToolsOptions.png)

### Generating Private Key (optional)

The following steps are options if you wish to use an SSH Private Key. These steps were written for Windows 10, however, on Linux the steps are similar.

1. Open PowerShell:
2. **Generate key** (_with old PEM format_)
   1. `ssh-keygen -m PEM -t rsa -b 4096`
   2. In the future, we'll be able to use `ssh-keygen`.. just not yet.
3. Set output name (_default is okay for basic setups_)
4. Input a passphrase for the key _(OPTIONAL)_
5. Windows will now generate your RSA public/private key pair.
   1. Default location: `%UserProfile%\.ssh` (WINOWS)
   2. The public key will be stored as `id_rsa.pub` in the directory
6. **Upload the public key** to your remote machine
   1. Navigate to folder, `~/.ssh/` on Linux device
   2. If `~/.ssh/authorized_keys` exists, append the contents of `id_rsa.pub` to the next line.
   3. If it does not exist, simply upload `id_rsa.pub` and rename it to, `authorized_keys`
7. DONE!

## Action Items

In order to get this project moving, the following must be done.

* [X] Create extension project for VS2022
* [X] SSH, SFTP, and SCP communication
* [X] Store settings (globally; per-project)
  * [X] IP, User, Pass, default-folder `"~/VsLinuxDbg/(proj-name)"`
* [X] Perform upload to remote machine
* [ ] Attach to process - _in testing phase_

## Developers Wanted

Contributors and Q/A are welcomed!

To contribute, please pick off an item from the project or issue page. We'd love to hear your enhancement ideas as well.

## References

* [Extension Docs](https://docs.microsoft.com/en-us/visualstudio/extensibility/creating-a-settings-category?view=vs-2022)
* [Extension Sample](https://github.com/microsoft/VSSDK-Extensibility-Samples/tree/master/Options)
* [Offroad Debugging](https://github.com/Microsoft/MIEngine/wiki/Offroad-Debugging-of-.NET-Core-on-Linux---OSX-from-Visual-Studio)
