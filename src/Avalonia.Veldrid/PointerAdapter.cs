using System.Numerics;
using Avalonia.Input;
using Avalonia.Input.Raw;

namespace Avalonia.Veldrid
{
    public class PointerAdapter : InputAdapterBase
    {
        private readonly AvaloniaVeldridContext _context;
        private readonly InputModifiersContainer _inputModifiers;
        private RaycastResult _lastRaycastResult;

        internal PointerAdapter(AvaloniaVeldridContext context, InputModifiersContainer inputModifiers)
        {
            _inputModifiers = inputModifiers ?? new InputModifiersContainer();
            _context = context;
        }

        public void OnButtonUp(MouseButton mouseButton)
        {
            var eventType = RawPointerEventType.Move;

            switch (mouseButton)
            {
                case MouseButton.Left:
                    _inputModifiers.Drop(RawInputModifiers.LeftMouseButton);
                    eventType = RawPointerEventType.LeftButtonUp;
                    break;
                case MouseButton.Right:
                    _inputModifiers.Drop(RawInputModifiers.RightMouseButton);
                    eventType = RawPointerEventType.RightButtonUp;
                    break;
                case MouseButton.Middle:
                    _inputModifiers.Drop(RawInputModifiers.MiddleMouseButton);
                    eventType = RawPointerEventType.MiddleButtonUp;
                    break;
            }

            RaiseEvent(eventType);
        }

        public void OnButtonDown(MouseButton mouseButton)
        {
            var eventType = RawPointerEventType.Move;

            switch (mouseButton)
            {
                case MouseButton.Left:
                    _inputModifiers.Set(RawInputModifiers.LeftMouseButton);
                    eventType = RawPointerEventType.LeftButtonDown;
                    break;
                case MouseButton.Right:
                    _inputModifiers.Set(RawInputModifiers.RightMouseButton);
                    eventType = RawPointerEventType.RightButtonDown;
                    break;
                case MouseButton.Middle:
                    _inputModifiers.Set(RawInputModifiers.MiddleMouseButton);
                    eventType = RawPointerEventType.MiddleButtonDown;
                    break;
            }

            RaiseEvent(eventType);
        }

        public void OnEntered()
        {
        }

        public void OnLeft()
        {
            RaiseEvent(RawPointerEventType.LeaveWindow);
        }

        public void OnMove(Vector2 position)
        {
            var res = _context.Raycast(new ClipSpaceRay(position.ToPoint(), _context.ScreenSize.ToSize(),
                _context.GraphicsDevice.IsDepthRangeZeroToOne, _context.GraphicsDevice.IsClipSpaceYInverted));
            if (!res.HasValue)
                return;

            var raycastResult = res.Value;
            if (raycastResult.WindowImpl != _lastRaycastResult.WindowImpl)
            {
                RaiseEvent(RawPointerEventType.LeaveWindow);
            }
            _lastRaycastResult = raycastResult;
            RaiseEvent(RawPointerEventType.Move);
        }

        private void RaiseEvent(RawPointerEventType rawPointerEventType)
        {
            var input = _lastRaycastResult.WindowImpl?.Input;
            if (input != null)
            {
                var args = CreatePointerEventArgs(rawPointerEventType);
                _context.EnsureInvokeOnMainThread(() => input(args));
            }
        }

        private RawPointerEventArgs CreatePointerEventArgs(RawPointerEventType type)
        {
            return new RawPointerEventArgs(_lastRaycastResult.WindowImpl.MouseDevice, GetTimestamp(),
                _lastRaycastResult.WindowImpl.InputRoot, type, _lastRaycastResult.WindowPoint,
                _inputModifiers.Modifiers);
        }
    }
}