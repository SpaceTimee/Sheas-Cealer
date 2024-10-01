using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Props;
using File = System.IO.File;

namespace Sheas_Cealer.Preses;

internal partial class MainPres : ObservableObject
{
    internal MainPres(string[] args)
    {
        int browserPathIndex = Array.FindIndex(args, arg => arg.Equals("-b", StringComparison.OrdinalIgnoreCase)) + 1;
        int upstreamUrlIndex = Array.FindIndex(args, arg => arg.Equals("-u", StringComparison.OrdinalIgnoreCase)) + 1;
        int extraArgsIndex = Array.FindIndex(args, arg => arg.Equals("-e", StringComparison.OrdinalIgnoreCase)) + 1;

        BrowserPath = browserPathIndex != 0 && browserPathIndex != args.Length ?args[browserPathIndex]:
            !string.IsNullOrWhiteSpace(Settings.Default.BrowserPath) ? Settings.Default.BrowserPath :
            (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe", string.Empty, null) ??
            Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", string.Empty, null) ??
            Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\brave.exe", string.Empty, null) ??
            string.Empty).ToString()!;

        UpstreamUrl = upstreamUrlIndex == 0 || upstreamUrlIndex == args.Length ?
            !string.IsNullOrWhiteSpace(Settings.Default.UpstreamUrl) ? Settings.Default.UpstreamUrl : MainConst.DefaultUpstreamUrl :
            args[upstreamUrlIndex];

        ExtraArgs = extraArgsIndex == 0 || extraArgsIndex == args.Length ?
            !string.IsNullOrWhiteSpace(Settings.Default.ExtraArgs) ? Settings.Default.ExtraArgs : string.Empty :
            args[extraArgsIndex];
    }

    [ObservableProperty]
    private MainConst.SettingsMode settingsMode;

    [ObservableProperty]
    private bool? isLightTheme = null;
    partial void OnIsLightThemeChanged(bool? value)
    {
        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();

        newTheme.SetBaseTheme(value.HasValue ? value.GetValueOrDefault() ? BaseTheme.Light : BaseTheme.Dark : BaseTheme.Inherit);
        paletteHelper.SetTheme(newTheme);
    }

    [ObservableProperty]
    private string browserPath;
    partial void OnBrowserPathChanged(string value)
    {
        if (File.Exists(value) && Path.GetFileName(value).ToLowerInvariant().EndsWith(".exe"))
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
    partial void OnExtraArgsChanged(string value)
    {
        if (MainConst.ExtraArgsRegex().IsMatch(value))
        {
            Settings.Default.ExtraArgs = value;
            Settings.Default.Save();
        }
    }
}