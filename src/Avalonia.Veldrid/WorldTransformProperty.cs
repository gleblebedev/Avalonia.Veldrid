using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Platform.Interop;

namespace Avalonia.Veldrid
{
    public class WorldTransformProperty
    {
        public static void SetValue(Window window, Matrix4x4 transform)
        {
            if (window?.PlatformImpl is VeldridTopLevelImpl impl) impl.WorldTransform = transform;
        }
    }
}