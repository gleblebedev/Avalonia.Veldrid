using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Platform.Interop;

namespace Avalonia.Veldrid
{
    public class VeldridProperty
    {
        //public static readonly AvaloniaProperty<Matrix4x4> WorldTransformProperty = AvaloniaProperty.RegisterAttached<VeldridProperty, Window, Matrix4x4>("WorldTransform", Matrix4x4.Identity);

        public static bool TrySetWorldTransform(Window window, Matrix4x4 transform)
        {
            if (window?.PlatformImpl is VeldridTopLevelImpl impl)
            {
                impl.WorldTransform = transform;
                return true;
            }

            return false;
        }
        public static bool TryGetWorldTransform(Window window, out Matrix4x4 transform)
        {
            if (window?.PlatformImpl is VeldridTopLevelImpl impl)
            {
                transform = impl.WorldTransform;
                return true;
            }
            transform = Matrix4x4.Identity;
            return false;
        }
        public static bool TrySetTexelSize(Window window, float size)
        {
            if (window?.PlatformImpl is VeldridTopLevelImpl impl)
            {
                impl.TexelSize = size;
                return true;
            }

            return false;
        }
        public static bool TryGetTexelSize(Window window, out float size)
        {
            if (window?.PlatformImpl is VeldridTopLevelImpl impl)
            {
                size = impl.TexelSize;
                return true;
            }
            size = 1.0f;
            return false;
        }
        
    }
}