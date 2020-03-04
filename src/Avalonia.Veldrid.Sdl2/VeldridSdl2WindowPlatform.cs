using System;
using System.Threading;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Rendering;

namespace Avalonia.Veldrid.Sdl2
{
    public class VeldridSdl2WindowPlatform : PlatformThreadingInterfaceBase, IPlatformSettings, IWindowingPlatform
    {
        private static readonly VeldridSdl2WindowPlatform s_instance = new VeldridSdl2WindowPlatform();
        private Sdl2AvaloniaWindow _windowImpl;

        public VeldridSdl2WindowPlatform()
        {
            _windowImpl = new Sdl2AvaloniaWindow();
        }

        public static VeldridSdl2PlatformOptions Options { get; private set; }

        public Size DoubleClickSize { get; } = new Size(2, 2);
        public TimeSpan DoubleClickTime { get; } = TimeSpan.FromSeconds(0.5);

        public static void Initialize()
        {
            Initialize(new VeldridSdl2PlatformOptions());
        }

        public static void Initialize(VeldridSdl2PlatformOptions options)
        {
            Options = options;
            AvaloniaLocator.CurrentMutable
                .Bind<IPlatformSettings>().ToConstant(s_instance)
                .Bind<IStandardCursorFactory>().ToTransient<CursorFactory>()
                .Bind<IPlatformThreadingInterface>().ToConstant(s_instance)
                .Bind<IRenderLoop>().ToConstant(new RenderLoop())
                .Bind<IRenderTimer>().ToConstant(new DefaultRenderTimer(60))
                .Bind<IWindowingPlatform>().ToConstant(s_instance)
                .Bind<PlatformHotkeyConfiguration>().ToSingleton<PlatformHotkeyConfiguration>()
                .Bind<IPlatformIconLoader>().ToConstant(new PlatformIconLoader());
        }

        public override void EnsureInvokeOnMainThread(Action callback)
        {
            _windowImpl.EnsureInvokeOnMainThread(callback);
        }

        public override void RunLoop(CancellationToken cancellationToken)
        {
            _windowImpl.Run(cancellationToken);
        }

        public IWindowImpl CreateWindow()
        {
            if (_windowImpl == null) _windowImpl = new Sdl2AvaloniaWindow();
            return _windowImpl.CreateWindow();
        }

        public IEmbeddableWindowImpl CreateEmbeddableWindow()
        {
            throw new NotImplementedException();
        }
    }
}