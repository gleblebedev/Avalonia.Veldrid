using System;
using System.Threading;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Skia;

namespace Avalonia.Veldrid
{
    public class PortableWindowPlatform : PlatformThreadingInterfaceBase, IPlatformSettings, IWindowingPlatform
    {
        private static readonly PortableWindowPlatform s_instance = new PortableWindowPlatform();
        private static AvaloniaVeldridContext _context;

        public Size DoubleClickSize { get; } = new Size(2, 2);
        public TimeSpan DoubleClickTime { get; } = TimeSpan.FromSeconds(0.5);

        public static void Initialize(AvaloniaVeldridContext context)
        {
            _context = context;
            AvaloniaLocator.CurrentMutable
                .Bind<IPlatformSettings>().ToConstant(s_instance)
                .Bind<IStandardCursorFactory>().ToTransient<CursorFactory>()
                .Bind<IPlatformThreadingInterface>().ToConstant(s_instance)
                .Bind<IRenderLoop>().ToConstant(new RenderLoop())
                .Bind<IRenderTimer>().ToConstant(new DefaultRenderTimer(60))
                .Bind<IWindowingPlatform>().ToConstant(s_instance)
                .Bind<PlatformHotkeyConfiguration>().ToSingleton<PlatformHotkeyConfiguration>()
                .Bind<IPlatformIconLoader>().ToConstant(new PlatformIconLoader());
            SkiaPlatform.Initialize();
        }

        public override void EnsureInvokeOnMainThread(Action action)
        {
            _context.EnsureInvokeOnMainThread(action);
        }

        public override void RunLoop(CancellationToken cancellationToken)
        {
            _context.RunLoop(cancellationToken);
        }

        public IWindowImpl CreateWindow()
        {
            return new VeldridWindowImpl(_context);
        }

        public IEmbeddableWindowImpl CreateEmbeddableWindow()
        {
            return new VeldridEmbeddableWindowImpl(_context);
        }
    }
}