using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommandLine;
using dnTrace.Bootstrapper;

namespace dnTrace
{
    class Program
    {
        static void Main(string[] args)
        {
            //var options = new Options();
            //if (!Parser.Default.ParseArguments(args, options))
            //{
            //    Console.WriteLine("Missing arguments");
            //    return;
            //}

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
                Task mainTask = MainAsync(null/*options*/, cts.Token);
                mainTask.ContinueWith(task => dispatcherFrame.Continue = false, cts.Token);

                Dispatcher.PushFrame(dispatcherFrame);
                mainTask.GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static async Task MainAsync(Options options, CancellationToken cancellationToken)
        {
            var path = Path.GetFullPath("../../../Test/TestApplicationVisualBasicGUI/bin/Debug/TestApplicationVisualBasicGUI.exe");
            var psi = new ProcessStartInfo
            {
                FileName = path,
                CreateNoWindow = false,
            };
            var p = Process.Start(psi);
            Console.WriteLine($"Launching {psi.FileName}...");
            
            var session = new TraceSession(p);
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

            await session.Run(new InjectContext
            {
                MethodName = "OnClick",
                TypeFQN = "System.Windows.Forms.Button, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                ParametersFQN = new List<string>
                {
                    "System.EventArgs, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                }
            }, cancellationToken);
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
                Colorful.Console.Write("<none>", Color.White);
            }

            Console.WriteLine();
            
        }
    }
}
