using System;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Utils;
using SheasCore;

namespace Sheas_Cealer.Proces;

internal class NginxProc : Proc
{
    internal NginxProc() : base(MainConst.NginxPath) { }

    public override void Process_Exited(object sender, EventArgs e) => GlobalCealCleaner.Clean();
}