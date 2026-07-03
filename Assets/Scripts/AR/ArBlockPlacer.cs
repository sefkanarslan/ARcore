using System.Collections.Generic;
using ArBlokEvren.Blocks;
using ArBlokEvren.Core;
using ArBlokEvren.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArBlokEvren.AR
{
    /// <summary>
    /// Handles tap input: add or break blocks. Taps first hit existing block
    /// colliders; if none is hit in Add mode, an AR plane raycast (AR mode) or a
    /// virtual ground plane (sandbox mode) seeds a block. A tap is only registered
    /// on release with little movement, so dragging to look around never edits.
    /// </summary>
    public sealed class ArBlockPlacer : MonoBehaviour
    {
        private const float TapMoveThreshold = 28f;
        private const float TapMaxDuration = 0.5f;

        private ARRaycastManager _raycastManager;
        private Camera _camera;
        private VoxelWorld _world;
        private AppState _state;

        private bool _seedOnGround;
        private float _groundY;

        private bool _pressed;
        private Vector2 _pressPos;
        private float _pressTime;

        private readonly List<ARRaycastHit> _hits = new();

        public void Init(ARRaycastManager raycastManager, Camera camera, VoxelWorld world, AppState state)
        {
            _raycastManager = raycastManager;
            _camera = camera;
            _world = world;
            _state = state;
            _seedOnGround = false;
        }

        public void InitDemo(Camera camera, VoxelWorld world, AppState state, float groundY)
        {
            _raycastManager = null;
            _camera = camera;
            _world = world;
            _state = state;
            _seedOnGround = true;
            _groundY = groundY;
        }

        private void Update()
        {
            if (!TryGetTap(out Vector2 screenPos))
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(screenPos);
            float half = VoxelSettings.VoxelSize * 0.5f;

            if (Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                if (_state.Mode == InteractionMode.Break)
                {
                    Vector3 inside = hit.point - hit.normal * half;
                    _world.SetBlock(Coords.WorldToBlock(inside), BlockType.Air);
                }
                else
                {
                    Vector3 outside = hit.point + hit.normal * half;
                    _world.SetBlock(Coords.WorldToBlock(outside), _state.SelectedBlock);
                }

                return;
            }

            if (_state.Mode != InteractionMode.Add)
            {
                return;
            }

            // Nothing solid tapped: seed a block on a surface.
            if (_raycastManager != null &&
                _raycastManager.Raycast(screenPos, _hits, TrackableType.PlaneWithinPolygon))
            {
                _world.SetBlockAtWorld(_hits[0].pose.position, _state.SelectedBlock);
            }
            else if (_seedOnGround && TryHitGround(ray, out Vector3 groundPoint))
            {
                _world.SetBlockAtWorld(groundPoint, _state.SelectedBlock);
            }
        }

        private bool TryHitGround(Ray ray, out Vector3 point)
        {
            point = default;
            if (ray.direction.y > -1e-4f)
            {
                return false;
            }

            float t = (_groundY - ray.origin.y) / ray.direction.y;
            if (t <= 0f)
            {
                return false;
            }

            point = ray.origin + ray.direction * t;
            point.y += VoxelSettings.VoxelSize * 0.5f;
            return true;
        }

        private bool TryGetTap(out Vector2 screenPos)
        {
            screenPos = default;

            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return false;
            }

            if (pointer.press.wasPressedThisFrame)
            {
                _pressed = true;
                _pressPos = pointer.position.ReadValue();
                _pressTime = Time.unscaledTime;
            }

            if (!pointer.press.wasReleasedThisFrame || !_pressed)
            {
                return false;
            }

            _pressed = false;
            Vector2 releasePos = pointer.position.ReadValue();

            if ((releasePos - _pressPos).magnitude > TapMoveThreshold)
            {
                return false;
            }

            if (Time.unscaledTime - _pressTime > TapMaxDuration)
            {
                return false;
            }

            // Ignore taps that land on UI (buttons, panels).
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            screenPos = releasePos;
            return true;
        }
    }
}
