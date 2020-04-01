using System.Numerics;

namespace Avalonia.Veldrid
{
    public static class ExtensionMethods
    {
        public static Point ToPoint(this Vector2 position)
        {
            return new Point(position.X, position.Y);
        }

        public static Size ToSize(this FramebufferSize size)
        {
            return new Size(size.Width, size.Height);
        }

        internal static Vector4 ToPositionVec4(this Vector3 pos)
        {
            return new Vector4(pos.X, pos.Y, pos.Z, 1.0f);
        }

        internal static Vector4 ToDirectionVec4(this Vector3 pos)
        {
            return new Vector4(pos.X, pos.Y, pos.Z, 0.0f);
        }

        internal static Vector3 ToPositionVec3(this Vector4 pos)
        {
            return new Vector3(pos.X, pos.Y, pos.Z) * (1.0f / pos.W);
        }

        internal static Vector3 ToDirectionVec3(this Vector4 pos)
        {
            return new Vector3(pos.X, pos.Y, pos.Z);
        }
    }
}