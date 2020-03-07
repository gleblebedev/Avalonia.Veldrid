using System.Numerics;

namespace Avalonia.Veldrid
{
    public struct ClipSpaceRay
    {
        public ClipSpaceRay(Point point, Size clientAreaSize, bool isDepthRangeZeroToOne, bool IsClipSpaceYInverted)
        {
            var x = (float) ((point.X / clientAreaSize.Width - 0.5) * 2.0);
            float y;
            if (IsClipSpaceYInverted)
                y = (float) ((-point.Y / clientAreaSize.Height + 0.5) * 2.0);
            else
                y = (float) ((point.Y / clientAreaSize.Height - 0.5) * 2.0);
            From = new Vector4(x, y, isDepthRangeZeroToOne ? 0 : -1, 1);
            To = new Vector4(x, y, 1, 1);
        }

        public Vector4 From;
        public Vector4 To;
    }
}