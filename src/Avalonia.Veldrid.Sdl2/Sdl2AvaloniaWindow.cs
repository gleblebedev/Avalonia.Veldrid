using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Avalonia.Input;
using Avalonia.Platform;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;
using Key = Veldrid.Key;
using PixelFormat = Veldrid.PixelFormat;
using WindowState = Avalonia.Controls.WindowState;

namespace Avalonia.Veldrid.Sdl2
{
    internal class Sdl2AvaloniaWindow
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static DisposeCollectorResourceFactory _factory;
        private static bool _windowResized = true;
        private Size _minSize;
        private Size _maxSize;
        private readonly ConcurrentQueue<Action> _executionList = new ConcurrentQueue<Action>();
        private readonly CommandList _cl;
        private Point _lastKnownMousePosition;

        private RawInputModifiers _rawInputModifiers = RawInputModifiers.None;

        private readonly List<Sdl2TextureWindowImpl> _windows = new List<Sdl2TextureWindowImpl>();
        private Sdl2TextureWindowImpl _hudWindow;
        private readonly object _gate = new object();
        private readonly AvaloniaVeldridContext _context;

        public Sdl2AvaloniaWindow()
        {
            var flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable | SDL_WindowFlags.Shown;
            _window = new Sdl2Window(
                "Avalonia.Veldrid.Sdl2",
                100,
                100,
                1281,
                720,
                flags,
                false);

            _window.Resized += () => { _windowResized = true; };

            var options = new GraphicsDeviceOptions(
                false,
                PixelFormat.R16_UNorm,
                true,
                ResourceBindingModel.Improved,
                true,
                true);
#if DEBUG
            options.Debug = true;
#endif
            _gd = VeldridStartup.CreateGraphicsDevice(_window, options);
            _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
            _context = new AvaloniaVeldridContext(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription);
            _cl = _factory.CreateCommandList();

            MouseDevice = new MouseDevice();

            _window.MouseMove += OnMouseMove;
            //_window.MouseEntered += OnMouseEntered;
            _window.MouseLeft += OnMouseLeft;
            _window.MouseDown += OnMouseDown;
            _window.MouseUp += OnMouseUp;
            _window.KeyUp += OnKeyUp;
            _window.KeyDown += OnKeyDown;
        }

        public MouseDevice MouseDevice { get; set; }

        public void Run(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var previousElapsed = sw.Elapsed.TotalSeconds;

            while (_window.Exists && !cancellationToken.IsCancellationRequested)
            {
                var newElapsed = sw.Elapsed.TotalSeconds;
                var deltaSeconds = (float) (newElapsed - previousElapsed);

                var inputSnapshot = _window.PumpEvents();
                //InputTracker.UpdateFrameInput(inputSnapshot);

                if (_window.Exists)
                {
                    previousElapsed = newElapsed;
                    if (_windowResized)
                    {
                        _windowResized = false;
                        _gd.ResizeMainWindow((uint) _window.Width, (uint) _window.Height);
                        //Resized?.Invoke(new Size(_window.Width, _window.Height));
                    }

                    while (_executionList.TryDequeue(out var action)) action();
                    //Paint?.Invoke(new Rect(0, 0, _window.Width, _window.Height));
                    _cl.Begin();
                    _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                    _cl.SetFullViewport(0);
                    lock (_gate)
                    {
                        var hasHud = false;
                        foreach (var window in _windows)
                            if (window.WindowState == WindowState.Maximized)
                            {
                                window.Resize(new Size(_window.Width, _window.Height));
                                window.RenderFullscreen(_cl);
                                hasHud = true;
                                break;
                            }

                        if (!hasHud) _cl.ClearColorTarget(0, RgbaFloat.Black);
                        foreach (var window in _windows)
                            if (window.WindowState != WindowState.Maximized)
                                window.RenderSlate(_cl);
                    }

                    _cl.End();
                    _gd.SubmitCommands(_cl);

                    _gd.SwapBuffers(_gd.MainSwapchain);
                }
            }

            _gd.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            _gd.Dispose();
        }

