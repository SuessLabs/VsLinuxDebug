using System;
using System.Threading;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace VsLinuxDebugger.Extensions
{
  public static class ProcessExtension
  {
    public static async Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
      var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

      void Process_Exited(object sender, EventArgs e)
      {
        tcs.TrySetResult(true);
      }

      process.EnableRaisingEvents = true;
      process.Exited += Process_Exited;

      try
      {
        if (process.HasExited)
        {
          return process.ExitCode;
        }

        using (cancellationToken.Register(() => tcs.TrySetCanceled()))
        {
          await tcs.Task.ConfigureAwait(false);
        }
      }
      finally
      {
        process.Exited -= Process_Exited;
      }

      return process.ExitCode;
    }
  }
}
