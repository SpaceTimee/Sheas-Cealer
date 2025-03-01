using Sheas_Cealer_Nix.Consts;
using Sheas_Cealer_Nix.Utils;
using Sheas_Core;
using System;

namespace Sheas_Cealer_Nix.Proces;

internal class NginxProc : Proc
{
    internal NginxProc() : base(MainConst.NginxPath) { }

    protected override async void Process_Exited(object? sender, EventArgs e) => await NginxCleaner.Clean();
}