using System;
using System.Numerics;
using Avalonia.Input.Raw;

namespace Avalonia.Veldrid
{
    public class TouchAdapter : InputAdapterBase, IDisposable
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

        /// <summary>
        ///     Move touch as a ray in 3D space.
        /// </summary>
        /// <param name="from">Origin of the ray.</param>
        /// <param name="to">Target of the ray (not direction!).</param>
        public void Move(Vector3 from, Vector3 to)
        {
            var matrix = _context.View * _context.Projection;
            var ray = new ClipSpaceRay(from, to, matrix);
            Move(_context.Raycast(ray));
        }

        /// <summary>
        ///     Move touch as a point in space with a certain tolerance distance.
        /// </summary>
        /// <param name="worldPosition">Position of the touch (tip of a finger) in world space.</param>
        /// <param name="newTouchToleranceInMeters">Distance to window at which a new touch is registered.</param>
        /// <param name="folowUpTouchToleranceInMeters">Distance to window at which an existing touch is still registered.</param>
        public void Move(Vector3 worldPosition, float newTouchToleranceInMeters = 0.03f, float folowUpTouchToleranceInMeters = 0.06f)
        {
            var useRaycast = false;
            RaycastResult? res;
            if (useRaycast)
            {
                var clipSpace = Vector4.Transform(worldPosition.ToPositionVec4(), _context.View * _context.Projection);
                Move(_context.Raycast(new ClipSpaceRay
                {
                    From = clipSpace,
                    To = clipSpace + Vector4.UnitZ
                }));
            }
            else
            {
                res = _context.Project(worldPosition);
                if (res.HasValue)
                {
                    var tolerance = (res.Value.WindowImpl == _lastProjectionResult.WindowImpl)
                        ? folowUpTouchToleranceInMeters
                        : newTouchToleranceInMeters;
                    if (res.Value.Distance > tolerance) res = null;
                }
                Move(res);
            }
        }

        public void Dispose()
        {
            if (ActiveWindow != null)
            {
                RaiseEvent(RawPointerEventType.TouchEnd);
                _lastProjectionResult.WindowImpl = null;
            }
        }

        private void Move(RaycastResult? res)
        {
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
                RaiseEvent(RawPointerEventType.TouchUpdate);
            }
        }

        private void RaiseEvent(RawPointerEventType rawPointerEventType)
        {
            var input = _lastProjectionResult.WindowImpl?.Input;
            if (input != null)
            {
                var args = CreateEventArgs(rawPointerEventType);
                _context.EnsureInvokeOnMainThread(() =>
                {
                    input(args);
                });
            }
        }

        private RawTouchEventArgs CreateEventArgs(RawPointerEventType type)
        {
            return new RawTouchEventArgs(_lastProjectionResult.WindowImpl.TouchDevice, GetTimestamp(),
                _lastProjectionResult.WindowImpl.InputRoot, type, _lastProjectionResult.WindowPoint,
                _inputModifiers.Modifiers, _touchPointId);
        }
    }
}