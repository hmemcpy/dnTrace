using System;
using System.Collections.Generic;
using CodeCop.Core;
using Jil;

namespace dnTrace.Bootstrapper
{
    [Serializable]
    public class ExecutionResult
    {
        public string Message { get; set;  }
        public bool IsMessage { get; set; } // todo ugh
        public string MethodName { get; set; }
        public List<ParameterData> Parameters { get; set; }
        public object Result { get; set; }
        public string Entry { get; set; }
    }

    [Serializable]
    public class ParameterData
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ValueJson { get; set; }
    }
}