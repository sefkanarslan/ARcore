using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace ArBlokEvren.Demo
{
    /// <summary>
    /// Orbit camera for the non-AR sandbox: one-finger drag rotates, two-finger
    /// pinch (or mouse wheel in the editor) zooms. A clean single tap is left for
    /// the block placer, so dragging to look around never places a block.
    /// </summary>
    public sealed class DemoCameraController : MonoBehaviour
    {
        public Vector3 Target;
        public float Distance = 2.4f;
        public float MinDistance = 0.6f;
        public float MaxDistance = 6f;

        private float _yaw = 35f;
        private float _pitch = 28f;
        private float _lastPinch = -1f;

        public void Configure(Vector3 target, float distance)
        {
            Target = target;
            Distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
            Apply();
        }

        private void Update()
        {
            if (ActiveTouchCount() >= 2)
            {
                HandlePinch();
            }
            else
            {
                _lastPinch = -1f;
                HandleOrbit();
            }

            HandleScrollZoom();
            Apply();
        }

        private void HandleOrbit()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null || !pointer.press.isPressed)
            {
                return;
            }

            if (IsOverUi())
            {
                return;
            }

            Vector2 delta = pointer.delta.ReadValue();
            _yaw += delta.x * 0.2f;
            _pitch = Mathf.Clamp(_pitch - delta.y * 0.2f, 5f, 85f);
        }

        private void HandlePinch()
        {
            Touchscreen ts = Touchscreen.current;
            if (ts == null)
            {
                return;
            }

            Vector2 a = ts.touches[0].position.ReadValue();
            Vector2 b = ts.touches[1].position.ReadValue();
            float dist = Vector2.Distance(a, b);

            if (_lastPinch > 0f)
            {
                Distance = Mathf.Clamp(Distance - (dist - _lastPinch) * 0.01f, MinDistance, MaxDistance);
            }

            _lastPinch = dist;
        }

        private void HandleScrollZoom()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Distance = Mathf.Clamp(Distance - scroll * 0.0025f, MinDistance, MaxDistance);
            }
        }

        private void Apply()
        {
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 pos = Target - rot * Vector3.forward * Distance;
            transform.SetPositionAndRotation(pos, rot);
        }

        private static int ActiveTouchCount()
        {
            Touchscreen ts = Touchscreen.current;
            if (ts == null)
            {
                return 0;
            }

            int count = 0;
            foreach (TouchControl touch in ts.touches)
            {
                if (touch.press.isPressed)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
