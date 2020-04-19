using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDRNetwork.Utils
{
    internal static class Util
    {
        internal static long TickCount => DateTime.Now.Ticks / 10000;
    }
}
