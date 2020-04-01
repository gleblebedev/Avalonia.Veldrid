using Avalonia.Input;

namespace Avalonia.Veldrid
{
    public class InputModifiersContainer
    {
        public RawInputModifiers Modifiers { get; private set; } = RawInputModifiers.None;

        public void Set(RawInputModifiers flag)
        {
            Modifiers |= flag;
        }

        public void Drop(RawInputModifiers flag)
        {
            Modifiers &= ~flag;
        }
    }
}