using System.Windows;

namespace Sheas_Cealer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            new MainWindow(e.Args).Show();
        }
    }
}