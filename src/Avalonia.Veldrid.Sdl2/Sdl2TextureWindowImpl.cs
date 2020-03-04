using Avalonia.Input;
using Veldrid;

namespace Avalonia.Veldrid.Sdl2
{
    internal class Sdl2TextureWindowImpl : TextureWindowImpl
    {
        private readonly Sdl2AvaloniaWindow _sdl2AvaloniaWindow;

        public Sdl2TextureWindowImpl(AvaloniaVeldridContext veldridContext, OutputDescription outputDescription,
            Sdl2AvaloniaWindow sdl2AvaloniaWindow) : base(veldridContext, outputDescription)
        {
            _sdl2AvaloniaWindow = sdl2AvaloniaWindow;
        }

        public override IMouseDevice MouseDevice => _sdl2AvaloniaWindow.MouseDevice;
    }
}