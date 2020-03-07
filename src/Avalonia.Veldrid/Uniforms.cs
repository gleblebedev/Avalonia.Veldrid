using System.Numerics;
using System.Runtime.InteropServices;

namespace Avalonia.Veldrid.Uniforms
{
    [StructLayout(LayoutKind.Explicit)]
    public struct WindowUniforms
    {
        [FieldOffset(0)] public Matrix4x4 Model;

        [FieldOffset(64)] public Matrix4x4 View;

        [FieldOffset(128)] public Matrix4x4 Projection;

        [FieldOffset(192)] public Vector2 Viewport;
    }
}