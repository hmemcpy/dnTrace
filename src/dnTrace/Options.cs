using System;
using CommandLine;

namespace dnTrace
{
    internal class Options
    {
        [Option('p', "pid", Required = true, HelpText = "Process name or PID")]
        public string Process { get; set; }

        [Option('t', "type", Required = true, HelpText = "Fully qualified type name (e.g. System.Diagnostic.Process)")]
        public string Type { get; set; }

        [Option('m', "method", Required = true, HelpText = "Method name (e.g. Start). In case of multiple overloads, a selection will be presented")]
        public string Method { get; set; }
    }
}