        public void EnsureInvokeOnMainThread(Action callback)
        {
            _executionList.Enqueue(callback);
        }

        public IWindowImpl CreateWindow()
        {
            var textureWindowImpl =
                new Sdl2TextureWindowImpl(_context, _gd.MainSwapchain.Framebuffer.OutputDescription, this);
            if (textureWindowImpl.WindowState == WindowState.Maximized)
            {
            }

            lock (_gate)
            {
                _windows.Add(textureWindowImpl);
            }

            return textureWindowImpl;
        }

        private void OnKeyDown(KeyEvent args)
        {
            switch (args.Key)
            {
                case Key.AltLeft:
                case Key.AltRight:
                    _rawInputModifiers |= RawInputModifiers.Alt;
                    break;
                case Key.ControlLeft:
                case Key.ControlRight:
                    _rawInputModifiers |= RawInputModifiers.Control;
                    break;
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    _rawInputModifiers |= RawInputModifiers.Shift;
                    break;
            }
        }

        private void OnKeyUp(KeyEvent args)
        {
            switch (args.Key)
            {
                case Key.AltLeft:
                case Key.AltRight:
                    _rawInputModifiers &= ~RawInputModifiers.Alt;
                    break;
                case Key.ControlLeft:
                case Key.ControlRight:
                    _rawInputModifiers &= ~RawInputModifiers.Control;
                    break;
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    _rawInputModifiers &= ~RawInputModifiers.Shift;
                    break;
            }
        }

        private void OnMouseUp(MouseEvent args)
        {
            //switch (args.MouseButton)
            //{
            //    case MouseButton.Left:
            //        _rawInputModifiers &= ~RawInputModifiers.LeftMouseButton;
            //        this.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _owner, RawPointerEventType.LeftButtonUp, _lastKnownMousePosition, _rawInputModifiers));
            //        break;
            //    case MouseButton.Middle:
            //        _rawInputModifiers &= ~RawInputModifiers.MiddleMouseButton;
            //        this.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _owner, RawPointerEventType.MiddleButtonUp, _lastKnownMousePosition, _rawInputModifiers));
            //        break;
            //    case MouseButton.Right:
            //        _rawInputModifiers &= ~RawInputModifiers.RightMouseButton;
            //        this.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _owner, RawPointerEventType.RightButtonUp, _lastKnownMousePosition, _rawInputModifiers));
            //        break;
            //}
        }

        private void OnMouseDown(MouseEvent args)
        {
            //switch (args.MouseButton)
            //{
            //    case MouseButton.Left:
            //        _rawInputModifiers |= RawInputModifiers.LeftMouseButton;
            //        this.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _owner, RawPointerEventType.LeftButtonDown, _lastKnownMousePosition, _rawInputModifiers));
            //        break;
            //    case MouseButton.Middle:
            //        _rawInputModifiers |= RawInputModifiers.MiddleMouseButton;
            //        this.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _owner, RawPointerEventType.MiddleButtonDown, _lastKnownMousePosition, _rawInputModifiers));
            //        break;
            //    case MouseButton.Right:
            //        _rawInputModifiers |= RawInputModifiers.RightMouseButton;
            //        this.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _owner, RawPointerEventType.RightButtonDown, _lastKnownMousePosition, _rawInputModifiers));
            //        break;
            //}
        }


        private void OnMouseLeft()
        {
            //_rawInputModifiers &= RawInputModifiers.KeyboardMask;
            //this.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _owner, RawPointerEventType.LeaveWindow, _lastKnownMousePosition, _rawInputModifiers));
        }

        private void OnMouseMove(MouseMoveEventArgs args)
        {
            //_lastKnownMousePosition = new Point(args.MousePosition.X, args.MousePosition.Y);
            //this.Input?.Invoke(new RawPointerEventArgs(MouseDevice, 0, _owner, RawPointerEventType.Move, _lastKnownMousePosition, _rawInputModifiers));
        }
    }
}