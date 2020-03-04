using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering;
using Veldrid;
using PixelFormat = Veldrid.PixelFormat;

namespace Avalonia.Veldrid
{
    public class TextureWindowImpl : IWindowImpl, IFramebufferPlatformSurface
    {
        private readonly AvaloniaVeldridContext _veldridContext;
        private Texture _texture;
        private Texture _stagingTexture;
        private TextureWindowFramebuffer _framebuffer;
        private IInputRoot _owner;
        private bool _hidden;
        private Size _minSize;
        private Size _maxSize;
        private ResourceSet _resrouceSet;
        private readonly ManagedDeferredRendererLock _rendererLock = new ManagedDeferredRendererLock();

        private readonly object _gate = new object();

        public TextureWindowImpl(AvaloniaVeldridContext veldridContext, OutputDescription outputDescription)
        {
            _veldridContext = veldridContext;
        }

        public virtual uint Width { get; set; } = 200;
        public virtual uint Height { get; set; } = 100;

        public virtual Size ClientSize => new Size(Width, Height);

        public virtual double Scaling => 1;

        public virtual IEnumerable<object> Surfaces
        {
            get { yield return this; }
        }

        public virtual IMouseDevice MouseDevice { get; }
        public Action<RawInputEventArgs> Input { get; set; }
        public Action<Rect> Paint { get; set; }
        public Action<Size> Resized { get; set; }
        public Action<double> ScalingChanged { get; set; }
        public Action Closed { get; set; }

        public virtual PixelPoint Position { get; set; }
        public virtual IPlatformHandle Handle { get; }
        public virtual Size MaxClientSize => _maxSize;
        public virtual IScreenImpl Screen { get; }
        public Action<PixelPoint> PositionChanged { get; set; }
        public Action Deactivated { get; set; }
        public Action Activated { get; set; }

        public virtual WindowState WindowState { get; set; }
        public Action<WindowState> WindowStateChanged { get; set; }
        public Func<bool> Closing { get; set; }

        public virtual void RenderFullscreen(CommandList commandList)
        {
            if (_hidden)
                return;

            Paint?.Invoke(new Rect(0, 0, Width, Height));

            if (_texture == null)
                return;
            EnsureTexture();

            //commandList.ClearColorTarget(0, RgbaFloat.Blue);
            lock (_gate)
            {
                commandList.CopyTexture(_stagingTexture, _texture);
                commandList.SetPipeline(_veldridContext.FullScreenPipeline);
                commandList.SetGraphicsResourceSet(0, _resrouceSet);
                commandList.Draw(4);
            }
        }

        public void RenderSlate(CommandList commandList)
        {
            if (_hidden)
                return;
            if (_texture == null)
                return;
            EnsureTexture();

            //commandList.ClearColorTarget(0, RgbaFloat.Blue);
            commandList.CopyTexture(_stagingTexture, _texture);
            commandList.SetPipeline(_veldridContext.FullScreenPipeline);
            commandList.SetGraphicsResourceSet(0, _resrouceSet);
            commandList.Draw(4);
        }

        public virtual void Dispose()
        {
            _resrouceSet?.Dispose();
            _texture?.Dispose();
            _framebuffer?.Dispose();
        }

        public virtual ILockedFramebuffer Lock()
        {
            EnsureTexture();
            return _framebuffer.Lock();
        }

        public virtual void Invalidate(Rect rect)
        {
        }

        public virtual void SetInputRoot(IInputRoot inputRoot)
        {
            _owner = inputRoot;
        }

        public virtual Point PointToClient(PixelPoint point)
        {
            var pos = Position;
            return new Point(point.X - pos.X, point.Y - pos.Y);
        }

        public virtual PixelPoint PointToScreen(Point point)
        {
            return new PixelPoint((int) point.X, (int) point.Y) + Position;
        }

        public virtual IPopupImpl CreatePopup()
        {
            return null;
        }

        public IRenderer CreateRenderer(IRenderRoot root)
        {
            var useDeferredRendering = false;
            if (useDeferredRendering)
            {
                var loop = AvaloniaLocator.Current.GetService<IRenderLoop>();
                var customRendererFactory = AvaloniaLocator.Current.GetService<IRendererFactory>();

                if (customRendererFactory != null)
                    return customRendererFactory.Create(root, loop);
                return new DeferredRenderer(root, loop, rendererLock: _rendererLock);
            }

            return new ImmediateRenderer(root);
        }

        public void SetCursor(IPlatformHandle cursor)
        {
        }

        public virtual void Show()
        {
            _hidden = false;
        }

        public virtual void Hide()
        {
            _hidden = true;
        }


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

        public virtual void Resize(Size clientSize)
        {
            lock (_gate)
            {
                var width = (uint) clientSize.Width;
                var height = (uint) clientSize.Height;
                if (width != Width || height != Height)
                {
                    Width = width;
                    Height = height;
                    Resized?.Invoke(clientSize);
                }
            }
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

        private void EnsureTexture()
        {
            lock (_gate)
            {
                if (_texture == null || _texture.Width != Width || _texture.Height != Height)
                {
                    _texture?.Dispose();
                    _stagingTexture?.Dispose();
                    _resrouceSet?.Dispose();
                    _framebuffer?.Dispose();
                    var factory = _veldridContext.GraphicsDevice.ResourceFactory;
                    _stagingTexture = factory.CreateTexture(new TextureDescription(Width, Height, 1, 1, 1,
                        PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging, TextureType.Texture2D));
                    _texture = factory.CreateTexture(new TextureDescription(Width, Height, 1, 1, 1,
                        PixelFormat.R8_G8_B8_A8_UNorm,
                        TextureUsage.Sampled, TextureType.Texture2D));
                    _resrouceSet = factory.CreateResourceSet(new ResourceSetDescription(
                        _veldridContext.TextureResourceLayout, _texture, _veldridContext.GraphicsDevice.PointSampler));
                    _framebuffer = new TextureWindowFramebuffer(_veldridContext.GraphicsDevice, _stagingTexture);
                }
            }
        }
    }
}