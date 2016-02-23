using System;
using System.Collections.Generic;

namespace dnTrace.Bootstrapper
{
    [Serializable]
    public class ExecutionResult
    {
        public string Message { get; set;  }
        public bool IsMessage { get; set; } // todo ugh
        public string MethodName { get; set; }
        public List<KeyValuePair<string, string>> Parameters { get; set; }
        public object Result { get; set; }
        public string Entry { get; set; }
    }
}