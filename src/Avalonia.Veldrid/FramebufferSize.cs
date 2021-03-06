﻿using System;
using System.Numerics;

namespace Avalonia.Veldrid
{
    public struct FramebufferSize : IEquatable<FramebufferSize>
    {
        public bool Equals(FramebufferSize other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is FramebufferSize other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Width * 397) ^ (int) Height;
            }
        }

        public static bool operator ==(FramebufferSize left, FramebufferSize right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FramebufferSize left, FramebufferSize right)
        {
            return !left.Equals(right);
        }

        public FramebufferSize(uint width, uint height)
        {
            Width = width;
            Height = height;
        }

        public FramebufferSize(int width, int height)
        {
            Width = (uint) width;
            Height = (uint) height;
        }

        public uint Width { get; }
        public uint Height { get; }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }

        public Vector2 ToVector2()
        {
            return new Vector2(Width, Height);
        }
    }
}