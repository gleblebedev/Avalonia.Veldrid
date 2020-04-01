using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Input;
using Avalonia.Platform;
using Veldrid;

namespace Avalonia.Veldrid
{
    public class AvaloniaVeldridContext : IDisposable
    {
        private readonly object _windowsCollectionLock = new object();
        private readonly HashSet<VeldridTopLevelImpl> _windows = new HashSet<VeldridTopLevelImpl>();
        private readonly Func<GraphicsDevice, Sampler> _samplerFactory;
        private OutputDescription? _outputDescription;
        private Shader[] _shaders;
        private readonly ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();
        private WindowsCollectionView _windowsView;
        private readonly InputModifiersContainer _modifiers;
        private int _touchCounter;

        public AvaloniaVeldridContext(GraphicsDevice graphicsDevice = null, OutputDescription? outputDescription = null,
            IScreenImpl screenImpl = null, IMouseDevice mouseDevice = null, IKeyboardDevice keyboardDevice = null,
            Func<GraphicsDevice, Sampler> samplerFactory = null)
        {
            GraphicsDevice = graphicsDevice;
            _outputDescription = outputDescription;
            _samplerFactory = samplerFactory ?? DefaultSamplerFactory;
            Screen = screenImpl ?? new VeldridScreenStub();
            MouseDevice = mouseDevice ?? new MouseDevice();
            KeyboardDevice = keyboardDevice ?? new KeyboardDevice();
            TouchDevice = new TouchDevice();
            _modifiers = new InputModifiersContainer();
            PointerAdapter = new PointerAdapter(this, _modifiers);
            KeyboardAdapter = new KeyboardAdapter(this, _modifiers);
            if (GraphicsDevice != null) OnDeviceCreated();
        }

        /// <summary>
        ///     GraphicsDevice been created and available.
        /// </summary>
        public event Action<GraphicsDevice> DeviceCreated;

        /// <summary>
        ///     GraphicsDevice been destroyed.
        /// </summary>
        public event Action DeviceDestroyed;

        public IScreenImpl Screen { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public Matrix4x4 Projection { get; set; } =
            Matrix4x4.CreatePerspectiveFieldOfView((float) Math.PI * 0.5f, 1, 0.01f, 100.0f);

        public Matrix4x4 View { get; set; } = Matrix4x4.CreateLookAt(new Vector3(1, 1, 4), Vector3.Zero, Vector3.UnitY);

        public ResourceLayout TextureResourceLayout { get; private set; }
        public Pipeline Pipeline { get; private set; }

        public IMouseDevice MouseDevice { get; }
        public IInputDevice TouchDevice { get; }

        public IKeyboardDevice KeyboardDevice { get; }

        public FramebufferSize ScreenSize =>
            new FramebufferSize((uint) Screen.AllScreens[0].WorkingArea.Width,
                (uint) Screen.AllScreens[0].WorkingArea.Height);

        public float TexelSize { get; set; } = 0.01f;
        public Sampler Sampler { get; private set; }
        public KeyboardAdapter KeyboardAdapter { get; }

        public PointerAdapter PointerAdapter { get; }

        public bool AllowNPow2Textures { get; set; } = false;
        public uint MipLevels { get; set; } = 4;

        public void SetGraphicsDevice(GraphicsDevice graphicsDevice, OutputDescription? outputDescription = null)
        {
            if (GraphicsDevice != graphicsDevice)
            {
                if (GraphicsDevice != null)
                {
                    OnDeviceDestroyed();
                    _outputDescription = null;
                }

                GraphicsDevice = graphicsDevice;
                if (GraphicsDevice != null)
                {
                    _outputDescription =
                        outputDescription ?? graphicsDevice.MainSwapchain.Framebuffer.OutputDescription;
                    OnDeviceCreated();
                }
            }
        }

        public RaycastResult? Raycast(ClipSpaceRay ray)
        {
            RaycastResult? bestMatch = null;
            lock (_windowsCollectionLock)
            {
                foreach (var window in _windows)
                {
                    var res = window.Raycast(ray);
                    if (res.HasValue)
                        if (!bestMatch.HasValue || res.Value.Distance < bestMatch.Value.Distance)
                            bestMatch = res.Value;
                }
            }

            return bestMatch;
        }


        public RaycastResult? Project(Vector3 worldPosition)
        {
            RaycastResult? bestMatch = null;
            lock (_windowsCollectionLock)
            {
                foreach (var window in _windows)
                {
                    var res = window.Project(worldPosition);
                    if (res.HasValue)
                        if (!bestMatch.HasValue || res.Value.Distance < bestMatch.Value.Distance)
                            bestMatch = res.Value;
                }
            }

            return bestMatch;
        }

        public void EnsureInvokeOnMainThread(Action action)
        {
            _mainThreadActions.Enqueue(action);
        }

        public void ProcessMainThreadQueue()
        {
            while (_mainThreadActions.TryDequeue(out var action)) action();
        }

        public TouchAdapter CreateTouchAdapter()
        {
            return new TouchAdapter(this, ++_touchCounter, _modifiers);
        }

        public virtual void Dispose()
        {
            Pipeline.Dispose();
            TextureResourceLayout.Dispose();
        }

        internal void UpdateView(List<VeldridTopLevelImpl> view)
        {
            lock (_windowsCollectionLock)
            {
                view.Clear();
                if (view.Capacity < _windows.Count)
                    view.Capacity = _windows.Count;
                foreach (var window in _windows) view.Add(window);
            }
        }

        internal void AddWindow(VeldridTopLevelImpl window)
        {
            lock (_windowsCollectionLock)
            {
                _windows.Add(window);
            }
        }

        internal void RemoveWindow(VeldridTopLevelImpl window)
        {
            lock (_windowsCollectionLock)
            {
                _windows.Remove(window);
            }
        }

        private Sampler DefaultSamplerFactory(GraphicsDevice graphicsDevice)
        {
            return graphicsDevice.Aniso4xSampler;
            //return graphicsDevice.PointSampler;
        }

        private void OnDeviceCreated()
        {
            var factory = GraphicsDevice.ResourceFactory;
            var thisClass = GetType();
            _shaders = ShaderHelper.LoadShader(GraphicsDevice, factory, thisClass.Assembly,
                thisClass.Namespace + ".Shaders");
            SpecializationConstant[] specConstants =
            {
                //new SpecializationConstant(0, gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
            };
            TextureResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WindowUniforms", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Input", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("InputSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            Pipeline = factory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleOverrideBlend,
                    DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerStateDescription.CullNone,
                    PrimitiveTopology.TriangleStrip,
                    new ShaderSetDescription(new VertexLayoutDescription[] { }, _shaders, specConstants),
                    new[] {TextureResourceLayout},
                    _outputDescription ?? GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription
                ));

            Sampler = _samplerFactory(GraphicsDevice);

            DeviceCreated?.Invoke(GraphicsDevice);
        }

        private void OnDeviceDestroyed()
        {
            TextureResourceLayout?.Dispose();
            TextureResourceLayout = null;
            Pipeline?.Dispose();
            Pipeline = null;
            if (_shaders != null)
                foreach (var shader in _shaders)
                    shader.Dispose();
            _shaders = null;
            DeviceDestroyed?.Invoke();
        }
    }
}