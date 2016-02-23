using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using CodeCop.Core;
using CodeCop.Core.Extensions;
using CodeCop.Core.Fluent;

namespace dnTrace.Bootstrapper
{
    public class Bootstrap
    {
        readonly BinaryFormatter binaryFormatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.CrossProcess))
        {
            Binder = new Binder()
        };

        static Bootstrap()
        {
            var home = Environment.GetEnvironmentVariable("DNTRACE_HOME", EnvironmentVariableTarget.User);
            if (string.IsNullOrWhiteSpace(home)) throw new InvalidOperationException("DNTRACE_HOME environment variable isn't set");

            var codeCopPath = Path.Combine(home, "CodeCop.dll");

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if (args.Name.Contains("CodeCop"))
                    return Assembly.LoadFile(codeCopPath);

                return args.RequestingAssembly;
            };
        }

        public Bootstrap()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    var p = Process.GetCurrentProcess();
                    var pipe = new NamedPipeClientStream(".", $"{p.ProcessName}.{p.Id}", PipeAccessRights.ReadWrite,
                        PipeOptions.Asynchronous, TokenImpersonationLevel.None, HandleInheritability.None);

                    Cop.AsFluent();

                    pipe.Connect();

                    Task.Run(() => Report(pipe, new ExecutionResult { Message = "Connected!", IsMessage = true }));

                    while (true)
                    {
                        var ctx = (InjectContext)binaryFormatter.Deserialize(pipe);

                        InterceptMethod(ctx, pipe);
                    }
                }
                catch (Exception e)
                {
                }
            });

        }

        void InterceptMethod(InjectContext ctx, Stream outputStream)
        {
            var type = Type.GetType(ctx.TypeFQN);
            if (type == null) return;
            var parameters = ctx.ParametersFQN.Select(Type.GetType).ToArray();
            var bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var methodInfo = type.GetMethod(ctx.MethodName, bindingFlags, null, parameters, new ParameterModifier[0]);

            methodInfo?.Override(context =>
            {
                var executionContext = new ExecutionResult();
                var mi = context.InterceptedMethod as MethodInfo;
                if (mi != null)
                {
                    executionContext.Entry = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                    executionContext.MethodName = mi.DeclaringType.FullName + "." + mi.Name;
                    executionContext.Parameters = context.Parameters.Select(p => new KeyValuePair<string, string>(p.Name, p.Value.ToString())).ToList();
                }
                Report(outputStream, executionContext);

                return context.InterceptedMethod.Execute(context.Sender, context.Parameters.Select(parameter => parameter.Value).ToArray());
            });

            Cop.Intercept();
        }

        void Report(Stream outputStream, ExecutionResult executionResult)
        {
            try
            {
                binaryFormatter.Serialize(outputStream, executionResult);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}
