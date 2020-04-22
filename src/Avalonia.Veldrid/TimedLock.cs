using System;
using System.Diagnostics;
using System.Threading;

namespace Avalonia.Veldrid
{
    internal class TimedLock: IDisposable
    {
        private readonly object _gate;
        Stopwatch _stopwatch = new Stopwatch();

        private TimedLock(object gate)
        {
            _gate = gate;
        }
        private void Enter()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
            Monitor.Enter(_gate);
            _stopwatch.Stop();
            if (_stopwatch.Elapsed.TotalMilliseconds > 2)
            {
            }
        }

        void IDisposable.Dispose()
        {
            Monitor.Exit(_gate);
        }

        public static IDisposable Lock(object gate)
        {
            var l = new TimedLock(gate);
            l.Enter();
            return l;
        }
    }
}