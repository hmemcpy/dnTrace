using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using CodeCop.Core;

namespace dnTrace.Bootstrapper
{
    public static class Injector
    {
        public static readonly string InjectorPath = new Uri(typeof(Bootstrap).Assembly.CodeBase).LocalPath;

        public static void InjectInto(Process process)
        {
            Cop.Inject(process, InjectorPath, typeof(Bootstrap).FullName);
        }
    }

    public class TraceSession
    {
        private readonly Process process;
        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.CrossProcess));

        private readonly NamedPipeServerStream server;

        public ObservableCollection<ExecutionResult> Messages { get; } = new ObservableCollection<ExecutionResult>();

        public TraceSession(Process process)
        {
            this.process = process;
            server = new NamedPipeServerStream($"{process.ProcessName}.{process.Id}", PipeDirection.InOut);
            
            process.Exited += (sender, args) =>
            {
                Messages.Add(new ExecutionResult { Message = $"Process terminated. Disconnecting...", IsMessage = true });
                server.Disconnect();
            };
        }

        public async Task Run(InjectContext injectContext, CancellationToken cancellationToken)
        {
            Injector.InjectInto(process);

            await server.WaitForConnectionAsync(cancellationToken);

            try
            {
                binaryFormatter.Serialize(server, injectContext);
            }
            catch (Exception)
            {
                Messages.Add(new ExecutionResult { Message = "An error occurred!", IsMessage = true });
                return;
            }

            var resultsTask = Task.Factory.StartNew(
                function: GetExecutionData,
                cancellationToken: cancellationToken,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.FromCurrentSynchronizationContext()
                );

            await resultsTask.Unwrap();
        }

        private Task GetExecutionData()
        {
            while (server.IsConnected)
            {
                try
                {
                    var context = (ExecutionResult)binaryFormatter.Deserialize(server);
                    Messages.Add(context);
                }
                catch (SerializationException)
                {
                }
            }

            return Task.CompletedTask;
        }
    }
}