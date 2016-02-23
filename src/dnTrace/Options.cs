using System;
using CommandLine;

namespace dnTrace
{
    internal class Options
    {
        [Option('p', "pid", Required = true, HelpText = "The process id to inspect")]
        public int Pid { get; set; }
    }
}