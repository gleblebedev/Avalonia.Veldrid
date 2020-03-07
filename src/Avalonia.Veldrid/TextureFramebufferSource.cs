using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.Platform;
using Veldrid;
using PixelFormat = Avalonia.Platform.PixelFormat;

namespace Avalonia.Veldrid
{
    public class TextureFramebufferSource : IDisposable
    {
        private readonly Lockable _lockable;
        private Texture _texture;
        private FramebufferSize _size;

        public TextureFramebufferSource(GraphicsDevice gd, FramebufferSize size,
            PixelFormat pixelFormat = PixelFormat.Rgba8888)
        {
            GraphicsDevice = gd;
            _lockable = new Lockable(this);
            Format = pixelFormat;
            Size = size;

            switch (Format)
            {
                case PixelFormat.Rgba8888:
                    VeldridFormat = global::Veldrid.PixelFormat.R8_G8_B8_A8_UNorm;
                    break;
                case PixelFormat.Bgra8888:
                    VeldridFormat = global::Veldrid.PixelFormat.B8_G8_R8_A8_UNorm;
                    break;
                default:
                    throw new NotImplementedException("Pixel format " + Format + " not supported");
            }
        }

        public global::Veldrid.PixelFormat VeldridFormat { get; }

        public GraphicsDevice GraphicsDevice { get; }

        public Vector Dpi => new Vector(96, 96);

        public PixelFormat Format { get; }

        public FramebufferSize Size
        {
            get => _size;
            set
            {
                _size = value;
                TextureSize = new FramebufferSize(NextPowerOf2(_size.Width), NextPowerOf2(_size.Height));
            }
        }

        public FramebufferSize TextureSize { get; set; }

        public Vector2 ViewportSize
        {
            get
            {
                var size = Size;
                var textureSize = TextureSize;
                return new Vector2(size.Width / (float) textureSize.Width, size.Height / (float) textureSize.Height);
            }
        }

        public Texture GetStagingTexture()
        {
            var framebufferSize = TextureSize;
            if (_texture == null || _texture.Width != framebufferSize.Width ||
                _texture.Height != framebufferSize.Height)
            {
                _texture?.Dispose();
                var factory = GraphicsDevice.ResourceFactory;
                _texture = factory.CreateTexture(new TextureDescription(framebufferSize.Width, framebufferSize.Height,
                    1, 1, 1,
                    VeldridFormat, TextureUsage.Staging, TextureType.Texture2D));
            }

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

        private uint NextPowerOf2(uint size)
        {
            if (size < 2)
                return 1;
            --size;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            return size + 1;
        }

        private class Lockable : ILockedFramebuffer
        {
            private readonly TextureFramebufferSource _framebufferSource;
            private MappedResourceView<byte> _mapping;

            public Lockable(TextureFramebufferSource framebufferSource)
            {
                _framebufferSource = framebufferSource;
            }

            public IntPtr Address => _mapping.MappedResource.Data;

            public PixelSize Size
            {
                get
                {
                    var stagingTexture = _framebufferSource.GetStagingTexture();
                    return new PixelSize((int) stagingTexture.Width,
                        (int) stagingTexture.Height);
                }
            }

            public int RowBytes => (int) (_mapping.SizeInBytes / _framebufferSource.GetStagingTexture().Height);

            public Vector Dpi => _framebufferSource.Dpi;

            public PixelFormat Format => _framebufferSource.Format;

            public void Lock()
            {
                _mapping = _framebufferSource.GraphicsDevice.Map<byte>(_framebufferSource.GetStagingTexture(),
                    MapMode.Write);
                //unsafe
                //{
                //    var size = _mapping.SizeInBytes;
                //    var span = new Span<byte>(_mapping.MappedResource.Data.ToPointer(), (int)size);
                //    span.Fill(255);
                //}
            }

            public void Dispose()
            {
                _framebufferSource.GraphicsDevice.Unmap(_framebufferSource.GetStagingTexture());
                _mapping = new MappedResourceView<byte>();
            }
        }
    }
}