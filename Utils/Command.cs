using System;
using System.Diagnostics;
using SheasCore;

namespace Sheas_Cealer.Utils;

internal class Command : Proc
{
    internal bool EnvironmentExitAfterProcessExited = true;

    internal Command(bool environmentExitAfterProcessExited) : base("Cmd.exe")
    {
        EnvironmentExitAfterProcessExited = environmentExitAfterProcessExited;
    }

    public override void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
    }
    public override void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
    }
    public override void Process_Exited(object sender, EventArgs e)
    {
        if (EnvironmentExitAfterProcessExited)
            Environment.Exit(0);
    }
}