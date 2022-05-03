# Release Notes

## 1.9.0

* Added: Now comes with PLink embedded. You can still manually set this if you prefer.
* Removed: Option to enable/disable PLink

## 1.8.1

* Fixed: Remote folder pre-cleanup.
* Added: Upload files async to reduce locking of Visual Studio
* Added: Removal of `launch.json` if it previously existed
* Added: More output logging.
* Update: Enhanced Output
* Updated: Output Window drop-down title to "Linux Debugger" from "Remote Debugger"

## 1.8.0

* Added: Logging to Output window under "_Remote Debugging_" dropdown
* Update: Do not include `launch.json` in the uploaded `.tar.gz` package
* Update: Readme notes
* Update: Code cleanup

## 1.7.0

* Fixed: Remote debugging for PLink
* Fixed: VSDBG default path
* Update: DeleteLaunchJsonAfterBuild set to false by default
* Update: Separated LaunchJson class into separate objects
* Updated: SSH parameters to include `-o` (option) for `StrictHostKeyChecking = NO`.
* Added: Additional internal logging
* Added: documentation to Launch and Configure classes

## 1.6.0

* Added: Ability to use SSH KeyFile with or without a passphrase.
* Added: Directions for creating and configuring local and remote devices
* Added: Additional directions in the Docs folder

## 1.2.0

* Removed: Publish option
* Updated Options page defaults
* Update: Remote output folder is now the assembly name
* Update: Remote output folder only clears intended target sub-folder
* Added: Remote Debugging (_still in preview stages.._)

## 1.1.1

* Updated: Branding name
* Removed: Temp disabled remote debugger

## 1.0.1

* Update: Remote output folder now creates subfolders with the same name as your project.
* Updated: project icon

## 1.0.0

* Initial release
