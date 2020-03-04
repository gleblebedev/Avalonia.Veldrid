using System;
using Avalonia.Platform;
using Veldrid;
using PixelFormat = Avalonia.Platform.PixelFormat;

namespace Avalonia.Veldrid
{
    public class TextureWindowFramebuffer : IDisposable
    {
        private readonly Texture _texture;
        private readonly Lockable _lockable;

        public TextureWindowFramebuffer(GraphicsDevice gd, Texture texture)
        {
            GraphicsDevice = gd;
            _texture = texture;
            _lockable = new Lockable(this);
            switch (_texture.Format)
            {
                case global::Veldrid.PixelFormat.B8_G8_R8_A8_UNorm:
                    Format = PixelFormat.Bgra8888;
                    break;
                case global::Veldrid.PixelFormat.R8_G8_B8_A8_UNorm:
                    Format = PixelFormat.Rgba8888;
                    break;
                default:
                    throw new NotImplementedException(_texture.Format + " not supported.");
            }
        }

        public GraphicsDevice GraphicsDevice { get; }

        public PixelSize Size => new PixelSize((int) _texture.Width, (int) _texture.Height);

        public Vector Dpi => new Vector(96, 96);
        
        public PixelFormat Format { get; }

        public Texture GetStagingTexture()
        {
            return _texture;
        }


        public ILockedFramebuffer Lock()
        {
            _lockable.Lock();
            return _lockable;
        }

        public void Dispose()
        {
            _texture.Dispose();
        }

        private class Lockable : ILockedFramebuffer
        {
            private readonly TextureWindowFramebuffer _framebuffer;
            private MappedResourceView<byte> _mapping;

            public Lockable(TextureWindowFramebuffer framebuffer)
            {
                _framebuffer = framebuffer;
            }

            public IntPtr Address => _mapping.MappedResource.Data;

            public PixelSize Size => _framebuffer.Size;

            public int RowBytes => (int)(_mapping.SizeInBytes / _framebuffer.GetStagingTexture().Height);

            public Vector Dpi => _framebuffer.Dpi;

            public PixelFormat Format => _framebuffer.Format;

            public void Lock()
            {
                _mapping = _framebuffer.GraphicsDevice.Map<byte>(_framebuffer.GetStagingTexture(), MapMode.Write);
            }

            public void Dispose()
            {
                _framebuffer.GraphicsDevice.Unmap(_framebuffer.GetStagingTexture());
                _mapping = new MappedResourceView<byte>();
            }
        }
    }
}