using System;
using System.Diagnostics;
using System.Numerics;
using Avalonia.Input;
using Avalonia.Input.Raw;

namespace Avalonia.Veldrid
{
    public class TouchAdapter :InputAdapterBase, IDisposable
    {
        private readonly AvaloniaVeldridContext _context;
        private readonly int _touchPointId;
        private readonly InputModifiersContainer _inputModifiers;
        private RaycastResult _lastProjectionResult;

        public TouchAdapter(AvaloniaVeldridContext context, int touchPointId, InputModifiersContainer inputModifiers)
        {
            _context = context;
            _touchPointId = touchPointId;
            _inputModifiers = inputModifiers;
        }

        public VeldridTopLevelImpl ActiveWindow => _lastProjectionResult.WindowImpl;
        public Vector3 HitPoint => _lastProjectionResult.WorldSpaceHitPoint;

        public void Move(Vector3 worldPosition)
        {
            bool useRaycast = false;
            RaycastResult? res;
            if (useRaycast)
            {
                var clipSpace = Vector4.Transform(worldPosition.ToPositionVec4(), _context.View * _context.Projection);
                res = _context.Raycast(new ClipSpaceRay()
                {
                    From = clipSpace,
                    To = clipSpace + Vector4.UnitZ
                });
            }
            else
            {
                res = _context.Project(worldPosition);
            }
            if (res.HasValue && res.Value.Distance > 0.03f)
            {
                res = null;
            }
            if (!res.HasValue)
            {
                RaiseEvent(RawPointerEventType.TouchEnd);
                _lastProjectionResult.WindowImpl = null;
                return;
            }

            var raycastResult = res.Value;
            if (raycastResult.WindowImpl != _lastProjectionResult.WindowImpl)
            {
                RaiseEvent(RawPointerEventType.TouchCancel);
                _lastProjectionResult = raycastResult;
                RaiseEvent(RawPointerEventType.TouchBegin);
                RaiseEvent(RawPointerEventType.Move);
            }
            else
            {
                _lastProjectionResult = raycastResult;
                RaiseEvent(RawPointerEventType.Move);
            }
        }

        public void Dispose()
        {
            
        }
        private void RaiseEvent(RawPointerEventType rawPointerEventType)
        {
            var windowImpl = _lastProjectionResult.WindowImpl;
            if (windowImpl != null)
            {
                windowImpl.Input?.Invoke(CreateEventArgs(rawPointerEventType));
            }
        }
        private RawTouchEventArgs CreateEventArgs(RawPointerEventType type)
        {
            return new RawTouchEventArgs(_lastProjectionResult.WindowImpl.TouchDevice, GetTimestamp(), _lastProjectionResult.WindowImpl.InputRoot, type, _lastProjectionResult.WindowPoint, _inputModifiers.Modifiers, _touchPointId);
        }
    }
}