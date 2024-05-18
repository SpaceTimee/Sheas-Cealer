using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Sheas_Cealer.Consts;
using File = System.IO.File;

namespace Sheas_Cealer.Preses;

internal partial class MainPres : ObservableObject
{
    internal MainPres(string[] args)
    {
        int browserPathIndex = Array.FindIndex(args, arg => arg == "-b") + 1,
        upstreamUrlIndex = Array.FindIndex(args, arg => arg == "-u") + 1,
        extraArgsIndex = Array.FindIndex(args, arg => arg == "e") + 1;

        BrowserPath = browserPathIndex == 0 ?
            (!string.IsNullOrWhiteSpace(Props.Settings.Default.BrowserPath) ? Props.Settings.Default.BrowserPath : string.Empty) :
            args[browserPathIndex];

        UpstreamUrl = upstreamUrlIndex == 0 ?
            (!string.IsNullOrWhiteSpace(Props.Settings.Default.UpstreamUrl) ? Props.Settings.Default.UpstreamUrl : MainConst.DefaultUpstreamUrl) :
            args[upstreamUrlIndex];

        ExtraArgs = extraArgsIndex == 0 ?
            (!string.IsNullOrWhiteSpace(Props.Settings.Default.ExtraArgs) ? Props.Settings.Default.ExtraArgs : string.Empty) :
            args[extraArgsIndex];
    }

    [ObservableProperty]
    private MainConst.SettingsMode mode = MainConst.SettingsMode.BrowserPathMode;

    [ObservableProperty]
    private string browserPath;
    partial void OnBrowserPathChanged(string value)
    {
        if (File.Exists(value) && Path.GetFileName(value).ToLower().EndsWith(".exe"))
        {
            Props.Settings.Default.BrowserPath = value;
            Props.Settings.Default.Save();
        }
    }

    [ObservableProperty]
    private string upstreamUrl;
    partial void OnUpstreamUrlChanged(string value)
    {
        if (MainConst.UpstreamUrlRegex().IsMatch(value))
        {
            Props.Settings.Default.UpstreamUrl = value;
            Props.Settings.Default.Save();
        }
    }

    [ObservableProperty]
    private string extraArgs;
    partial void OnExtraArgsChanged(string value)
    {
        if (MainConst.ExtraArgsRegex().IsMatch(value))
        {
            Props.Settings.Default.ExtraArgs = value;
            Props.Settings.Default.Save();
        }
    }
}