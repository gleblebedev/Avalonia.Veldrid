using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Dialogs;
using Avalonia.Veldrid.Sample;
using Avalonia.Veldrid.Sdl2;
using CommandLine;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Avalonia.Veldrid.DesktopSample
{
    internal class Program
    {
        private static GraphicsDevice _graphicsDevice;
        private static AvaloniaVeldridContext _veldridContext;
        private static CommandList _commandList;
        private static WindowsCollectionView _windows;
        private static VeldridScreenStub _veldridScreen;
        private static Sdl2Window _window;
        private static Sdl2KeyboardAdapter _keyboard;
        private static Sdl2MouseAdapter _pointer;
        private static bool _windowResized;

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

            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowTitle = "Veldrid Tutorial",
                WindowInitialState = options.Value.WindowState
            };
            _window = VeldridStartup.CreateWindow(ref windowCI);
            _window.Resized += () => { _windowResized = true; };

            var graphicsDeviceOptions = new GraphicsDeviceOptions() { PreferStandardClipSpaceYDirection = true };
            if (options.Value.GraphicsBackend.HasValue)
            {
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, graphicsDeviceOptions, options.Value.GraphicsBackend.Value);
            }
            else
            {
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, graphicsDeviceOptions);
            }
            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

            _veldridScreen = new VeldridScreenStub() { Size = new FramebufferSize(_window.Width, _window.Height) };
            _veldridContext = new AvaloniaVeldridContext(_graphicsDevice, screenImpl: _veldridScreen);
            _windows = new WindowsCollectionView();
            _keyboard = new Sdl2KeyboardAdapter(_window, _veldridContext.KeyboardAdapter);
            _pointer = new Sdl2MouseAdapter(_window, _veldridContext.PointerAdapter);

            PortableAppBuilder.Configure<App>()
                .UsePortablePlatfrom()
                .With(_veldridContext)
                .UseSkia()
                .UseManagedSystemDialogs()
                .SetupWithoutStarting();

            var mainWindow = new MainWindow();
            mainWindow.Show();

            while (_window.Exists)
            {
                _window.PumpEvents();
                if (_window.Exists)
                {
                    _veldridContext.PurgeMainThreadQueue();
                    if (_windowResized)
                    {
                        _windowResized = false;
                        _graphicsDevice.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
                    }

                    Draw();
                }
            }

            _keyboard.Dispose();
            _pointer.Dispose();
            _commandList.Dispose();
            _graphicsDevice.Dispose();

            return 0;
        }


        private static void Draw()
        {
            _veldridScreen.Size = new FramebufferSize(_window.Width, _window.Height);
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.MainSwapchain.Framebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            _windows.Fetch(_veldridContext);
            foreach (var window in _windows)
            {
                window.Render(_commandList);
            }
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers(_graphicsDevice.MainSwapchain);
        }
    }
}