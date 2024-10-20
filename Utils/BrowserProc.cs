using System;
using System.Windows;
using SheasCore;

namespace Sheas_Cealer.Utils;

internal class BrowserProc : Proc
{
    private static bool ShutDownAppOnProcessExit;

    internal BrowserProc(string browserPath, bool shutDownAppOnProcessExit) : base(browserPath) => ShutDownAppOnProcessExit = shutDownAppOnProcessExit;

    public override void Process_Exited(object sender, EventArgs e)
    {
        if (ShutDownAppOnProcessExit)
            Application.Current.Dispatcher.InvokeShutdown();
    }
}