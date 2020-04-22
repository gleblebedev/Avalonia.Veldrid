using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Veldrid.Uniforms;
using Veldrid;
using PixelFormat = Avalonia.Platform.PixelFormat;

namespace Avalonia.Veldrid
{
    public class VeldridTopLevelImpl : ITopLevelImpl, IFramebufferPlatformSurface
    {
        private static readonly Matrix4x4 _fullScreenModel = Matrix4x4.CreateTranslation(0, 0, 0.999f);

        private readonly ManagedDeferredRendererLock _rendererLock = new ManagedDeferredRendererLock();
        private readonly object _framebufferLock = new object();
        private TextureFramebufferSource _framebufferSource;
        private DeviceBuffer _uniformBuffer;

        private Texture _texture;
        private ResourceSet _resrouceSet;
        private bool _isVisible;
        private PixelPoint _position;
        private bool _isFullscreen;
        private Size _clientSizeCache;
        private FramebufferSize _framebufferSize;
        private WindowUniforms _uniforms;
        private float? _texelSize;
        private object _invalidRegionLock = new object();
        private Rect _invalidRegion = Rect.Empty;
        private double _dpi = 96.0;
        private bool _hasActualSize;
        private bool _hasUpdatedImage;

        public VeldridTopLevelImpl(AvaloniaVeldridContext veldridContext)
        {
            VeldridContext = veldridContext;
            _framebufferSize = new FramebufferSize(100, 100);
            VeldridContext.DeviceCreated += OnDeviceCreated;
            VeldridContext.DeviceDestroyed += OnDeviceDestroyed;

            if (veldridContext.GraphicsDevice != null) OnDeviceCreated(veldridContext.GraphicsDevice);
            veldridContext.AddWindow(this);
        }


