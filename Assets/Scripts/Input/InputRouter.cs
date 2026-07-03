using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace ArSpacePlanner.Input
{
    /// <summary>
    /// Thin wrapper over the Input System so the rest of the app does not care whether
    /// it runs on a phone (touch) or in the Editor (mouse). Reports "taps" for the
    /// measurement/placement tools and exposes raw active touches for gestures.
    /// </summary>
    public static class InputRouter
    {
        public static void Enable()
        {
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
        }

        /// <summary>Number of fingers currently on screen (0 in the Editor with a mouse).</summary>
        public static int TouchCount => ETouch.Touch.activeTouches.Count;

        public static ETouch.Touch GetTouch(int index) => ETouch.Touch.activeTouches[index];

        /// <summary>
        /// True on the frame a single-finger tap (or left mouse click) begins, when it
        /// is not on top of a UI element. Provides the screen position of the press.
        /// </summary>
        public static bool TryGetTap(out Vector2 screenPos)
        {
            screenPos = Vector2.zero;

            var touches = ETouch.Touch.activeTouches;
            if (touches.Count == 1)
            {
                ETouch.Touch touch = touches[0];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began && !IsOverUI(touch.screenPosition))
                {
                    screenPos = touch.screenPosition;
                    return true;
                }
                return false;
            }

            if (touches.Count == 0)
            {
                Mouse mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                {
                    Vector2 pos = mouse.position.ReadValue();
                    if (!IsOverUI(pos))
                    {
                        screenPos = pos;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Current primary pointer position (first touch or mouse).</summary>
        public static bool TryGetPointer(out Vector2 screenPos)
        {
            var touches = ETouch.Touch.activeTouches;
            if (touches.Count > 0)
            {
                screenPos = touches[0].screenPosition;
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                screenPos = mouse.position.ReadValue();
                return true;
            }

            screenPos = Vector2.zero;
            return false;
        }

        public static bool IsOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            var data = new PointerEventData(EventSystem.current) { position = screenPos };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            return results.Count > 0;
        }
    }
}
