using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VsLinuxDebugger
{
  public enum OutputWindowType
  {
    Debug,
    General,
    Custom,
  };

  public static class Logger
  {
    private static string _name;
    private static IVsOutputWindowPane _outputPane;
    private static OutputWindowType _outputType = OutputWindowType.Debug;
    private static IServiceProvider _provider;

    private static string FormattedTime
    {
      get
      {
        return string.Format(
          "{0:00}:{1:00}:{2:00}.{3:000}",
          DateTime.Now.Hour,
          DateTime.Now.Minute,
          DateTime.Now.Second,
          DateTime.Now.Millisecond);
      }
    }

    public static void Init(IServiceProvider provider, OutputWindowType outputType = OutputWindowType.Debug, string name = "Remote Debugger")
    {
      _provider = provider;
      _outputType = outputType;
      _name = name;
    }

    public static void Output(string message)
    {
      var msg = $"{FormattedTime}: {message}{Environment.NewLine}";

      try
      {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (HasOutputWindow())
        {
          _outputPane.OutputStringThreadSafe(msg);
          _outputPane.Activate(); // Brings pane into view
        }
      }
      catch (Exception)
      {
        Console.Write($"Failed to Output: '{msg}'");
      }
    }

    private static bool HasOutputWindow()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      if (_outputPane == null && _provider != null)
      {
        Guid guid = Guid.NewGuid();

        ////IVsOutputWindow output = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
        ////if (output is null)
        ////  return false;

        IVsOutputWindow output = (IVsOutputWindow)_provider.GetService(typeof(SVsOutputWindow));
        if (output is null)
          return false;

        switch (_outputType)
        {
          case OutputWindowType.Debug:
            guid = VSConstants.GUID_OutWindowDebugPane;
            break;

          case OutputWindowType.General:
            guid = VSConstants.GUID_OutWindowGeneralPane;
            break;

          case OutputWindowType.Custom:
          default:
            output.CreatePane(ref guid, _name, 1, 1);
            break;
        }

        output.GetPane(ref guid, out _outputPane);
        ////output.Activate(); // Brings this pane into view
      }

      return _outputPane != null;

      // Reference:
      //  - https://stackoverflow.com/a/1852535/249492
      //  - https://docs.microsoft.com/en-us/visualstudio/extensibility/extending-the-output-window?view=vs-2022
      //  - https://github.com/microsoft/VSSDK-Extensibility-Samples/blob/master/Reference_Services/C%23/Reference.Services/HelperFunctions.cs
      //
      // TODO:
      //  1) Consider passing in IServiceProvider from Commands class
      //  2) Use the MS GitHub example
      //
      ////// TODO: Use main thread
      ////////await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
      ////ThreadHelper.ThrowIfNotOnUIThread();
      ////
      ////IVsOutputWindow output = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      ////
      ////// Guid debugPaneGuid = VSConstants.GUID_OutWindowDebugPane;
      ////Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane;
      ////IVsOutputWindowPane generalPane;
      ////output.GetPane(ref generalPaneGuid, out generalPane);
      ////
      ////generalPane.OutputStringThreadSafe(message);
      ////generalPane.Activate(); // Brings this pane into view
    }
  }
}
