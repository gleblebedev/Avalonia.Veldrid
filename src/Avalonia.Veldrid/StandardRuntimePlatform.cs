using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Platform;

namespace Avalonia.Veldrid
{
    internal partial class StandardRuntimePlatform : IRuntimePlatform
    {
        public IDisposable StartSystemTimer(TimeSpan interval, Action tick)
        {
            return new Timer(_ => tick(), null, interval, interval);
        }

        public RuntimePlatformInfo GetRuntimeInfo() => new RuntimePlatformInfo
        {
            IsCoreClr = false,
            IsDesktop = false,
            IsMobile = true,
            IsDotNetFramework = false,
            IsMono = true,
            IsUnix = false,
            OperatingSystem = OperatingSystemType.Unknown
        };

        public IUnmanagedBlob AllocBlob(int size) => new UnmanagedBlob(this, size);

        class UnmanagedBlob : IUnmanagedBlob
        {
            private readonly StandardRuntimePlatform _plat;
            private IntPtr _address;
            private readonly object _lock = new object();
#if DEBUG
            private static readonly List<string> Backtraces = new List<string>();
            private static Thread GCThread;
            private readonly string _backtrace;
            private static readonly object _btlock = new object();

            class GCThreadDetector
            {
                ~GCThreadDetector()
                {
                    GCThread = Thread.CurrentThread;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void Spawn() => new GCThreadDetector();

            static UnmanagedBlob()
            {
                Spawn();
                GC.WaitForPendingFinalizers();
            }

#endif

            public UnmanagedBlob(StandardRuntimePlatform plat, int size)
            {
                if (size <= 0)
                    throw new ArgumentException("Positive number required", nameof(size));
                _plat = plat;
                _address = plat.Alloc(size);
                GC.AddMemoryPressure(size);
                Size = size;
#if DEBUG
                _backtrace = Environment.StackTrace;
                lock (_btlock)
                    Backtraces.Add(_backtrace);
#endif
            }

            void DoDispose()
            {
                lock (_lock)
                {
                    if (!IsDisposed)
                    {
#if DEBUG
                        lock (_btlock)
                            Backtraces.Remove(_backtrace);
#endif
                        _plat?.Free(_address, Size);
                        GC.RemoveMemoryPressure(Size);
                        IsDisposed = true;
                        _address = IntPtr.Zero;
                        Size = 0;
                    }
                }
            }

            public void Dispose()
            {
#if DEBUG
                if (Thread.CurrentThread.ManagedThreadId == GCThread?.ManagedThreadId)
                {
                    lock (_lock)
                        if (!IsDisposed)
                            Console.Error.WriteLine("Native blob disposal from finalizer thread\nBacktrace: "
                                                    + Environment.StackTrace
                                                    + "\n\nBlob created by " + _backtrace);
                }
#endif
                DoDispose();
                GC.SuppressFinalize(this);
            }

            ~UnmanagedBlob()
            {
#if DEBUG
                Console.Error.WriteLine("Undisposed native blob created by " + _backtrace);
#endif
                DoDispose();
            }

            public IntPtr Address => IsDisposed ? throw new ObjectDisposedException("UnmanagedBlob") : _address;
            public int Size { get; private set; }
            public bool IsDisposed { get; private set; }
        }


        IntPtr Alloc(int size) => Marshal.AllocHGlobal(size);
        void Free(IntPtr ptr, int len) => Marshal.FreeHGlobal(ptr);
    }
}