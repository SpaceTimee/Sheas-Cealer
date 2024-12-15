using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ona_Core;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Utils;

namespace Sheas_Cealer.Wins;

public partial class AboutWin : Window
{
    private readonly AboutPres AboutPres;
    private readonly HttpClient AboutClient = new(new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });

    internal AboutWin()
    {
        InitializeComponent();

        DataContext = AboutPres = new();
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        IconRemover.RemoveIcon(this);
        BorderThemeSetter.SetBorderTheme(this, AboutPres.IsLightTheme);
    }
    private async void AboutWin_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(async () =>
        {
            try
            {
                AboutClient.DefaultRequestHeaders.Add("User-Agent", AboutConst.ReleaseApiUserAgent);

                JsonElement releaseInfoObject = JsonDocument.Parse(await Http.GetAsync<string>(AboutConst.ReleaseApiUrl, AboutClient)).RootElement;

                AboutClient.DefaultRequestHeaders.Clear();

                foreach (JsonProperty releaseInfoContent in releaseInfoObject.EnumerateObject())
                    if (releaseInfoContent.Name == "name" && releaseInfoContent.Value.ToString() != AboutConst.VersionButtonVersionContent)
                        AboutPres.IsSheasCealerUtd = false;
            }
            catch { }
        });
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        Button senderButton = (Button)sender;

        if (senderButton == VersionButton)
            MessageBox.Show($"{AboutConst._ReleasePagePasswordLabel} 3wnj");

        ProcessStartInfo processStartInfo = new(senderButton == EmailButton ? "mailto:" : string.Empty + senderButton.ToolTip) { UseShellExecute = true };

        try { Process.Start(processStartInfo); }
        catch (UnauthorizedAccessException)
        {
            processStartInfo.Verb = "RunAs";
            Process.Start(processStartInfo);
        }
    }

    private void AboutWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }
}