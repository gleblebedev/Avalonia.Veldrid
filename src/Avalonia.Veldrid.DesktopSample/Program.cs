using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Dialogs;
using CommandLine;
using Veldrid;

namespace Avalonia.Veldrid.DesktopSample
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var options = Parser.Default.ParseArguments<Options>(args) as Parsed<Options>;

            var viewerOptions = options?.Value ?? new Options();

            if (viewerOptions.WaitForDebugger)
            {
                Console.WriteLine("Attach debugger and use 'Set next statement'");
                while (true)
                {
                    Thread.Sleep(100);
                    if (Debugger.IsAttached)
                        break;
                }
            }

            var builder = AppBuilder.Configure<Avalonia.Veldrid.Sample.App>()
                .UseVeldridSdl2Window()
                .With(new VeldridSdl2PlatformOptions())
                .UseSkia()
                //.UseReactiveUI()
                .UseManagedSystemDialogs();

            //return builder.StartWithClassicDesktopLifetime(args);
            return builder.StartWithClassicDesktopLifetime(args);
        }
    }

    public class Options
    {
        [Option('g', "graphics")] public GraphicsBackend? GraphicsBackend { get; set; }

        [Option('w', "windowstate")] public WindowState WindowState { get; set; } = WindowState.Normal;

        [Option('r', "renderdoc")] public bool RenderDoc { get; set; }
        [Option('d', "wait-for-debugger")] public bool WaitForDebugger { get; set; }
    }
}