using System;
using System.Diagnostics;
using Avalonia.Input;

namespace Avalonia.Veldrid.UnitTests
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var key in Enum.GetValues(typeof(Key)))
            {
                Debug.WriteLine($"                case global::Veldrid.Key.{key}: return Key.{key};");
            }
        }
    }
}
