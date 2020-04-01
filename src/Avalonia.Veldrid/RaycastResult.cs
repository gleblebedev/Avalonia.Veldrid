using System.Numerics;

namespace Avalonia.Veldrid
{
    public struct RaycastResult
    {
        public static RaycastResult Miss = new RaycastResult
        {
            WindowImpl = null, WindowPoint = new Point(0, 0), ClipSpaceDepth = float.MaxValue,
            Distance = float.MaxValue, WorldSpaceHitPoint = Vector3.Zero
        };

        public VeldridTopLevelImpl WindowImpl;
        public Point WindowPoint;
        public float ClipSpaceDepth;

        public Vector3 WorldSpaceHitPoint;

        // World space distance ray have to travel to hit the window surface.
        public float Distance;
    }
}