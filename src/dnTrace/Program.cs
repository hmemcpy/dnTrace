using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Colorful;
using CommandLine;
using dnTrace.Bootstrapper;
using dnTrace.ProcessData;
using dnTrace.Utils;
using Console = Colorful.Console;

namespace dnTrace
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("dnTrace.Resources.block.flf"))
            {
                var styleSheet = new StyleSheet(Color.LightGoldenrodYellow);
                styleSheet.AddStyle("dn", Color.Yellow);
                styleSheet.AddStyle("Trace", Color.PaleGreen);
                var figletFont = FigletFont.Load(fontStream);
                
                Console.WriteAsciiStyled("dnTrace", figletFont, styleSheet);
            }

            Console.WriteLine("dnTrace v1.0 by Igal Tabachnik");
            Console.WriteLine("==============================");
            Console.WriteLine();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => Run(options));
        }

        private static void Run(Options options)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress +=
                (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

            // Since Console apps do not have a SynchronizationContext, we're leveraging the built-in support
            // in WPF to pump the messages via the Dispatcher.
            // See the following for additional details:
            //   http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
            //   http://blogs.msdn.com/b/pfxteam/archive/2012/01/21/10259307.aspx
            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                var context = new DispatcherSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);

                var dispatcherFrame = new DispatcherFrame();
                Task mainTask = MainAsync(options, cts.Token);
                mainTask.ContinueWith(task => dispatcherFrame.Continue = false, cts.Token);

                Dispatcher.PushFrame(dispatcherFrame);
                mainTask.GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        private static async Task MainAsync(Options options, CancellationToken cancellationToken)
        {
            Process targetProcess;
            if (!TryGetProcess(options.Process, out targetProcess))
            {
                Console.WriteLine("Unable to find process...");
                return;
            }

            var session = new TraceSession(targetProcess);
            session.Messages.CollectionChanged += (sender, e) =>
            {
                foreach (ExecutionResult ctx in e.NewItems)
                {
                    if (ctx.IsMessage)
                        Colorful.Console.WriteLine(ctx.Message, Color.Yellow);
                    else
                    {
                        PrintValues(ctx);
                    }
                }
            };

            var creator = new InjectionContextCreator(targetProcess);
            var contexts = creator.CreateInjectionContext(options.Type, options.Method).ToArray();

            Console.WriteLine($"Intercepting '{targetProcess.ProcessName}', PID: {targetProcess.Id}...");
            Console.WriteLine();
            Console.WriteLine("Listening to the following method(s):");
            foreach (var context in contexts)
            {
                Console.WriteLine(context.ToString());
            }

            await session.Run(cancellationToken, contexts);
        }

        private static bool TryGetProcess(string process, out Process targetProcess)
        {
            var availableProcesses = ProcessUtil.GetAllProcesses().Where(info => info.IsManaged && !info.IsAccessDenied).ToList();

            int pid;
            if (int.TryParse(process, out pid))
            {
                var p = availableProcesses.FirstOrDefault(info => info.ProcessId == pid);
                if (p != null)
                {
                    targetProcess = Process.GetProcessById(pid);
                    return true;
                }
            }

            var namedProcesses = availableProcesses.Where(info => info.Name.StartsWith(process, StringComparison.OrdinalIgnoreCase)).ToList();
            if (namedProcesses.Any())
            {
                var selectedProcessId = namedProcesses.Count > 1 ? PromptSelectProcess(process, namedProcesses) : namedProcesses[0].ProcessId;
                targetProcess = Process.GetProcessById(selectedProcessId);
                return true;
            }

            targetProcess = null;
            return false;
        }

        private static int PromptSelectProcess(string processName, List<ProcessInfo> namedProcesses)
        {
            Console.Write("Multiple processes with the name '");
            Colorful.Console.Write($"{processName}", Color.White);
            Console.WriteLine("' were found. Please select the desired process:");
            Console.WriteLine();

            for (int i = 0; i < namedProcesses.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {namedProcesses[i].Name}, PID: {namedProcesses[i].ProcessId}");
            }

            int result;
            string input;
            do
            {
                Console.WriteLine($"Enter process index (e.g. {string.Join(", ", Enumerable.Range(1, namedProcesses.Count))}): ");
                input = Console.ReadLine();
            } while (!int.TryParse(input, out result) && result >= 1 && result <= namedProcesses.Count);

            return namedProcesses[result].ProcessId;
        }

        private static void PrintValues(ExecutionResult result)
        {
            var parts = result.MethodName.Split('.');
            var name = parts.Last();
            var ns = parts.Take(parts.Length - 1);
            Console.Write(string.Join(".", ns));
            Console.Write('.');
            Colorful.Console.Write(name, Color.FromArgb(0xc33400));
            Console.Write('(');
            if (result.Parameters.Any())
            {
                foreach (var param in result.Parameters)
                {
                    Colorful.Console.Write("{ ", Color.White);
                    Colorful.Console.Write(param.Value, Color.AliceBlue);
                    Colorful.Console.Write(" }");
                }
            }
            Console.Write(')');
            Console.Write(" = ");
            if (result.Result != null)
            {
                Colorful.Console.Write(result.Result, Color.White);
            }
            else
            {
                Colorful.Console.Write("void", Color.White);
            }

            Console.WriteLine();
            
        }
    }
}
