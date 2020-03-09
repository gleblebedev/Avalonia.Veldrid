using Avalonia.Input;

namespace Avalonia.Veldrid
{
    public class InputModifiersContainer
    {
        private RawInputModifiers _inputModifiers = RawInputModifiers.None;

        public void Set(RawInputModifiers flag)
        {
            _inputModifiers |= flag;
        }

        public void Drop(RawInputModifiers flag)
        {
            _inputModifiers &= ~flag;
        }
        public RawInputModifiers Modifiers => _inputModifiers;
    }
}