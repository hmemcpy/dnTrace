using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dnlib.DotNet;
using dnlib.PE;
using dnTrace.Bootstrapper;

namespace dnTrace.ProcessData
{
    public class InjectionContextCreator
    {
        private readonly Process process;

        public InjectionContextCreator(Process process)
        {
            this.process = process;
        }

        public IEnumerable<InjectContext> CreateInjectionContext(string typeFullName, string methodName)
        {
            var moduleDef = LoadFile(process.MainModule.FileName);
            if (moduleDef == null) return null;
            
            var finder = new MemberFinder().FindAll(moduleDef);

            var match = Match(finder.MethodDefs.Keys, typeFullName, methodName).Concat(
                        Match(finder.MemberRefs.Keys, typeFullName, methodName)).ToArray();

            return match.Select(entry => new InjectContext
            {
                MethodName = entry.Name,
                TypeFQN = entry.DeclaringType.AssemblyQualifiedName,
                ParametersFQN = GetParameters(entry)
            }).ToArray();
        }

        private static IEnumerable<IMethodDefOrRef> Match(IEnumerable<IMethodDefOrRef> members, string typeFullName, string methodName)
        {
            return members.Where(m => m.DeclaringType.ReflectionFullName == typeFullName && m.Name == methodName);
        }

        private static List<string> GetParameters(IMethodDefOrRef method)
        {
            return method.MethodSig.Params.Select(sig => sig.AssemblyQualifiedName).ToList();
        }

        private ModuleDef LoadFile(string fileName)
        {
            var peImage = new PEImage(fileName);
            var dotNetDir = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[14];
            bool isDotNet = dotNetDir.VirtualAddress != 0 && dotNetDir.Size >= 0x48;
            if (isDotNet)
            {
                var assemblyResolver = new AssemblyResolver { EnableTypeDefCache = true };
                var options = new ModuleCreationOptions(CreateModuleContext(assemblyResolver));
                var module = ModuleDefMD.Load(peImage, options);
                module.EnableTypeDefFindCache = true;
                assemblyResolver.AddToCache(module);

                return module;
            }

            return null;
        }

        private static ModuleContext CreateModuleContext(IAssemblyResolver assemblyResolver)
        {
            var moduleCtx = new ModuleContext { AssemblyResolver = assemblyResolver };
            moduleCtx.Resolver = new Resolver(moduleCtx.AssemblyResolver) { ProjectWinMDRefs = false };
            return moduleCtx;
        }
    }
}