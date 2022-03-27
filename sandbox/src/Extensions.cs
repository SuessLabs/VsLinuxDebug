using System;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;

namespace VSRemoteDebugger
{
    public static class Extensions
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

        /// <summary> 
        /// Gets the startup project for the given solution. 
        /// </summary> 
        /// <param name="solution">The solution.</param> 
        /// <returns><c>null</c> if the startup project cannot be found.</returns> 
        public static Project GetStartupProject(this Solution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Project ret = null;

            if (solution?.SolutionBuild?.StartupProjects != null)
            {
                string uniqueName = (string)((object[])solution.SolutionBuild.StartupProjects)[0];

                // Can't use the solution.Item(uniqueName) here since that doesn't work 
                // for projects under solution folders. 
                ret = GetProject(solution, uniqueName);
            }

            return ret;
        }

        /// <summary> 
        /// Gets the project located in the given solution. 
        /// </summary> 
        /// <param name="solution">The solution.</param> 
        /// <param name="uniqueName">The unique name of the project.</param> 
        /// <returns><c>null</c> if the project could not be found.</returns> 
        private static Project GetProject(Solution solution, string uniqueName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Project ret = null;

            if (solution != null && uniqueName != null)
            {
                foreach (Project p in solution.Projects)
                {
                    ret = GetSubProject(p, uniqueName);

                    if (ret != null)
                        break;
                }
            }

            return ret;
        }

        /// <summary> 
        /// Gets a project located under another project item. 
        /// </summary> 
        /// <param name="project">The project to start the search from.</param> 
        /// <param name="uniqueName">Unique name of the project.</param> 
        /// <returns><c>null</c> if the project can't be found.</returns> 
        /// <remarks>Only works for solution folders.</remarks> 
        private static Project GetSubProject(Project project, string uniqueName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Project ret = null;

            if (project != null)
            {
                if (project.UniqueName == uniqueName)
                {
                    ret = project;
                }
                else if (project.Kind == Constants.vsProjectKindSolutionItems)
                {
                    // Solution folder 
                    foreach (ProjectItem projectItem in project.ProjectItems)
                    {
                        ret = GetSubProject(projectItem.SubProject, uniqueName);

                        if (ret != null)
                            break;
                    }
                }
            }

            return ret;
        }
    }
}

