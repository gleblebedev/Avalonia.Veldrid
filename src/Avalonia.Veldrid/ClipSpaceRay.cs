using System.Numerics;

namespace Avalonia.Veldrid
{
    public struct ClipSpaceRay
    {
        public ClipSpaceRay(Vector4 from, Vector4 to)
        {
            From = from;
            To = to;
        }

        public ClipSpaceRay(Vector3 from, Vector3 to, Matrix4x4 viewProjection)
        {
            From = Vector4.Transform(new Vector4(from.X, from.Y, from.Z, 1.0f), viewProjection);
            To = Vector4.Transform(new Vector4(to.X, to.Y, to.Z, 1.0f), viewProjection);
        }

        public ClipSpaceRay(Point point, Size clientAreaSize, bool isDepthRangeZeroToOne, bool IsClipSpaceYInverted)
        {
            var x = (float) ((point.X / clientAreaSize.Width - 0.5) * 2.0);
            float y;
            y = (float)((-point.Y / clientAreaSize.Height + 0.5) * 2.0);
            if (IsClipSpaceYInverted)
                y = -y;
            From = new Vector4(x, y, isDepthRangeZeroToOne ? 0 : -1, 1);
            To = new Vector4(x, y, 1, 1);
        }

        public Vector4 From;
        public Vector4 To;
    }
}