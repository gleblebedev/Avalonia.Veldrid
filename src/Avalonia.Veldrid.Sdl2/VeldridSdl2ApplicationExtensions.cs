using Avalonia.Controls;
using Avalonia.Veldrid.Sdl2;

namespace Avalonia
{
    /// <summary>
    ///     Skia application extensions.
    /// </summary>
    public static class VeldridSdl2ApplicationExtensions
    {
        /// <summary>
        ///     Enable Skia renderer.
        /// </summary>
        /// <typeparam name="T">Builder type.</typeparam>
        /// <param name="builder">Builder.</param>
        /// <returns>Configure builder.</returns>
        public static T UseVeldridSdl2Window<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            return builder.UseWindowingSubsystem(
                () => VeldridSdl2WindowPlatform.Initialize(
                    AvaloniaLocator.Current.GetService<VeldridSdl2PlatformOptions>() ??
                    new VeldridSdl2PlatformOptions()),
                "Veldrid");
        }
    }

    public class VeldridSdl2PlatformOptions
    {
    }
}