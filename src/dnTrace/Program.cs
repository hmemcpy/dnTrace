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

            Console.WriteLine("dnTrace v1.0 by Igal Tabachnik", Color.White);
            Console.WriteLine("──────────────────────────────", Color.White);
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
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DNTRACE_HOME", EnvironmentVariableTarget.User)))
                Environment.SetEnvironmentVariable("DNTRACE_HOME", Path.GetDirectoryName(Injector.InjectorPath), EnvironmentVariableTarget.User);

            Process targetProcess;
            if (!TryGetProcess(options.Process, out targetProcess))
            {
                Console.WriteLine("Unable to find process...", Color.Red);
                return;
            }

            var session = new TraceSession(targetProcess);
            session.Messages.CollectionChanged += (sender, e) =>
            {
                foreach (ExecutionResult ctx in e.NewItems)
                {
                    if (ctx.IsMessage)
                        Console.WriteLine(ctx.Message, Color.Yellow);
                    else
                    {
                        PrintValues(ctx);
                    }
                }
            };

            var creator = new InjectionContextCreator(targetProcess);
            var contexts = creator.CreateInjectionContext(options.Type, options.Method).ToArray();

            Console.WriteLine($"Attaching to '{targetProcess.ProcessName}', PID: {targetProcess.Id}...");
            Console.WriteLine();

            if (!contexts.Any())
            {
                Console.WriteLine($"Unable to find any references to '{options.Type}.{options.Method}' in the target process or its assemblies.", Color.Yellow);
                Console.WriteLine("Make sure the specified full type name and method name is correct.", Color.Yellow);
                return;
            }

            if (contexts.Length > 1)
            {
                contexts = SelectMethodsToIntercept(contexts);
                Console.WriteLine();
            }

            Console.WriteLine("Tracing the following method(s):", Color.PaleGreen);
            foreach (var context in contexts)
            {
                Console.WriteLine(context.ToString(), Color.White);
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

        private static int PromptSelectProcess(string processName, IReadOnlyList<ProcessInfo> namedProcesses)
        {
            Console.Write("Multiple processes with the name '");
            Console.Write($"{processName}", Color.White);
            Console.WriteLine("' were found. Please select the desired process:");
            Console.WriteLine();

            for (int i = 0; i < namedProcesses.Count; i++)
            {
                Console.Write($"[{i + 1}] ", Color.White);
                Console.WriteLine($"{ namedProcesses[i].Name}, PID: {namedProcesses[i].ProcessId}");
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

        private static InjectContext[] SelectMethodsToIntercept(InjectContext[] contexts)
        {
            int numberOfOverloads = contexts.Length;
            Console.WriteLine($"Found {numberOfOverloads} overloads:", Color.Yellow);

            for (int i = 0; i < numberOfOverloads; i++)
            {
                Console.Write($"[{i + 1}] ", Color.White);
                Console.WriteLine($"{contexts[i]}");
            }

            int[] result;
            string input;
            do
            {
                Console.Write("Select overload(s) to trace (e.g. " + $"{GetSelectionString(numberOfOverloads)}), or press Enter to trace all: ", Color.White);
                input = Console.ReadLine();
            } while (!TryGetSelection(input, numberOfOverloads, out result));

            return result.Select(i => contexts[i - 1]).ToArray();
        }

        private static bool TryGetSelection(string input, int numberOfOverloads, out int[] result)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                result = Enumerable.Range(1, numberOfOverloads).ToArray();
                return true;
            }

            int index = 0;
            result = input.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                          .Where(s =>
                          {
                              if (int.TryParse(s, out index) && index > numberOfOverloads)
                              {
                                  Console.WriteLine($"Invalid entry index {s}, skipping...", Color.Orange);
                                  return false;
                              }
                              return true;
                          })
                          .Select(i => index)
                          .Distinct()
                          .OrderBy(i => i)
                          .ToArray();

            return true;
        }

        private static string GetSelectionString(int numberOfOverloads)
        {
            int half = numberOfOverloads / 2;
            if (half == 1)
                return "1,2";

            return string.Join(",", Enumerable.Range(1, half)) + "..,n";
        }

        private static void PrintValues(ExecutionResult result)
        {
            var parts = result.MethodName.Split('.');
            var name = parts.Last();
            Console.Write(name, Color.FromArgb(0x2B91AF));
            Console.Write('(');
            if (result.Parameters.Any())
            {
                foreach (var param in result.Parameters)
                {
                    Console.Write(param.ValueJson, Color.AliceBlue);
                }
            }
            Console.Write(')');
            Console.Write(" = ");
            if (result.Result != null)
            {
                Console.Write(result.Result, Color.White);
            }
            else
            {
                Console.Write("void", Color.White);
            }

            Console.WriteLine();
            
        }
    }
}
