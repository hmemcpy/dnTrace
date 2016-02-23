using System;
using System.Collections.Generic;
using System.IO;

namespace dnTrace.Bootstrapper
{
    [Serializable]
    public class InjectContext
    {
        public InjectContext()
        {
            HomeDirectory = Path.GetDirectoryName(Injector.InjectorPath);
        }

        public string HomeDirectory { get; set; }
        public string TypeFQN { get; set; }
        public string MethodName { get; set; }
        public List<string> ParametersFQN { get; set; }
        public string Visibility { get; set; }
    }
}