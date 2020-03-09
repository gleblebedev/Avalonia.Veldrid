using Avalonia.Input;

namespace Avalonia.Veldrid
{
    public class KeyboardAdapter
    {
        private readonly AvaloniaVeldridContext _context;
        private readonly InputModifiersContainer _inputModifiers;

        internal KeyboardAdapter(AvaloniaVeldridContext context, InputModifiersContainer inputModifiers)
        {
            _context = context;
            _inputModifiers = inputModifiers ?? new InputModifiersContainer();
        }

        public void OnKeyDown(Key key)
        {
            switch (key)
            {
            }
        }

        public void OnKeyUp(Key key)
        {
            switch (key)
            {
            }
        }
    }
}