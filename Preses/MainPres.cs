using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Props;
using Sheas_Cealer.Wins;
using File = System.IO.File;

namespace Sheas_Cealer.Preses;

internal partial class MainPres : ObservableObject
{
    internal MainPres(string[] args)
    {
        int browserPathIndex = Array.FindIndex(args, arg => arg == "-b") + 1,
        upstreamUrlIndex = Array.FindIndex(args, arg => arg == "-u") + 1,
        extraArgsIndex = Array.FindIndex(args, arg => arg == "-e") + 1;

        BrowserPath = browserPathIndex == 0 ?
            (!string.IsNullOrWhiteSpace(Settings.Default.BrowserPath) ? Settings.Default.BrowserPath : string.Empty) :
            args[browserPathIndex];

        UpstreamUrl = upstreamUrlIndex == 0 ?
            (!string.IsNullOrWhiteSpace(Settings.Default.UpstreamUrl) ? Settings.Default.UpstreamUrl : MainConst.DefaultUpstreamUrl) :
            args[upstreamUrlIndex];

        ExtraArgs = extraArgsIndex == 0 ?
            (!string.IsNullOrWhiteSpace(Settings.Default.ExtraArgs) ? Settings.Default.ExtraArgs : string.Empty) :
            args[extraArgsIndex];

        if (Array.Exists(args, args => args == "-d"))
            new MainWin([]).StartCealButton_Click(null!, null!);
    }

    [ObservableProperty]
    private MainConst.SettingsMode mode;

    [ObservableProperty]
    private bool? isLightTheme = null;
    partial void OnIsLightThemeChanged(bool? value)
    {
        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();
        newTheme.SetBaseTheme(value.HasValue ? (value.GetValueOrDefault() ? BaseTheme.Light : BaseTheme.Dark) : BaseTheme.Inherit);
        paletteHelper.SetTheme(newTheme);
    }

    [ObservableProperty]
    private string browserPath;
    partial void OnBrowserPathChanged(string value)
    {
        if (File.Exists(value) && Path.GetFileName(value).ToLower().EndsWith(".exe"))
        {
            Settings.Default.BrowserPath = value;
            Settings.Default.Save();
        }
    }

    [ObservableProperty]
    private string upstreamUrl;
    partial void OnUpstreamUrlChanged(string value)
    {
        if (MainConst.UpstreamUrlRegex().IsMatch(value))
        {
            Settings.Default.UpstreamUrl = value;
            Settings.Default.Save();
        }
    }

    [ObservableProperty]
    private string extraArgs;
    private partial void OnExtraArgsChanged(string value)
    {
        if (MainConst.ExtraArgsRegex().IsMatch(value))
        {
            Settings.Default.ExtraArgs = value;
            Settings.Default.Save();
        }
    }
}