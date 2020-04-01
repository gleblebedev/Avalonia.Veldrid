using System;
using System.Numerics;
using Avalonia.Platform;
using Veldrid;
using PixelFormat = Avalonia.Platform.PixelFormat;

namespace Avalonia.Veldrid
{
    public class TextureFramebufferSource : IDisposable
    {
        private readonly uint _mipLevels;
        private readonly bool _allowNpow2;
        private readonly Lockable _lockable;
        private Texture _texture;
        private FramebufferSize _size;

        public TextureFramebufferSource(GraphicsDevice gd, FramebufferSize size,
            PixelFormat pixelFormat = PixelFormat.Rgba8888, uint mipLevels = 1, bool allowNpow2 = false,
            double dpi = 96)
        {
            _mipLevels = mipLevels;
            _allowNpow2 = allowNpow2;
            GraphicsDevice = gd;
            _lockable = new Lockable(this);
            Format = pixelFormat;
            Size = size;
            Dpi = dpi;

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

        public double Dpi { get; set; }

        public PixelFormat Format { get; }

        public FramebufferSize Size
        {
            get => _size;
            set
            {
                if (value.Width == 0 || value.Height == 0)
                    throw new ArgumentOutOfRangeException("Size can't be zero");
                _size = value;
                if (_allowNpow2)
                {
                    TextureSize = _size;
                }
                else
                {
                    var width = NextPowerOf2(_size.Width);
                    var height = NextPowerOf2(_size.Height);
                    TextureSize = new FramebufferSize(width, height);
                }
            }
        }

        public FramebufferSize TextureSize { get; private set; }

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
                var mipLevels = (int) _mipLevels;
                while (mipLevels > 0)
                {
                    var size = 1u << (mipLevels - 1);
                    if (size <= framebufferSize.Width && size <= framebufferSize.Height) break;

                    --mipLevels;
                }

                if (mipLevels == 0) mipLevels = 1;
                _texture = factory.CreateTexture(new TextureDescription(framebufferSize.Width, framebufferSize.Height,
                    1, (uint) mipLevels, 1,
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
                    var stagingTexture = _framebufferSource.Size;
                    return new PixelSize((int) stagingTexture.Width,
                        (int) stagingTexture.Height);
                }
            }

            public int RowBytes => (int) (_mapping.SizeInBytes / _framebufferSource.GetStagingTexture().Height);

            public Vector Dpi => new Vector(_framebufferSource.Dpi, _framebufferSource.Dpi);

            public PixelFormat Format => _framebufferSource.Format;

            public void Lock()
            {
                _mapping = _framebufferSource.GraphicsDevice.Map<byte>(_framebufferSource.GetStagingTexture(),
                    MapMode.Write);
            }

            public void Dispose()
            {
                SaveFramebuffer();
                _framebufferSource.GraphicsDevice.Unmap(_framebufferSource.GetStagingTexture());
                _mapping = new MappedResourceView<byte>();
            }

#if DEBUG_FRAMEBUFFER
            private int _tgaCounter = 0;

            private void SaveFramebuffer()
            {
                var name = "Framebuffer"+_tgaCounter+".tga";
                ++_tgaCounter;
                var stagingTexture = _framebufferSource.GetStagingTexture();
                var rowBytes = RowBytes;
                using (var f = File.Create(name))
                {
                    using (var w = new BinaryWriter(f))
                    {
                        w.Write((byte)0);
                        w.Write((byte)0);
                        w.Write((byte)2);
                        w.Write((ushort)0);
                        w.Write((ushort)0);
                        w.Write((byte)0);
                        w.Write((ushort)0);
                        w.Write((ushort)0);
                        var width = stagingTexture.Width;
                        w.Write((byte)(width & 0x0FF));
                        w.Write((byte)((width>>8) & 0x0FF));
                        var height = stagingTexture.Height;
                        w.Write((byte)(height & 0x0FF));
                        w.Write((byte)((height >> 8) & 0x0FF));
                        w.Write((byte)32);
                        w.Write((byte)0);
                        for (int y = (int)height-1; y >= 0; --y)
                        {
                            for (uint x = 0; x < width; ++x)
                            {
                                w.Write(_mapping[(int)(x * 4 + y * rowBytes + 0)]);
                                w.Write(_mapping[(int)(x * 4 + y * rowBytes + 1)]);
                                w.Write(_mapping[(int)(x * 4 + y * rowBytes + 2)]);
                                w.Write(_mapping[(int)(x * 4 + y * rowBytes + 3)]);
                            }
                        }
                    }
                }
            }
#else
            private void SaveFramebuffer()
            {
            }
#endif
        }
    }
}