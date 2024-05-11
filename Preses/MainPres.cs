using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Sheas_Cealer.Consts;
using File = System.IO.File;

namespace Sheas_Cealer.Preses
{
    internal partial class MainPres : ObservableObject
    {
        private static readonly MainConst MainConst = new();

        internal MainPres(string[] args)
        {
            if (args.Length > 0)
                BrowserPath = args[0];
            else if (!string.IsNullOrWhiteSpace(Props.Settings.Default.BrowserPath))
                BrowserPath = Props.Settings.Default.BrowserPath;

            if (!string.IsNullOrWhiteSpace(Props.Settings.Default.UpstreamUrl))
                UpstreamUrl = Props.Settings.Default.UpstreamUrl;

            if (!string.IsNullOrWhiteSpace(Props.Settings.Default.ExtraArgs))
                ExtraArgs = Props.Settings.Default.ExtraArgs;
        }

        [ObservableProperty]
        private MainConst.Mode mode = MainConst.Mode.browserPathMode;

        [ObservableProperty]
        private bool isContentBoxFocused = true;

        [ObservableProperty]
        private string browserPath = string.Empty;
        partial void OnBrowserPathChanged(string value)
        {
            if (File.Exists(value) && Path.GetFileName(value).ToLower().EndsWith(".exe"))
            {
                Props.Settings.Default.BrowserPath = value;
                Props.Settings.Default.Save();
            }
        }

        [ObservableProperty]
        private string upstreamUrl = MainConst.DefaultUpstreamUrl;
        partial void OnUpstreamUrlChanged(string value)
        {
            if (MainConst.UrlRegex().IsMatch(value))
            {
                Props.Settings.Default.UpstreamUrl = value;
                Props.Settings.Default.Save();
            }
        }

        [ObservableProperty]
        private string extraArgs = string.Empty;
        partial void OnExtraArgsChanged(string value)
        {
            if (MainConst.ArgsRegex().IsMatch(value))
            {
                Props.Settings.Default.ExtraArgs = value;
                Props.Settings.Default.Save();
            }
        }
    }
}