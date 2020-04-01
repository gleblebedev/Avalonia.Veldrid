using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Platform;

namespace Avalonia.Veldrid
{
    internal class StandardRuntimePlatform : IRuntimePlatform
    {
        public IDisposable StartSystemTimer(TimeSpan interval, Action tick)
        {
            return new Timer(_ => tick(), null, interval, interval);
        }

        public RuntimePlatformInfo GetRuntimeInfo()
        {
            return new RuntimePlatformInfo
            {
                IsCoreClr = false,
                IsDesktop = false,
                IsMobile = true,
                IsDotNetFramework = false,
                IsMono = true,
                IsUnix = false,
                OperatingSystem = OperatingSystemType.Unknown
            };
        }

        public IUnmanagedBlob AllocBlob(int size)
        {
            return new UnmanagedBlob(this, size);
        }


        private IntPtr Alloc(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        private void Free(IntPtr ptr, int len)
        {
            Marshal.FreeHGlobal(ptr);
        }

        private class UnmanagedBlob : IUnmanagedBlob
        {
            private readonly StandardRuntimePlatform _plat;
            private readonly object _lock = new object();
            private IntPtr _address;

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
                {
                    Backtraces.Add(_backtrace);
                }
#endif
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

            public void Dispose()
            {
#if DEBUG
                if (Thread.CurrentThread.ManagedThreadId == GCThread?.ManagedThreadId)
                    lock (_lock)
                    {
                        if (!IsDisposed)
                            Console.Error.WriteLine("Native blob disposal from finalizer thread\nBacktrace: "
                                                    + Environment.StackTrace
                                                    + "\n\nBlob created by " + _backtrace);
                    }
#endif
                DoDispose();
                GC.SuppressFinalize(this);
            }

            private void DoDispose()
            {
                lock (_lock)
                {
                    if (!IsDisposed)
                    {
#if DEBUG
                        lock (_btlock)
                        {
                            Backtraces.Remove(_backtrace);
                        }
#endif
                        _plat?.Free(_address, Size);
                        GC.RemoveMemoryPressure(Size);
                        IsDisposed = true;
                        _address = IntPtr.Zero;
                        Size = 0;
                    }
                }
            }
#if DEBUG
            private static readonly List<string> Backtraces = new List<string>();
            private static Thread GCThread;
            private readonly string _backtrace;
            private static readonly object _btlock = new object();

            private class GCThreadDetector
            {
                ~GCThreadDetector()
                {
                    GCThread = Thread.CurrentThread;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void Spawn()
            {
                new GCThreadDetector();
            }

            static UnmanagedBlob()
            {
                Spawn();
                GC.WaitForPendingFinalizers();
            }

#endif
        }
    }
}