using System;
using Veldrid;
using Veldrid.Sdl2;

namespace Avalonia.Veldrid.Sdl2
{
    public class Sdl2KeyboardAdapter:IDisposable
    {
        private readonly Sdl2Window _window;
        private readonly KeyboardAdapter _keyboard;

        public Sdl2KeyboardAdapter(Sdl2Window window, KeyboardAdapter keyboard)
        {
            _window = window;
            _keyboard = keyboard;
            _window.KeyUp += OnKeyUp;
            _window.KeyDown += OnKeyDown;
        }
        private void OnKeyDown(KeyEvent obj)
        {
            _keyboard.OnKeyDown(obj.Key.ToAvaloniaKey());
        }

        private void OnKeyUp(KeyEvent obj)
        {
            _keyboard.OnKeyUp(obj.Key.ToAvaloniaKey());
        }

        public void Dispose()
        {
            _window.KeyUp -= OnKeyUp;
            _window.KeyDown -= OnKeyDown;
        }
    }
}