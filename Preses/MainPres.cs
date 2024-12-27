using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Props;
using System;
using System.Diagnostics;
using System.IO;
using File = System.IO.File;

namespace Sheas_Cealer.Preses;

internal partial class MainPres : GlobalPres
{
    internal MainPres()
    {
        string[] args = Environment.GetCommandLineArgs();

        int browserPathIndex = Array.FindIndex(args, arg => arg.Equals("-b", StringComparison.OrdinalIgnoreCase)) + 1;
        int upstreamUrlIndex = Array.FindIndex(args, arg => arg.Equals("-u", StringComparison.OrdinalIgnoreCase)) + 1;
        int extraArgsIndex = Array.FindIndex(args, arg => arg.Equals("-e", StringComparison.OrdinalIgnoreCase)) + 1;

        BrowserPath = browserPathIndex != 0 && browserPathIndex != args.Length ? args[browserPathIndex] :
            !string.IsNullOrWhiteSpace(Settings.Default.BrowserPath) ? Settings.Default.BrowserPath :
            (Registry.LocalMachine.OpenSubKey(MainConst.EdgeBrowserRegistryPath)?.GetValue(string.Empty, null) ??
            Registry.LocalMachine.OpenSubKey(MainConst.ChromeBrowserRegistryPath)?.GetValue(string.Empty, null) ??
            Registry.LocalMachine.OpenSubKey(MainConst.BraveBrowserRegistryPath)?.GetValue(string.Empty, null) ??
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
    private string browserPath;
    partial void OnBrowserPathChanged(string value)
    {
        if (!File.Exists(value) || !Path.GetFileName(value).ToLowerInvariant().EndsWith(".exe"))
            return;

        Settings.Default.BrowserPath = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private string upstreamUrl;
    partial void OnUpstreamUrlChanged(string value)
    {
        if (!MainConst.UpstreamUrlRegex().IsMatch(value))
            return;

        Settings.Default.UpstreamUrl = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private string extraArgs;
    partial void OnExtraArgsChanged(string value)
    {
        if (!MainConst.ExtraArgsRegex().IsMatch(value))
            return;

        Settings.Default.ExtraArgs = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private bool isUpstreamHostUtd = true;

    [ObservableProperty]
    private bool isCoproxyIniting = false;

    [ObservableProperty]
    private bool isCoproxyStopping = false;

    [ObservableProperty]
    private bool isConginxExist = File.Exists(MainConst.ConginxPath);

    [ObservableProperty]
    private bool isNginxExist = File.Exists(MainConst.NginxPath);

    [ObservableProperty]
    private bool isNginxIniting = false;

    [ObservableProperty]
    private bool isConginxRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainConst.ConginxPath)).Length != 0;

    [ObservableProperty]
    private bool isNginxRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainConst.NginxPath)).Length != 0;

    [ObservableProperty]
    private bool isComihomoExist = File.Exists(MainConst.ComihomoPath);

    [ObservableProperty]
    private bool isMihomoExist = File.Exists(MainConst.MihomoPath);

    [ObservableProperty]
    private bool isComihomoIniting = false;

    [ObservableProperty]
    private bool isMihomoIniting = false;

    [ObservableProperty]
    private bool isComihomoRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainConst.ComihomoPath)).Length != 0;

    [ObservableProperty]
    private bool isMihomoRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainConst.MihomoPath)).Length != 0;

    [ObservableProperty]
    private bool isFlashing = false;
}