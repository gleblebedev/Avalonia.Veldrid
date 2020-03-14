using System.Collections;
using System.Collections.Generic;
using Avalonia.Platform;

namespace Avalonia.Veldrid
{
    public class VeldridScreenStub : IScreenImpl, IReadOnlyList<Screen>
    {
        private Screen _screen;

        public VeldridScreenStub()
        {
            SetScreenSize(96, new FramebufferSize(1920, 1280));
        }

        public VeldridScreenStub(Screen screen)
        {
            _screen = screen;
        }

        public VeldridScreenStub(double dpi, FramebufferSize framebufferSize)
        {
            SetScreenSize(dpi, framebufferSize);
        }

        private void SetScreenSize(double pixelDensity, FramebufferSize framebufferSize)
        {
            _screen = new Screen(pixelDensity,
                new PixelRect(0, 0, (int) framebufferSize.Width, (int) framebufferSize.Height),
                new PixelRect(0, 0, (int) framebufferSize.Width, (int) framebufferSize.Height), true);
        }

        public FramebufferSize Size
        {
            get => new FramebufferSize((uint) _screen.WorkingArea.Width, (uint) _screen.WorkingArea.Height);
            set
            {
                SetScreenSize(_screen.PixelDensity, value);
            }
        }

        public Screen Screen => _screen;

        int IScreenImpl.ScreenCount => 1;

        IReadOnlyList<Screen> IScreenImpl.AllScreens => this;

        public IEnumerator<Screen> GetEnumerator()
        {
            yield return _screen;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IReadOnlyCollection<Screen>.Count => 1;

        Screen IReadOnlyList<Screen>.this[int index] => _screen;
    }
}