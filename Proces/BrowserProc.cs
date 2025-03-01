using Sheas_Core;
using System;

namespace Sheas_Cealer_Nix.Proces;

internal class BrowserProc : Proc
{
    private readonly bool ShutDownAppOnProcessExit;

    internal BrowserProc(string browserPath, bool shutDownAppOnProcessExit) : base(browserPath)
    {
        ShutDownAppOnProcessExit = shutDownAppOnProcessExit;

        Process_Exited(null, EventArgs.Empty);
    }

    protected sealed override void Process_Exited(object? sender, EventArgs e)
    {
        if (ShutDownAppOnProcessExit)
            Environment.Exit(0);
    }
}