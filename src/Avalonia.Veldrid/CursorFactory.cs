using System;
using Avalonia.Input;
using Avalonia.Platform;

namespace Avalonia.Veldrid
{
    public class CursorFactory : IStandardCursorFactory
    {
        public IPlatformHandle GetCursor(StandardCursorType cursorType)
        {
            return new PlatformHandle(IntPtr.Zero, "ZeroCursor");
        }
    }
}