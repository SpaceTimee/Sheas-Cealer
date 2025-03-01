using CommunityToolkit.Mvvm.ComponentModel;

namespace Sheas_Cealer_Nix.Preses;

internal partial class SettingsPres : GlobalPres
{
    internal SettingsPres()
    {
        //IsEnglishLang = Settings.Default.IsEnglishLang switch
        //{
        //    -1 => null,
        //    0 => false,
        //    1 => true,
        //    _ => throw new UnreachableException()
        //};

        //IsLightWeight = Settings.Default.IsLightWeight switch
        //{
        //    -1 => null,
        //    0 => false,
        //    1 => true,
        //    _ => throw new UnreachableException()
        //};
    }

    [ObservableProperty]
    private static bool? isEnglishLang = null;
    partial void OnIsEnglishLangChanged(bool? value)
    {
        //CultureInfo newCulture = value.HasValue ? new(value.Value ? "en" : "zh") : CultureInfo.InstalledUICulture;

        //Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = newCulture;

        //foreach (Window currentWindow in Application.Current.Windows)
        //    currentWindow.Language = XmlLanguage.GetLanguage(newCulture.IetfLanguageTag);

        //Settings.Default.IsEnglishLang = (sbyte)(value.HasValue ? value.Value ? 1 : 0 : -1);
        //Settings.Default.Save();
    }

    [ObservableProperty]
    private static bool? isLightWeight = null;
    partial void OnIsLightWeightChanged(bool? value)
    {
        //FontWeight newWeight = value.HasValue ? value.Value ? FontWeights.Light : FontWeights.Bold : FontWeights.Regular;
        //Style newWindowStyle = new(typeof(Window), Application.Current.Resources["CommonWindow"] as Style);

        //newWindowStyle.Setters.Add(new Setter(Window.FontWeightProperty, newWeight));
        //Application.Current.Resources["CommonWindow"] = newWindowStyle;

        //Style newButtonStyle = new(typeof(Button), Application.Current.Resources[typeof(Button)] as Style);

        //newButtonStyle.Setters.Add(new Setter(Button.FontWeightProperty, newWeight));
        //Application.Current.Resources[typeof(Button)] = newButtonStyle;

        //Settings.Default.IsLightWeight = (sbyte)(value.HasValue ? value.Value ? 1 : 0 : -1);
        //Settings.Default.Save();
    }
}