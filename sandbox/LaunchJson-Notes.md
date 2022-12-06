# Launch.JSON Notes

## Launch for Debugging Example

```json
{
  "version": "0.2.0",
  "adapter": "C:\\work\\tools\\PuTTY\\plink.exe",
  "adapterArgs": "-pw XXXX USERNAME@IPADDRESS -batch -T ~/.vsdbg/vsdbg --interpreter=vscode",
  "configurations": [
    {
      "name": ".NET Core Launch",
      "type": "coreclr",
      "request": "launch",
      "program": "dotnet",
      "args": ["ConsoleApp1.dll"],
      "cwd": "./VSLinuxDbg/",
      "console": "internalConsole",
      "stopAtEntry": true
    }
  ]
}
```

## Attach and Launch Example

```json
{
  "version": "0.2.0",
  "adapter": "c:\\work\\tools\\putty\\plink.exe",

  // NOTE: Must include ~/vsdbg
  "adapterArgs": "-pw XXXX USERNAME@IPADDRESS -batch -T ~/vsdbg/vsdbg",
  "configurations": [
    {
      "name": ".NET Launch",
      "type": "coreclr",
      "request": "launch",
      "program": "dotnet",
      "args": [ "LaunchTester.dll" ],
      // MUST USE "~/VSLinuxDbg/..."
      "cwd": "./VSLinuxDbg/LaunchTester",
      "console": "internalConsole"
    },
    {
      "name": ".NET Attach",
      "type": "coreclr",
      "request": "attach",
      "program": "dotnet",
      "args": [ "LaunchTester.dll" ],
      "cwd": "./VSLinuxDbg/LaunchTester",
      "console": "internalConsole"
    }
  ]
}
```

## Notes

```json
{
  "version": "0.2.0",

  // WORKS!
  "adapter": "c:\\work\\tools\\putty\\plink.exe",
  "adapterArgs": "-pw XXXXXXX USERNAME@IPADDRESS -batch -T ~/vsdbg/vsdbg",

  // Port Number causes issues
  // "adapter": "ssh.exe",
  // "adapterArgs": "-i C:\\Users\\USERNAME\\.ssh\\id_rsa USERNAME@IPADDRESS:22 vsdbg --interpreter=vscode  --engineLogging=./VSLinuxDbg/LaunchTester/_vsdbg.log",

  // Connects and hangs.. needs password
  // "adapter": "ssh.exe",
  // "adapterArgs": "-i C:\\Users\\USERNAME\\.ssh\\id_rsa USERNAME@IPADDRESS ~/vsdbg/vsdbg  --engineLogging=./VSLinuxDbg/LaunchTester/_vsdbg.log",

  // Fails
  // "adapter": "ssh.exe",
  // "adapterArgs": "-pw XXXXXXXX USERNAME@IPADDRESS ~/vsdbg/vsdbg  --engineLogging=./VSLinuxDbg/LaunchTester/_vsdbg.log",


  "configurations": [
    {
      "name": ".NET Launch",
      "type": "coreclr",
      "request": "launch",
      "program": "dotnet",
      "args": [ "LaunchTester.dll" ],
      "cwd": "~/VSLinuxDbg/LaunchTester"
    },
    {
      "name": ".NET Attach",
      "type": "coreclr",
      "request": "attach",
      "program": "dotnet",
      "args": [
        "LaunchTester.dll"
      ],
      "cwd": "./VSLinuxDbg/LaunchTester",
      "console": "internalConsole"
    }
  ]
}
```
