using System;

namespace Avalonia.Veldrid
{
    public class InputAdapterBase
    {
        protected static ulong GetTimestamp()
        {
            return (ulong)DateTimeOffset.UtcNow.Ticks;
        }
    }
}