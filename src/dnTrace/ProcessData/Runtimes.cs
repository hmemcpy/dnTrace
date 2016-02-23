using System;
using System.ComponentModel;

namespace dnTrace.ProcessData
{
    [Flags]
    internal enum Runtimes
    {
        Unknown = 0,
        [Description("v2.0")]
        Clr2 = 2,
        [Description("v4.0")]
        Clr4 = 4,
    }
}