# Build script for generating release package

if (Test-Path -Path "bin")
{
  Remove-Item bin\* -Recurse -Force
}

# Clean both debug and release
##dotnet clean src/LinuxDebugger.sln
##dotnet clean src/LinuxDebugger.sln --configuration Release
dotnet msbuild src/LinuxDebugger.sln -t:Clean
dotnet msbuild src/LinuxDebugger.sln -t:Clean -p:Configuration=Release;TargetFrameworkVersion=v472

# build package for release
dotnet msbuild src/LinuxDebugger.sln -t:Rebuild -p:Configuration=Release
## dotnet build src/LinuxDebugger.sln --configuration release

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
