using Sheas_Cealer.Consts;
using Sheas_Cealer.Utils;
using Sheas_Core;
using System;

namespace Sheas_Cealer.Proces;

internal class NginxProc : Proc
{
    internal NginxProc() : base(MainConst.NginxPath) { }

    protected override async void Process_Exited(object sender, EventArgs e) => await NginxCleaner.Clean();
}