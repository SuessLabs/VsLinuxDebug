# [VS .NET Linux Debugger](https://github.com/SuessLabs/VsLinuxDebug)

<image align="right" width="200" height="200" src="https://github.com/SuessLabs/VsLinuxDebug/blob/master/docs/TuxDebug.png" />

Remotely deploy and debug your .NET C# apps via SSH to Linux using Visual Studio 2022.

Get it on the [VS MarketPlace](https://marketplace.visualstudio.com/items?itemName=SuessLabs.VSLinuxDebugger)!

Visual Studio's "attach to process via SSH" is cute, but it lacks deployment and automatic attaching. This project allows you to do just that on your Linux VM or Raspberry Pi over the network!

Suess Labs consulting is sponsored by _Xeno Innovations, Inc._

## Overview

Now developers can build, deploy and debug projects on their remote Linux (Ubuntu, Raspberry PI, etc) devices! Customize your SSH connection to use either a _password_ or a _private key_.

If you enjoy using the extension, please give it a ★★★★★ rating on the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=SuessLabs.VSLinuxDebugger).

### Usage

![Menu Screenshot](https://raw.githubusercontent.com/SuessLabs/VsLinuxDebug/develop/docs/ScreenShot-MenuItems.png)

* Build and upload to remote devices
* Remote debugging*
  * _This is still in the experimental stages. Please use VS' Attach to Process if you have issues_
* VS Linux Debugger will automatically detect and install `vsdbg` for you!

For GUI app debugging, you can use the _Build and Deploy_ feature, however, you must manually _Attach to Process_ via SSH using Visual Studio at this time.

### Getting Started

**Linux**, we'll need **SSH** and **cURL** for access and downloading any missing tools:

```bash
sudo apt install openssh-server
sudo apt install curl
```

**Windows**:

1. Open Visual Studio (VS) > Tools > Options > **Linux Debugger**
2. **Input:** Remote Host IP address
3. **Input:** Remote's User Name and Password
4. VS > Extensions > Linux Debugger > **Build, Deploy, Debug**

![Tools Options](https://raw.githubusercontent.com/SuessLabs/VsLinuxDebug/develop/docs/ScreenShot-ToolsOptions.png)

### Manually Attaching (for GUI apps)

For GUI projects, you can use **Build and Deploy** and then manually attach to the process via SSH by using Visual Studio's built-in tool

1. Deploy to remote machine via
   1. Extensions > Linux Debugger > **"Build and Deploy"**
2. Run GUI app on remote machine
   1. `dotnet MyGuiApp.dll`
3. Debug > **"Attach to Process.."**
4. Connection Type: **SSH**
5. Connection Target: **(Remote machine's IP)**
6. (Select process)
7. Click, **Attach**
8. Check, **"Managed (.NET Core for Unix)"**
9. Click, **OK**

This will save you 1.5 minutes on every build of manual uploading and updating rights via `chown -R`.

## Developers Wanted

Contributors and Q/A are welcomed!

To contribute, please pick off an item from the project or issue page. We'd love to hear your enhancement ideas as well.

## Revision History

This document contains the release information for the project.

### 2.1

* Update: Code cleanup
* Update: Moved RSA SHA256 classes into their own files for separation of responsibility
* Update: Enabled `SSH` for remote debugging by default instead of using `PLink` (_still experimental_)
* Removed: Out of date files (.NET 5 sample applications, UpgradeLog.htm, LinuxDebugger-2019.sln)

### 2.0.3.2 (Preview)

Contributor: [ZeuSWarE GmbH](https://github.com/zeusware) - PR: #62

* Added: The RSA-SHA2-256 for SSH.Net for interpreting private keys made by `ssh-keygen -m PEM -t rsa -b 4096` allowing connecting directly.
  * Ubuntu 22.04 LTS OpenSSH package does not support the ssh algo: `ssh-rsa` you intended being generating via `ssh-keygen -m PEM -t rsa -b 4096` by default anymore
* Added: Using a ppk via plink is obsolete if u gen the key via PowerShell `ssh-keygen`, so u have now the option to use the system integrated ssh.exe (PS 6 integrate that by default)

### 2.0.3

Contributor: [Claas Hilbrecht](https://github.com/clahil-linum) - PR: #59

* Fixed: As per #32 add `vsdbg` constant to debugger full path.
* Update: Options descriptions are now more clear about the .NET executable. It's not a path but the dotnet executable.

### 2.0.2

* Fixed: As per #53, cleaned up exponential Build status messages.
* Added: Submenu item to "Options" to quickly access to Linux Debugger's Options dialog
* Update: Refactored options mechanism in prep for custom profiles.

### 2.0.1 (Prev-1)

* Added: Option to set output window focus to Linux Debugger, default=`false`. (PR #46)
  * `Tools > Options > Linux Debugger > "Switch to LinuxDbg Output on Build"`
* Added: Async BASH and SFTP operations to not lock up Visual Studio (PR #40)
* Added: "Experimental" tag to menu items for Alpha/Beta items. (PR# 41)
  * `Build, Deploy, and Debug`
  * `Build, Deploy, and Launch` - _Temp disabled in Preview-1_
* Added: Deploy and Launch (**ALPHA Feature**) (PR #36)
* Added: BashSudo (PR #36)
* Update: Default VSDBG path to match Visual Studio 2022's deployed path (`~/.vs-debugger/vs2022/`). (PR #36, #47)
* Update: Sample's NuGet package for Prism.Avalonia
* Fixed: Typo, "Build was notsuccessful" (PR #43) `User Contribution` :rocket:
* Fixed: Auto-install cURL (PR #36)
* Fixed: Reduced duplicate output messages (PR #40)
* Removed: Publish (PR #36)
* Removed: Redundant sample project

### 1.9.0

* Added: Now comes with PLink embedded. You can still manually set this if you prefer.
* Removed: Option to enable/disable PLink

### 1.8.1

* Fixed: Remote folder pre-cleanup.
* Added: Upload files async to reduce locking of Visual Studio
* Added: Removal of `launch.json` if it previously existed
* Added: More output logging.
* Update: Enhanced Output
* Updated: Output Window drop-down title to "Linux Debugger" from "Remote Debugger"

### 1.8.0

* Added: Logging to Output window under "_Remote Debugging_" dropdown
* Update: Do not include `launch.json` in the uploaded `.tar.gz` package
* Update: Readme notes
* Update: Code cleanup

### 1.7.0

* Fixed: Remote debugging for PLink
* Fixed: VSDBG default path
* Update: DeleteLaunchJsonAfterBuild set to false by default
* Update: Separated LaunchJson class into separate objects
* Updated: SSH parameters to include `-o` (option) for `StrictHostKeyChecking = NO`.
* Added: Additional internal logging
* Added: documentation to Launch and Configure classes

### 1.6.0

* Added: Ability to use SSH KeyFile with or without a passphrase.
* Added: Directions for creating and configuring local and remote devices
* Added: Additional directions in the Docs folder

### 1.2.0

* Removed: Publish option
* Updated Options page defaults
* Update: Remote output folder is now the assembly name
* Update: Remote output folder only clears intended target sub-folder
* Added: Remote Debugging (_still in preview stages.._)

### 1.1.1

* Updated: Branding name
* Removed: Temp disabled remote debugger

### 1.0.1

* Update: Remote output folder now creates subfolders with the same name as your project.
* Updated: project icon

### 1.0.0

* Initial release
