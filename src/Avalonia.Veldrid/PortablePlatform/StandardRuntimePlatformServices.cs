using System.Reflection;
using Avalonia.Platform;
using Avalonia.Platform.Interop;

namespace Avalonia.Veldrid
{
    internal static class StandardRuntimePlatformServices
    {
        public static void Register(Assembly assembly = null)
        {
            var standardPlatform = new StandardRuntimePlatform();
            AssetLoader.RegisterResUriParsers();
            AvaloniaLocator.CurrentMutable
                .Bind<IRuntimePlatform>().ToConstant(standardPlatform)
                .Bind<IAssetLoader>().ToConstant(new AssetLoader(assembly))
                .Bind<IDynamicLibraryLoader>().ToConstant(new DynamicLibraryLoader());
        }
    }
}