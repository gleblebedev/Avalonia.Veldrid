//using System;
//using System.Collections.Concurrent;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using Avalonia.Input;
//using Avalonia.Input.Raw;
//using Avalonia.Platform;
//using Veldrid;
//using Veldrid.Sdl2;
//using Veldrid.StartupUtilities;
//using Veldrid.Utilities;
//using Key = Veldrid.Key;
//using MouseButton = Veldrid.MouseButton;
//using PixelFormat = Veldrid.PixelFormat;

//namespace Avalonia.Veldrid.Sdl2
//{
//    internal class Sdl2AvaloniaWindow
//    {
//        private static Sdl2Window _window;
//        private static GraphicsDevice _gd;
//        private static DisposeCollectorResourceFactory _factory;
//        private static bool _windowResized = true;
//        private readonly CommandList _cl;

//        private readonly WindowsCollectionView _windows = new WindowsCollectionView();
//        private readonly AvaloniaVeldridContext _context;
//        private readonly VeldridScreenStub _veldridScreen;
//        private Point _lastKnownMousePosition;

//        private RawInputModifiers _rawInputModifiers = RawInputModifiers.None;
//        private VeldridTopLevelImpl _activeWindow;

//        public Sdl2AvaloniaWindow(VeldridSdl2PlatformOptions options, AvaloniaVeldridContext context = null)
//        {
//            var flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable | SDL_WindowFlags.Shown;
//            switch (options.WindowState)
//            {
//                case WindowState.Normal:
//                    break;
//                case WindowState.FullScreen:
//                    flags |= SDL_WindowFlags.FullScreenDesktop;
//                    break;
//                case WindowState.Maximized:
//                    flags |= SDL_WindowFlags.Maximized;
//                    break;
//                case WindowState.Minimized:
//                    flags |= SDL_WindowFlags.Minimized;
//                    break;
//                case WindowState.BorderlessFullScreen:
//                    flags |= SDL_WindowFlags.Borderless | SDL_WindowFlags.FullScreenDesktop;
//                    break;
//                case WindowState.Hidden:
//                    flags |= SDL_WindowFlags.Hidden;
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }

//            _window = new Sdl2Window(
//                "Avalonia.Veldrid.Sdl2",
//                100,
//                100,
//                1280,
//                720,
//                flags,
//                false);

//            var graphicsDeviceOptions = new GraphicsDeviceOptions(
//                false,
//                PixelFormat.R16_UNorm,
//                true,
//                ResourceBindingModel.Improved,
//                true,
//                true);
//#if DEBUG
//            graphicsDeviceOptions.Debug = true;
//#endif
//            if (options.GraphicsBackend.HasValue)
//                _gd = VeldridStartup.CreateGraphicsDevice(_window, graphicsDeviceOptions,
//                    options.GraphicsBackend.Value);
//            else
//                _gd = VeldridStartup.CreateGraphicsDevice(_window, graphicsDeviceOptions);
//            _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
//            _veldridScreen = new VeldridScreenStub
//                {Size = new FramebufferSize((uint) _window.Width, (uint) _window.Height)};
//            _context = context ?? new AvaloniaVeldridContext();
//            _context.SetGraphicsDevice(_gd);
//            _context.Screen = _veldridScreen;
//            _cl = _factory.CreateCommandList();

//            _window.Resized += () =>
//            {
//                _veldridScreen.Size = new FramebufferSize((uint) _window.Width, (uint) _window.Height);
//                _windowResized = true;
//            };

//            MouseDevice = new MouseDevice();

//            _window.MouseMove += OnMouseMove;
//            //_window.MouseEntered += OnMouseEntered;
//            _window.MouseLeft += OnMouseLeft;
//            _window.MouseDown += OnMouseDown;
//            _window.MouseUp += OnMouseUp;
//            _window.KeyUp += OnKeyUp;
//            _window.KeyDown += OnKeyDown;
//        }

//        public MouseDevice MouseDevice { get; set; }

//        public void Run(CancellationToken cancellationToken)
//        {
//            var sw = Stopwatch.StartNew();
//            var previousElapsed = sw.Elapsed.TotalSeconds;

//            while (_window.Exists && !cancellationToken.IsCancellationRequested)
//            {
//                var newElapsed = sw.Elapsed.TotalSeconds;
//                var deltaSeconds = (float) (newElapsed - previousElapsed);

//                var inputSnapshot = _window.PumpEvents();
//                //InputTracker.UpdateFrameInput(inputSnapshot);

//                if (_window.Exists)
//                {
//                    previousElapsed = newElapsed;
//                    if (_windowResized)
//                    {
//                        _windowResized = false;
//                        _gd.ResizeMainWindow((uint) _window.Width, (uint) _window.Height);
//                        //Resized?.Invoke(new Size(_window.FramebufferWidth, _window.FramebufferHeight));
//                    }
//                    _context.PurgeMainThreadQueue();
//                    //Paint?.Invoke(new Rect(0, 0, _window.FramebufferWidth, _window.FramebufferHeight));
//                    _cl.Begin();
//                    _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
//                    _cl.SetFullViewport(0);
//                    _cl.ClearDepthStencil(1.0f);
//                    _windows.Fetch(_context);
//                    var hasHud = _windows.Any(_ => _.IsFullscreen);
//                    if (!hasHud) _cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
//                    foreach (var window in _windows) window.Render(_cl);
//                    _cl.End();
//                    _gd.SubmitCommands(_cl);

//                    _gd.SwapBuffers(_gd.MainSwapchain);
//                }
//            }

