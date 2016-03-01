using System;
using System.Collections.Generic;
using System.Linq;

namespace dnTrace.Bootstrapper
{
    [Serializable]
    public class InjectContext
    {
        public string TypeFQN { get; set; }
        public string MethodName { get; set; }
        public List<string> ParametersFQN { get; set; }

        public override string ToString()
        {
            return $"{TypeFQN.Split(',', ' ')[0]}.{MethodName}({string.Join(", ", ParametersFQN.Select(s => s.Split(',', ' ')[0]))})";
        }
    }
}