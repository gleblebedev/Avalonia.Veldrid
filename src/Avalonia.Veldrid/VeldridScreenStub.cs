using System.Collections.Generic;
using Avalonia.Platform;

namespace Avalonia.Veldrid
{
    public class VeldridScreenStub : IScreenImpl
    {
        private readonly Screen[] _screens;
        private Screen _screen;

        public VeldridScreenStub()
        {
            _screen = new Screen(96, new PixelRect(0, 0, 1920, 1280),
                new PixelRect(0, 0, 1920, 1280), true);
            _screens = new[] {_screen};
        }

        public FramebufferSize Size
        {
            get => new FramebufferSize((uint) _screen.WorkingArea.Width, (uint) _screen.WorkingArea.Height);
            set
            {
                _screen = new Screen(96, new PixelRect(0, 0, (int) value.Width, (int) value.Height),
                    new PixelRect(0, 0, (int) value.Width, (int) value.Height), true);
                _screens[0] = _screen;
            }
        }

        int IScreenImpl.ScreenCount => _screens.Length;

        IReadOnlyList<Screen> IScreenImpl.AllScreens => _screens;
    }
}