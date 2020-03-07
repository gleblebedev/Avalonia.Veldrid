using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;

namespace Avalonia.Veldrid
{
    public static class VeldridApplicationExtensions
    {
        /// <summary>
        ///     Enable Skia renderer.
        /// </summary>
        /// <typeparam name="T">Builder type.</typeparam>
        /// <param name="builder">Builder.</param>
        /// <returns>Configure builder.</returns>
        public static T UsePortablePlatfrom<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            return builder.UseWindowingSubsystem(
                () => PortableWindowPlatform.Initialize(
                    AvaloniaLocator.Current.GetService<AvaloniaVeldridContext>() ??
                    new AvaloniaVeldridContext(null)),
                "PortableVeldridPlatform");
            
        }
    }
}
