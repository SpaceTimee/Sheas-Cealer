using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using Sheas_Cealer.Props;

namespace Sheas_Cealer.Preses;

internal partial class SettingsPres : GlobalPres
{
    internal SettingsPres()
    {
        IsEnglishLang = Settings.Default.IsEnglishLang switch
        {
            -1 => null,
            0 => false,
            1 => true,
            _ => throw new UnreachableException()
        };
    }

    [ObservableProperty]
    private static bool? isEnglishLang = null;
    partial void OnIsEnglishLangChanged(bool? value)
    {
        CultureInfo newCulture = value.HasValue ? new(value.Value ? "en" : "zh") : CultureInfo.InstalledUICulture;

        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = newCulture;

        foreach (Window currentWindow in Application.Current.Windows)
            currentWindow.Language = XmlLanguage.GetLanguage(newCulture.IetfLanguageTag);

        Settings.Default.IsEnglishLang = (sbyte)(value.HasValue ? value.Value ? 1 : 0 : -1);
        Settings.Default.Save();
    }
}