//            _gd.WaitForIdle();
//            _factory.DisposeCollector.DisposeAll();
//            _gd.Dispose();
//        }

//        public void EnsureInvokeOnMainThread(Action callback)
//        {
//            _context.EnsureInvokeOnMainThread(callback);
//        }

//        public IWindowImpl CreateWindow()
//        {
//            var textureWindowImpl = new VeldridWindowImpl(_context);

//            return textureWindowImpl;
//        }

//        public IEmbeddableWindowImpl CreateEmbeddableWindow()
//        {
//            var veldridEmbeddableWindowImpl = new VeldridEmbeddableWindowImpl(_context);

//            return veldridEmbeddableWindowImpl;
//        }

//        private void OnKeyDown(KeyEvent args)
//        {
//            switch (args.Key)
//            {
//                case Key.AltLeft:
//                case Key.AltRight:
//                    _rawInputModifiers |= RawInputModifiers.Alt;
//                    break;
//                case Key.ControlLeft:
//                case Key.ControlRight:
//                    _rawInputModifiers |= RawInputModifiers.Control;
//                    break;
//                case Key.ShiftLeft:
//                case Key.ShiftRight:
//                    _rawInputModifiers |= RawInputModifiers.Shift;
//                    break;
//            }
//        }

//        private void OnKeyUp(KeyEvent args)
//        {
//            switch (args.Key)
//            {
//                case Key.AltLeft:
//                case Key.AltRight:
//                    _rawInputModifiers &= ~RawInputModifiers.Alt;
//                    break;
//                case Key.ControlLeft:
//                case Key.ControlRight:
//                    _rawInputModifiers &= ~RawInputModifiers.Control;
//                    break;
//                case Key.ShiftLeft:
//                case Key.ShiftRight:
//                    _rawInputModifiers &= ~RawInputModifiers.Shift;
//                    break;
//            }
//        }

//        private void OnMouseUp(MouseEvent args)
//        {
//            switch (args.MouseButton)
//            {
//                case MouseButton.Left:
//                    _rawInputModifiers &= ~RawInputModifiers.LeftMouseButton;
//                    _activeWindow?.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _activeWindow.InputRoot,
//                        RawPointerEventType.LeftButtonUp, _lastKnownMousePosition, _rawInputModifiers));
//                    break;
//                case MouseButton.Middle:
//                    _rawInputModifiers &= ~RawInputModifiers.MiddleMouseButton;
//                    _activeWindow?.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _activeWindow.InputRoot,
//                        RawPointerEventType.MiddleButtonUp, _lastKnownMousePosition, _rawInputModifiers));
//                    break;
//                case MouseButton.Right:
//                    _rawInputModifiers &= ~RawInputModifiers.RightMouseButton;
//                    _activeWindow?.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _activeWindow.InputRoot,
//                        RawPointerEventType.RightButtonUp, _lastKnownMousePosition, _rawInputModifiers));
//                    break;
//            }
//        }

//        private void OnMouseDown(MouseEvent args)
//        {
//            switch (args.MouseButton)
//            {
//                case MouseButton.Left:
//                    _rawInputModifiers |= RawInputModifiers.LeftMouseButton;
//                    _activeWindow?.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _activeWindow.InputRoot,
//                        RawPointerEventType.LeftButtonDown, _lastKnownMousePosition, _rawInputModifiers));
//                    break;
//                case MouseButton.Middle:
//                    _rawInputModifiers |= RawInputModifiers.MiddleMouseButton;
//                    _activeWindow?.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _activeWindow.InputRoot,
//                        RawPointerEventType.MiddleButtonDown, _lastKnownMousePosition, _rawInputModifiers));
//                    break;
//                case MouseButton.Right:
//                    _rawInputModifiers |= RawInputModifiers.RightMouseButton;
//                    _activeWindow?.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _activeWindow.InputRoot,
//                        RawPointerEventType.RightButtonDown, _lastKnownMousePosition, _rawInputModifiers));
//                    break;
//            }
//        }


//        private void OnMouseLeft()
//        {
//            _rawInputModifiers &= RawInputModifiers.KeyboardMask;
//            _activeWindow?.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _activeWindow.InputRoot,
//                RawPointerEventType.LeaveWindow, _lastKnownMousePosition, _rawInputModifiers));
//        }

//        private void OnMouseMove(MouseMoveEventArgs args)
//        {
//            UpdateMousePosition(new Point(args.MousePosition.X, args.MousePosition.Y));
//            _activeWindow?.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _activeWindow.InputRoot,
//                RawPointerEventType.Move, _lastKnownMousePosition, _rawInputModifiers));
//        }

//        private void UpdateMousePosition(Point point)
//        {
//            //_lastKnownMousePosition = point;
//            var clipSpaceRay =
//                new ClipSpaceRay(point, new Size(_window.Width, _window.Height), _gd.IsDepthRangeZeroToOne, false);
//            VeldridTopLevelImpl activeWindow = null;
//            var mousePos = default(Point);
//            for (var index = 0; index < _windows.Count && clipSpaceRay.From.Z < clipSpaceRay.To.Z; index++)
//            {
//                var window = _windows[index];
//                var res = window.Raycast(clipSpaceRay);
//                if (res.HasValue)
//                {
//                    activeWindow = window;
//                    mousePos = res.Value.WindowPoint;
//                    clipSpaceRay.To.Z = res.Value.ClipSpaceDepth;
//                }
//            }

//            if (activeWindow != null)
//            {
//                _activeWindow = activeWindow;
//                _lastKnownMousePosition = mousePos;
//            }
//        }
//    }
//}