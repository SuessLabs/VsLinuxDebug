# Samples

Documentation assistance for `launch.json` file.

## Sample from VSMonoDebugger

```json
{
  "version": "0.2.0",
  "adapter": "$(PLINK_EXE_PATH)",
  "adapterArgs": "$(PLINK_SSH_CONNECTION_ARGS) -batch -T vsdbg --interpreter=vscode",
  "configurations": [
    {
      "name": ".NET Core Launch (console)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "dotnet",
      "args": [
        "$(TARGET_EXE_FILENAME)",
        "$(START_ARGUMENTS)"
      ],
      "cwd": "$(DEPLOYMENT_PATH)",
      "console": "internalConsole",
      "stopAtEntry": true
    }
  ]
}
```