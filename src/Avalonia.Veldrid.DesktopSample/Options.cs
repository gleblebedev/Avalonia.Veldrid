using CommandLine;
using Veldrid;

namespace Avalonia.Veldrid.DesktopSample
{
    public class Options
    {
        [Option('g', "graphics")] public GraphicsBackend? GraphicsBackend { get; set; }

        [Option('w', "windowstate")] public WindowState WindowState { get; set; } = WindowState.Normal;

        [Option('r', "renderdoc")] public bool RenderDoc { get; set; }
        [Option('d', "wait-for-debugger")] public bool WaitForDebugger { get; set; }
    }
}