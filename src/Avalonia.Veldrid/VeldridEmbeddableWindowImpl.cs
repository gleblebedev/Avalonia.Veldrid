using System;
using Avalonia.Platform;

namespace Avalonia.Veldrid
{
    public class VeldridEmbeddableWindowImpl : VeldridTopLevelImpl, IEmbeddableWindowImpl
    {
        public VeldridEmbeddableWindowImpl(AvaloniaVeldridContext veldridContext) : base(veldridContext)
        {
        }

        public event Action LostFocus;
    }
}