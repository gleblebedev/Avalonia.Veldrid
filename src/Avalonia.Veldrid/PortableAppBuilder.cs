using Avalonia.Controls;

namespace Avalonia.Veldrid
{
    public sealed class PortableAppBuilder : AppBuilderBase<PortableAppBuilder>
    {
        public PortableAppBuilder() : base(new StandardRuntimePlatform(),
            builder => StandardRuntimePlatformServices.Register(builder.Instance?.GetType()?.Assembly))
        {
        }

        public void UseContext(AvaloniaVeldridContext veldridContext)
        {
        }
    }
}