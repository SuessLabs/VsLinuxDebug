# Code Snips

## RemoteDebugger.cs

```cs
    /*
     * Borrowed from VSMonoDebugger
     *
    public async Task BuildStartupProjectAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var failedBuilds = BuildStartupProject();
      if (failedBuilds > 0)
      {
        Window window = _dte.Windows.Item("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}");//EnvDTE.Constants.vsWindowKindOutput
        OutputWindow outputWindow = (OutputWindow)window.Object;
        outputWindow.ActivePane.Activate();
        outputWindow.ActivePane.OutputString($"{failedBuilds} project(s) failed to build. See error and output window!");

        //// _errorListProvider.Show();

        throw new Exception($"{failedBuilds} project(s) failed to build. See error and output window!");
      }
    }

    private int BuildStartupProject()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      //// var dte = (DTE)Package.GetGlobalService(typeof(DTE));
      var sb = (SolutionBuild2)_dte.Solution.SolutionBuild;

      try
      {
        var startProject = GetStartupProject();
        var activeConfiguration = _dte.Solution.SolutionBuild.ActiveConfiguration as SolutionConfiguration2;
        var activeConfigurationName = activeConfiguration.Name;
        var activeConfigurationPlatform = activeConfiguration.PlatformName;
        var startProjectName = startProject.FullName;

        sb.BuildProject($"{activeConfigurationName}|{activeConfigurationPlatform}", startProject.FullName, true);
      }
      catch (Exception ex)
      {
        // Build complete solution (fallback solution)
        return BuildSolution();
      }

      return sb.LastBuildInfo;
    }

    private int BuildSolution()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      var sb = (SolutionBuild2)_dte.Solution.SolutionBuild;
      sb.Build(true);
      return sb.LastBuildInfo;
    }
    */
```