        public virtual PixelPoint Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    PositionChanged?.Invoke(_position);
                }
            }
        }

        public virtual IScreenImpl Screen => new VeldridScreenStub(Dpi, FramebufferSize);


        public virtual FramebufferSize FramebufferSize => IsFullscreen ? VeldridContext.ScreenSize : _framebufferSize;

        public virtual IInputDevice TouchDevice => VeldridContext.TouchDevice;

        public double Dpi
        {
            get => _dpi;
            set
            {
                if (_dpi != value)
                {
                    var clientSize = ClientSize;
                    _dpi = value;
                    using (TimedLock.Lock(_framebufferLock))
                    {
                        if (_framebufferSource != null) _framebufferSource.Dpi = value;
                    }
                    if (_hasActualSize) Resize(clientSize);
                    ScalingChanged?.Invoke(Scaling);
                    //Invalidate(new Rect(new Point(0,0), ClientSize));
                }
            }
        }

        public AvaloniaVeldridContext VeldridContext { get; }

        public GraphicsDevice GraphicsDevice => VeldridContext?.GraphicsDevice;

        public Action<PixelPoint> PositionChanged { get; set; }

        public bool IsFullscreen
        {
            get => _isFullscreen;
            set
            {
                if (_isFullscreen != value)
                {
                    _isFullscreen = value;
                    FireResizedIfNecessary();
                }
            }
        }

        public Matrix4x4 WorldTransform { get; set; } = Matrix4x4.Identity;

        public IInputRoot InputRoot { get; private set; }

        public float TexelSize
        {
            get => _texelSize ?? VeldridContext.TexelSize;
            set => _texelSize = value;
        }

        /// <summary>
        ///     Gets the client size of the toplevel.
        /// </summary>
        public virtual Size ClientSize
        {
            get
            {
                var framebufferSize = FramebufferSize;
                return new Size(framebufferSize.Width / Scaling, framebufferSize.Height / Scaling);
            }
        }

        /// <summary>
        ///     Gets the scaling factor for the toplevel.
        /// </summary>
        public virtual double Scaling
        {
            get => _dpi / 96.0;
            set
            {
                var scaling = Scaling;
                if (scaling != value) Dpi = 96.0 * value;
            }
        }

        /// <summary>
        ///     The list of native platform's surfaces that can be consumed by rendering subsystems.
        /// </summary>
        /// <remarks>
        ///     Rendering platform will check that list and see if it can utilize one of them to output.
        ///     It should be enough to expose a native window handle via IPlatformHandle
        ///     and add support for framebuffer (even if it's emulated one) via IFramebufferPlatformSurface.
        ///     If you have some rendering platform that's tied to your particular windowing platform,
        ///     just expose some toolkit-specific object (e. g. Func&lt;Gdk.Drawable&gt; in case of GTK#+Cairo)
        /// </remarks>
        public virtual IEnumerable<object> Surfaces
        {
            get { yield return this; }
        }

        /// <summary>Gets a mouse device associated with toplevel</summary>
        public virtual IMouseDevice MouseDevice => VeldridContext.MouseDevice;

        /// <summary>
        ///     Gets or sets a method called when the toplevel receives input.
        /// </summary>
        public Action<RawInputEventArgs> Input { get; set; }

        /// <summary>
        ///     Gets or sets a method called when the toplevel requires painting.
        /// </summary>
        public Action<Rect> Paint { get; set; }

        /// <summary>
        ///     Gets or sets a method called when the toplevel is resized.
        /// </summary>
        public Action<Size> Resized { get; set; }

        /// <summary>
        ///     Gets or sets a method called when the toplevel's scaling changes.
        /// </summary>
        public Action<double> ScalingChanged { get; set; }

        /// <summary>
        ///     Gets or sets a method called when the underlying implementation is destroyed.
        /// </summary>
        public Action Closed { get; set; }

        public virtual void Show()
        {
            _isVisible = true;
            
        }

        public virtual void Hide()
        {
            _isVisible = false;
        }

        public virtual void Resize(Size clientSize)
        {
            _hasActualSize = true;
            var scaling = Scaling;
            _framebufferSize =
                new FramebufferSize((uint) (clientSize.Width * scaling), (uint) (clientSize.Height * scaling));
            FireResizedIfNecessary();
        }

        internal void PaintImpl()
        {
            var paint = Paint;
            if (paint == null)
                return;

            lock (_framebufferLock)
            {
                if (_framebufferSource == null)
                    return;
                var updateTexture = _invalidRegion != Rect.Empty;
                if (updateTexture)
                {
                    paint?.Invoke(_invalidRegion.Intersect(new Rect(new Point(0, 0), ClientSize)));
                    _invalidRegion = Rect.Empty;
                    _hasUpdatedImage = true;
                }
            }
        }

        public virtual void Render(CommandList commandList)
        {
            var framebufferSource = _framebufferSource;

            if (framebufferSource == null)
                return;

            if (framebufferSource.Size == new FramebufferSize(0, 0))
                return;

            if (!_isVisible)
                return;

            if (IsFullscreen) FireResizedIfNecessary();

            if (_hasUpdatedImage)
            {
                if (Monitor.TryEnter(_framebufferLock))
                {

                    try
                    {
                        EnsureTexture();

                        var stagingTexture = framebufferSource.GetStagingTexture();
                        commandList.CopyTexture(stagingTexture, _texture, 0, 0);
                        if (stagingTexture.MipLevels > 1)
                            commandList.GenerateMipmaps(_texture);
                    }
                    finally
                    {
                        Monitor.Exit(_framebufferLock);
                    }

                    _hasUpdatedImage = false;
                }
            }

            if (_texture == null)
                return;

            _uniforms.Projection = GetActiveProjection();
            _uniforms.View = GetActiveView();
            _uniforms.Model = GetActiveModel();
            _uniforms.Viewport = framebufferSource.ViewportSize;
            commandList.UpdateBuffer(_uniformBuffer, 0, ref _uniforms);
            commandList.SetPipeline(VeldridContext.Pipeline);
            commandList.SetGraphicsResourceSet(0, _resrouceSet);
            commandList.Draw(4);
        }

        public void ResetTexelSize()
        {
            _texelSize = null;
        }

        public RaycastResult? Project(Vector3 worldSpacePosition)
        {
            if (!_isVisible)
                return null;

            if (IsFullscreen)
                return null;

            var vp = GetContextViewProjection();

            var mvp = GetModelViewProjection();
            Matrix4x4.Invert(mvp, out var invMVP);
            var from4 = Vector4.Transform(Vector4.Transform(worldSpacePosition, vp), invMVP);
            var pos = from4.ToPositionVec3();
            if (pos.X < -1 ||
                pos.X > 1 ||
                pos.Y < -1 ||
                pos.Y > 1)
                return null;

            var projectionPos = pos;
            projectionPos.Z = 0;
            var clipSpacePoint = Vector4.Transform(projectionPos.ToPositionVec4(), mvp);
            Matrix4x4.Invert(vp, out var invVP);
            var worldSpaceHit = Vector4.Transform(clipSpacePoint, invVP).ToPositionVec3();
            var distance = (worldSpaceHit - worldSpacePosition).Length();

            var clientSize = ClientSize;

            return new RaycastResult
            {
                WindowImpl = this,
                WindowPoint = new Point(clientSize.Width * (pos.X + 1.0) * 0.5,
                    clientSize.Height * (-pos.Y + 1.0) * 0.5),
                ClipSpaceDepth = clipSpacePoint.Z / clipSpacePoint.W,
                WorldSpaceHitPoint = worldSpaceHit,
                Distance = distance
            };
        }

        public RaycastResult? Raycast(ClipSpaceRay ray)
        {
            if (!_isVisible)
                return null;
            var mvp = GetModelViewProjection();
            Matrix4x4.Invert(mvp, out var invMVP);
            var from = Vector4.Transform(ray.From, invMVP).ToPositionVec3();
            var to = Vector4.Transform(ray.To, invMVP).ToPositionVec3();

            if (from.Z * to.Z > 1e-6f) return null;

            var k = -from.Z / (to.Z - from.Z);
            if (float.IsNaN(k)) k = 0;
            var pos = Vector3.Lerp(from, to, k);
            if (pos.X < -1 ||
                pos.X > 1 ||
                pos.Y < -1 ||
                pos.Y > 1)
                return null;

            var clipSpacePoint = Vector4.Transform(pos.ToPositionVec4(), mvp);
            var vp = GetContextViewProjection();
            Matrix4x4.Invert(vp, out var invVP);
            var worldSpaceFrom = Vector4.Transform(ray.From, invVP).ToPositionVec3();
            var worldSpaceHit = Vector4.Transform(clipSpacePoint, invVP).ToPositionVec3();

            var clientSize = ClientSize;
            return new RaycastResult
            {
                WindowImpl = this,
                WindowPoint = new Point(clientSize.Width * (pos.X + 1.0) * 0.5,
                    clientSize.Height * (-pos.Y + 1.0) * 0.5),
                ClipSpaceDepth = clipSpacePoint.Z / clipSpacePoint.W,
                WorldSpaceHitPoint = worldSpaceHit,
                Distance = (worldSpaceHit - worldSpaceFrom).Length()
            };
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public virtual void Dispose()
        {
            VeldridContext.RemoveWindow(this);
            VeldridContext.DeviceCreated -= OnDeviceCreated;
            VeldridContext.DeviceDestroyed -= OnDeviceDestroyed;

            lock (_framebufferLock)
            {
                _resrouceSet?.Dispose();
                _texture?.Dispose();
                _framebufferSource?.Dispose();
                _framebufferSource = null;
            }
        }

        /// <summary>Provides a framebuffer descriptor for drawing.</summary>
        /// <remarks>
        ///     Contents should be drawn on actual window after disposing
        /// </remarks>
        public virtual ILockedFramebuffer Lock()
        {
            EnsureTexture();
            return _framebufferSource.Lock();
        }

        /// <summary>Converts a point from screen to client coordinates.</summary>
        /// <param name="point">The point in screen coordinates.</param>
        /// <returns>The point in client coordinates.</returns>
        public virtual Point PointToClient(PixelPoint point)
        {
            var position = IsFullscreen ? new PixelPoint(0, 0) : Position;
            return (point - position).ToPoint(Scaling);
        }

        /// <summary>Converts a point from client to screen coordinates.</summary>
        /// <param name="point">The point in client coordinates.</param>
        /// <returns>The point in screen coordinates.</returns>
        public virtual PixelPoint PointToScreen(Point point)
        {
            var position = IsFullscreen ? new PixelPoint(0, 0) : Position;
            return PixelPoint.FromPoint(point, Scaling) + position;
        }

        public virtual IPopupImpl CreatePopup()
        {
            return null;
        }

        /// <summary>Invalidates a rect on the toplevel.</summary>
        public virtual void Invalidate(Rect rect)
        {
            _invalidRegion = _invalidRegion.Union(rect);
            SchedulePaint();
        }

        private void SchedulePaint()
        {
            VeldridContext.SchedulePaint(this);
        }

        /// <summary>
        ///     Sets the <see cref="T:Avalonia.Input.IInputRoot" /> for the toplevel.
        /// </summary>
        public virtual void SetInputRoot(IInputRoot inputRoot)
        {
            InputRoot = inputRoot;
        }

        /// <summary>
        ///     Creates a new renderer for the toplevel.
        /// </summary>
        /// <param name="root">The toplevel.</param>
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

        /// <summary>Sets the cursor associated with the toplevel.</summary>
        /// <param name="cursor">The cursor. Use null for default cursor</param>
        public void SetCursor(IPlatformHandle cursor)
        {
        }

        private void OnDeviceDestroyed()
        {
            lock (_framebufferLock)
            {
                _framebufferSource?.Dispose();
                _uniformBuffer?.Dispose();
                _framebufferSource = null;
                _uniformBuffer = null;
            }
        }

        private void OnDeviceCreated(GraphicsDevice obj)
        {
            lock (_framebufferLock)
            {
                _framebufferSource = new TextureFramebufferSource(
                    VeldridContext.GraphicsDevice,
                    _framebufferSize,
                    PixelFormat.Rgba8888,
                    VeldridContext.MipLevels,
                    VeldridContext.AllowNPow2Textures,
                    Dpi);
                _clientSizeCache = ClientSize;
                var sizeInBytes = (uint) Marshal.SizeOf<WindowUniforms>();
                sizeInBytes = 16 * ((sizeInBytes + 15) / 16);
                _uniformBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(sizeInBytes,
                        BufferUsage.UniformBuffer | BufferUsage.Dynamic));
                Invalidate(new Rect(0, 0, double.MaxValue, double.MaxValue));
            }
        }

        private void FireResizedIfNecessary()
        {
            using (TimedLock.Lock(_framebufferLock))
            {
                if (_framebufferSource != null)
                {
                    var framebufferSize = FramebufferSize;
                    if (framebufferSize != _framebufferSource.Size) _framebufferSource.Size = framebufferSize;
                }
            }

            var size = ClientSize;
            if (_clientSizeCache != size)
            {
                _clientSizeCache = size;
                Resized?.Invoke(size);
            }
        }

        private Matrix4x4 GetActiveView()
        {
            return IsFullscreen ? Matrix4x4.Identity : VeldridContext.View;
        }

        private Matrix4x4 GetActiveModel()
        {
            if (IsFullscreen)
                return _fullScreenModel;

            var clientSize = ClientSize;
            var texelSize = TexelSize * 0.5f;
            var scale = Matrix4x4.CreateScale((float) clientSize.Width * texelSize,
                (float) clientSize.Height * texelSize, 1.0f);
            return scale * WorldTransform;
        }

        private Matrix4x4 GetActiveProjection()
        {
            return IsFullscreen ? Matrix4x4.Identity : VeldridContext.Projection;
        }

        private Matrix4x4 GetModelViewProjection()
        {
            var projection = GetActiveProjection();
            var view = GetActiveView();
            var model = GetActiveModel();

            return model * view * projection;
        }


        private Matrix4x4 GetContextViewProjection()
        {
            var projection = VeldridContext.Projection;
            var view = VeldridContext.View;

            return view * projection;
        }

        private void EnsureTexture()
        {
            using (TimedLock.Lock(_framebufferLock))
            {
                var stagingTexture = _framebufferSource.GetStagingTexture();
                if (_texture == null || _texture.Width != stagingTexture.Width ||
                    _texture.Height != stagingTexture.Height)
                {
                    _texture?.Dispose();
                    _resrouceSet?.Dispose();
                    var factory = VeldridContext.GraphicsDevice.ResourceFactory;
                    var textureDescription = new TextureDescription(
                        stagingTexture.Width,
                        stagingTexture.Height,
                        stagingTexture.Depth,
                        stagingTexture.MipLevels,
                        stagingTexture.ArrayLayers,
                        stagingTexture.Format,
                        TextureUsage.Sampled,
                        stagingTexture.Type);
                    if (stagingTexture.MipLevels > 1)
                        textureDescription.Usage |= TextureUsage.GenerateMipmaps;
                    _texture = factory.CreateTexture(textureDescription);
                    _resrouceSet = factory.CreateResourceSet(new ResourceSetDescription(
                        VeldridContext.TextureResourceLayout, _uniformBuffer, _texture,
                        VeldridContext.Sampler));
                }
            }
        }
    }
}