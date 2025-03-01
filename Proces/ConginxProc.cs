using Sheas_Cealer_Nix.Consts;
using Sheas_Cealer_Nix.Utils;
using Sheas_Core;
using System;

namespace Sheas_Cealer_Nix.Proces;

internal class ConginxProc : Proc
{
    internal ConginxProc() : base(MainConst.ConginxPath) { }

    protected override async void Process_Exited(object? sender, EventArgs e) => await NginxCleaner.Clean();
}