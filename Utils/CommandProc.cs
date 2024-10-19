using System;
using System.Windows;
using SheasCore;

namespace Sheas_Cealer.Utils;

internal class CommandProc : Proc
{
    private static bool ShutDownAppOnProcessExit;

    internal CommandProc(bool shutDownAppOnProcessExit) : base("Cmd.exe") => ShutDownAppOnProcessExit = shutDownAppOnProcessExit;

    public override void Process_Exited(object sender, EventArgs e)
    {
        if (ShutDownAppOnProcessExit)
            Application.Current.Dispatcher.InvokeShutdown();
    }
}