using System.Collections;
using System.Collections.Generic;
using Avalonia.Platform;

namespace Avalonia.Veldrid
{
    public class VeldridScreenStub : IScreenImpl, IReadOnlyList<Screen>
    {
        public VeldridScreenStub()
        {
            SetScreenSize(96, new FramebufferSize(1920, 1280));
        }

        public VeldridScreenStub(Screen screen)
        {
            Screen = screen;
        }

        public VeldridScreenStub(double dpi, FramebufferSize framebufferSize)
        {
            SetScreenSize(dpi, framebufferSize);
        }

        public FramebufferSize Size
        {
            get => new FramebufferSize((uint) Screen.WorkingArea.Width, (uint) Screen.WorkingArea.Height);
            set => SetScreenSize(Screen.PixelDensity, value);
        }

        public Screen Screen { get; private set; }

        int IReadOnlyCollection<Screen>.Count => 1;

        int IScreenImpl.ScreenCount => 1;

        IReadOnlyList<Screen> IScreenImpl.AllScreens => this;

        Screen IReadOnlyList<Screen>.this[int index] => Screen;

        public IEnumerator<Screen> GetEnumerator()
        {
            yield return Screen;
        }

        private void SetScreenSize(double pixelDensity, FramebufferSize framebufferSize)
        {
            Screen = new Screen(pixelDensity,
                new PixelRect(0, 0, (int) framebufferSize.Width, (int) framebufferSize.Height),
                new PixelRect(0, 0, (int) framebufferSize.Width, (int) framebufferSize.Height), true);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}