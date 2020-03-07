using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;

namespace Avalonia.Veldrid
{
    public class VeldridWindowImpl : VeldridTopLevelImpl, IWindowImpl
    {
        private Size _minSize;
        private Size _maxSize;
        private WindowState _windowState;

        public VeldridWindowImpl(AvaloniaVeldridContext veldridContext) : base(veldridContext)
        {
        }

        public virtual IPlatformHandle Handle { get; }
        public virtual Size MaxClientSize => _maxSize;
        public virtual IScreenImpl Screen => VeldridContext.Screen;
        public Action Deactivated { get; set; }
        public Action Activated { get; set; }

        public virtual WindowState WindowState
        {
            get => _windowState;
            set
            {
                if (_windowState != value)
                {
                    _windowState = value;
                    if (_windowState == WindowState.Maximized)
                        IsFullscreen = true;
                    else
                        IsFullscreen = false;
                    WindowStateChanged?.Invoke(_windowState);
                }
            }
        }

        public Action<WindowState> WindowStateChanged { get; set; }
        public Func<bool> Closing { get; set; }

        public virtual void Activate()
        {
        }

        public virtual void SetTopmost(bool value)
        {
        }

        public virtual void SetTitle(string title)
        {
        }

        public virtual void ShowDialog(IWindowImpl parent)
        {
        }

        public virtual void SetSystemDecorations(bool enabled)
        {
        }

        public virtual void SetIcon(IWindowIconImpl icon)
        {
        }

        public virtual void ShowTaskbarIcon(bool value)
        {
        }

        public virtual void CanResize(bool value)
        {
        }

        public virtual void BeginMoveDrag(PointerPressedEventArgs e)
        {
        }

        public virtual void BeginResizeDrag(WindowEdge edge, PointerPressedEventArgs e)
        {
        }

        public virtual void Move(PixelPoint point)
        {
            Position = point;
        }

        public void SetMinMaxSize(Size minSize, Size maxSize)
        {
            _minSize = minSize;
            _maxSize = maxSize;
        }
    }
}