# Linux Debugger
# Build script for generating release package

if (Test-Path -Path "bin")
{
  Remove-Item bin\* -Recurse -Force
}

$VCToolsInstallDir = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -Latest -requires Microsoft.Component.MSBuild -property InstallationPath
Write-Host "VCToolsInstallDir: $VCToolsInstallDir"

$msBuildPath = "$VCToolsInstallDir\MSBuild\Current\Bin\msbuild.exe"
Write-Host "msBuildPath: $msBuildPath"

Write-Host "Cleaning..."
& $msBuildPath -t:Clean src/LinuxDebugger.sln

Write-Host "Building..."
& $msBuildPath /restore `
               src/LinuxDebugger.sln `
               /p:Configuration=Release

# TODO:
### https://github.com/madskristensen/VsctIntellisense/blob/master/appveyor.yml
#   (new-object Net.WebClient).DownloadString("https://raw.github.com/madskristensen/ExtensionScripts/master/AppVeyor/vsix.ps1") | iex
#   Vsix-IncrementVsixVersion .\src\VsctCompletion2019\source.extension.vsixmanifest | Vsix-UpdateBuildVersion
#   Vsix-IncrementVsixVersion .\src\VsctCompletion2022\source.extension.vsixmanifest
#   Vsix-TokenReplacement src\VsctCompletion2019\source.extension.cs 'Version = "([0-9\\.]+)"' 'Version = "{version}"'
#   Vsix-TokenReplacement src\VsctCompletion2022\source.extension.cs 'Version = "([0-9\\.]+)"' 'Version = "{version}"'
#   nuget restore -Verbosity quiet
#   msbuild /p:configuration=Release /p:DeployExtension=false /p:ZipPackageCompressionLevel=normal /v:m

## TODO: Build project and set version to 1.2.3.4
# dotnet build -p:Version=1.2.3.4
