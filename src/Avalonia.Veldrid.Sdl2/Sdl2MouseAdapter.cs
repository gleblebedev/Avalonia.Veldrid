using System;
using Veldrid;
using Veldrid.Sdl2;

namespace Avalonia.Veldrid.Sdl2
{
    public class Sdl2MouseAdapter : IDisposable
    {
        private readonly Sdl2Window _window;
        private readonly PointerAdapter _pointerAdapter;

        public Sdl2MouseAdapter(Sdl2Window window, PointerAdapter pointer)
        {
            _window = window;
            _pointerAdapter = pointer;
            _window.MouseMove += OnMouseMove;
            _window.MouseEntered += OnMouseEntered;
            _window.MouseLeft += OnMouseLeft;
            _window.MouseDown += OnMouseDown;
            _window.MouseUp += OnMouseUp;
        }
        private void OnMouseUp(MouseEvent obj)
        {
            _pointerAdapter.OnButtonUp(obj.MouseButton.ToAvalonia());
        }

        private void OnMouseDown(MouseEvent obj)
        {
            _pointerAdapter.OnButtonDown(obj.MouseButton.ToAvalonia());
        }

        private void OnMouseEntered()
        {
            _pointerAdapter.OnEntered();
        }

        private void OnMouseLeft()
        {
            _pointerAdapter.OnLeft();
        }

        private void OnMouseMove(MouseMoveEventArgs obj)
        {
            _pointerAdapter.OnMove(obj.MousePosition);
        }

        public void Dispose()
        {
            _window.MouseMove -= OnMouseMove;
            _window.MouseEntered -= OnMouseEntered;
            _window.MouseLeft -= OnMouseLeft;
            _window.MouseDown -= OnMouseDown;
            _window.MouseUp -= OnMouseUp;
        }
    }
}