using System.Windows;
using System.Windows.Threading;
using Sheas_Cealer.Wins;

namespace Sheas_Cealer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) => new MainWin(e.Args).Show();

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Error: " + e.Exception.Message);
            e.Handled = true;
        }
    }
}