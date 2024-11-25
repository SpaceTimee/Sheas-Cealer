using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Utils;

namespace Sheas_Cealer.Preses;

internal partial class GlobalPres : ObservableRecipient, IRecipient<PropertyChangedMessage<object>>
{
    internal GlobalPres() => IsActive = true;

    [ObservableProperty, NotifyPropertyChangedRecipients]
    private bool? isLightTheme = null;
    partial void OnIsLightThemeChanged(bool? value)
    {
        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();

        newTheme.SetBaseTheme(value.HasValue ? value.GetValueOrDefault() ? BaseTheme.Light : BaseTheme.Dark : BaseTheme.Inherit);
        paletteHelper.SetTheme(newTheme);

        BorderThemeSetter.SetBorderTheme(Application.Current.MainWindow, value);
    }

    protected override void Broadcast<T>(T oldValue, T newValue, string? propertyName) => Messenger.Send(new PropertyChangedMessage<object>(this, propertyName, oldValue!, newValue!));

    public void Receive(PropertyChangedMessage<object> message) => GetType().GetProperty(message.PropertyName!)!.SetValue(this, message.NewValue);
}