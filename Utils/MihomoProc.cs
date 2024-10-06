using System.IO;
using Sheas_Cealer.Consts;
using SheasCore;

namespace Sheas_Cealer.Utils;

internal class MihomoProc : Proc
{
    internal MihomoProc() : base(Path.GetFileName(MainConst.MihomoPath)) { }
}