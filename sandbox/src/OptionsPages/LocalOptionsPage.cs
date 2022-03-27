using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace VSRemoteDebugger.OptionsPages
{
    public class LocalOptionsPage : DialogPage
    {
        [Category("Local Machine Settings")]
        [DisplayName("Publish")]
        [Description("Publishes the solution rather than building it. Useful for ASP.NET/Blazor projects. Only compatible with .NET Core due to inherit limitations in Visual Studio")]
        public bool Publish { get; set; } = false;

        [Category("Local Machine Settings")]
        [DisplayName("Use Command Line Arguments")]
        [Description("Uses command line arguments picked up from (Visual Studio) Project Settings -> Debugging -> Command Line Arguments." +
            "Does not work properly if using more than one debugging profiles, please set to false if that is the case")]
        public bool UseCommandLineArgs { get; set; } = false;

        [Category("Local Machine Settings")]
        [DisplayName("Display GUI")]
        [Description("Display application on remote machine. This is helpful for debugging GUI applications on remote devices.")]
        public bool DisplayInGui { get; set; } = true;


        [Category("Local Machine Settings")]
        [DisplayName("No Debug (just build and deploy to remote)")]
        [Description("If you simply want to deploy changes to the remote machine without starting a debug session, set this flag to true.")]
        public bool NoDebug { get; set; } = false;
    }
}
