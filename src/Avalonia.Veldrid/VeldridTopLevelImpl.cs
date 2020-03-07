using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Veldrid.Uniforms;
using Veldrid;

namespace Avalonia.Veldrid
{
    public class VeldridTopLevelImpl : ITopLevelImpl, IFramebufferPlatformSurface
    {
        private static readonly Matrix4x4 _fullScreenModel = Matrix4x4.CreateTranslation(0, 0, 0.999f);

        private readonly ManagedDeferredRendererLock _rendererLock = new ManagedDeferredRendererLock();
        private readonly object _gate = new object();
        private TextureFramebufferSource _framebufferSource;
        private DeviceBuffer _uniformBuffer;

        private Texture _texture;
        private ResourceSet _resrouceSet;
        private bool _hidden;
        private PixelPoint _position;
        private double _scaling = 1;
        private bool _isFullscreen;
        private Size _clientSizeCache;
        private FramebufferSize _framebufferSize = new FramebufferSize(100, 100);
        private WindowUniforms _uniforms;
        private bool _updateTexture;
        private float? _texelSize;

        public VeldridTopLevelImpl(AvaloniaVeldridContext veldridContext)
        {
            VeldridContext = veldridContext;
            VeldridContext.DeviceCreated += OnDeviceCreated;
            VeldridContext.DeviceDestroyed += OnDeviceDestroyed;

            if (veldridContext.GraphicsDevice != null)
            {
                OnDeviceCreated(veldridContext.GraphicsDevice);
            }
            veldridContext.AddWindow(this);
        }

        private void OnDeviceDestroyed()
        {
            _framebufferSource?.Dispose();
            _uniformBuffer?.Dispose();
            _framebufferSource = null;
            _uniformBuffer = null;
        }

        private void OnDeviceCreated(GraphicsDevice obj)
        {
            _framebufferSource = new TextureFramebufferSource(VeldridContext.GraphicsDevice, _framebufferSize);
            _clientSizeCache = ClientSize;
            var sizeInBytes = (uint)Marshal.SizeOf<WindowUniforms>();
            sizeInBytes = 16 * ((sizeInBytes + 15) / 16);
            _uniformBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(sizeInBytes,
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _updateTexture = true;
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

        public virtual FramebufferSize FramebufferSize => IsFullscreen ? VeldridContext.ScreenSize : _framebufferSize;

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
                return new Size(framebufferSize.Width, framebufferSize.Height);
            }
        }

        /// <summary>
        ///     Gets the scaling factor for the toplevel.
        /// </summary>
        public virtual double Scaling
        {
            get => _scaling;
            set
            {
                if (_scaling != value)
                {
                    _scaling = value;
                    ScalingChanged?.Invoke(_scaling);
                }
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
            _hidden = false;
        }

        public virtual void Hide()
        {
            _hidden = true;
        }

        public virtual void Resize(Size clientSize)
        {
            _framebufferSize = new FramebufferSize((uint) clientSize.Width, (uint) clientSize.Height);
            FireResizedIfNecessary();
        }

        public virtual void Render(CommandList commandList)
        {
            if (_framebufferSource == null)
                return;

            if (_hidden)
                return;

            if (IsFullscreen) FireResizedIfNecessary();

            if (_updateTexture)
            {
                var clientSize = ClientSize;
                Paint?.Invoke(new Rect(0, 0, clientSize.Width, clientSize.Height));
            }

            if (_texture == null)
                return;

            EnsureTexture();

            lock (_gate)
            {
                _uniforms.Projection = GetActiveProjection();
                _uniforms.View = GetActiveView();
                _uniforms.Model = GetActiveModel();
                _uniforms.Viewport = _framebufferSource.ViewportSize;
                commandList.UpdateBuffer(_uniformBuffer, 0, ref _uniforms);
                if (_updateTexture)
                {
                    var stagingTexture = _framebufferSource.GetStagingTexture();
                    commandList.CopyTexture(stagingTexture, _texture, 0, 0);
                    commandList.GenerateMipmaps(_texture);
                    _updateTexture = false;
                }

                commandList.SetPipeline(VeldridContext.Pipeline);
                commandList.SetGraphicsResourceSet(0, _resrouceSet);
                commandList.Draw(4);
            }
        }

        public void ResetTexelSize()
        {
            _texelSize = null;
        }

        public RaycastResult? Raycast(ClipSpaceRay ray)
        {
            var projection = GetActiveProjection();
            var view = GetActiveView();
            var model = GetActiveModel();

            var pvm = model * view * projection;
            Matrix4x4.Invert(pvm, out var invPVM);
            var from4 = Vector4.Transform(InvertYIfNotFullscreen(ray.From), invPVM);
            var from = new Vector3(from4.X, from4.Y, from4.Z) * (1.0f / from4.W);
            var to4 = Vector4.Transform(InvertYIfNotFullscreen(ray.To), invPVM);
            var to = new Vector3(to4.X, to4.Y, to4.Z) * (1.0f / to4.W);

            if (from.Z * to.Z > 1e-6f) return null;

            var k = -from.Z / (to.Z - from.Z);
            if (float.IsNaN(k)) k = 0;
            var pos = Vector3.Lerp(from, to, k);
            if (pos.X < -1 ||
                pos.X > 1 ||
                pos.Y < -1 ||
                pos.Y > 1)
                return null;

            var clientSize = ClientSize;
            var clipSpacePoint = Vector4.Transform(pos, pvm);
            return new RaycastResult
            {
                WindowPoint = new Point(clientSize.Width * (pos.X + 1.0) * 0.5, clientSize.Height * (pos.Y + 1) * 0.5),
                ClipSpaceDepth = clipSpacePoint.Z / clipSpacePoint.W
            };
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public virtual void Dispose()
        {
            VeldridContext.RemoveWindow(this);
            VeldridContext.DeviceCreated -= OnDeviceCreated;
            VeldridContext.DeviceDestroyed -= OnDeviceDestroyed;

            lock (_gate)
            {
                _resrouceSet?.Dispose();
                _texture?.Dispose();
                _framebufferSource?.Dispose();
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
            _updateTexture = true;
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

        private Vector4 InvertYIfNotFullscreen(Vector4 rayFrom)
        {
            if (IsFullscreen)
                return rayFrom;
            return new Vector4(rayFrom.X, -rayFrom.Y, rayFrom.Z, rayFrom.W);
        }

        private void FireResizedIfNecessary()
        {
            if (_framebufferSource != null)
            {
                var framebufferSize = FramebufferSize;
                if (framebufferSize != _framebufferSource.Size)
                {
                    _framebufferSource.Size = framebufferSize;
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

            var clientSize = FramebufferSize;
            var scale = Matrix4x4.CreateScale(clientSize.Width * TexelSize, clientSize.Height * TexelSize, 1.0f);
            return scale*WorldTransform;
        }

        private Matrix4x4 GetActiveProjection()
        {
            return IsFullscreen ? Matrix4x4.Identity : VeldridContext.Projection;
        }

        private void EnsureTexture()
        {
            lock (_gate)
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
                        TextureUsage.Sampled | TextureUsage.GenerateMipmaps,
                        stagingTexture.Type);
                    _texture = factory.CreateTexture(textureDescription);
                    _resrouceSet = factory.CreateResourceSet(new ResourceSetDescription(
                        VeldridContext.TextureResourceLayout, _uniformBuffer, _texture,
                        VeldridContext.Sampler));
                }
            }
        }
    }
}