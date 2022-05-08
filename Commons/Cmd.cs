using System;
using System.Diagnostics;
using SheasCore;

namespace Sheas_Cealer
{
    internal class Cmd : Proc
    {
        internal Cmd() : base("Cmd.exe")
        {
        }

        public override void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
        }
        public override void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
        }
        public override void Process_Exited(object sender, EventArgs e) => Environment.Exit(0);
    }
}