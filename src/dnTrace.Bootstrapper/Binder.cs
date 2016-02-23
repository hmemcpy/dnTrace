using System;
using System.Runtime.Serialization;

namespace dnTrace.Bootstrapper
{
    internal sealed class Binder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return typeName.Contains("InjectContext") ? typeof(InjectContext) : Type.GetType($"{typeName}, {assemblyName}");
        }
    }
}