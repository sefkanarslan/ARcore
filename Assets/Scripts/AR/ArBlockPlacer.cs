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
    /// colliders; if none is hit in Add mode, an AR plane raycast seeds the first
    /// block on a detected surface.
    /// </summary>
    public sealed class ArBlockPlacer : MonoBehaviour
    {
        private ARRaycastManager _raycastManager;
        private Camera _arCamera;
        private VoxelWorld _world;
        private AppState _state;

        private readonly List<ARRaycastHit> _hits = new();

        public void Init(ARRaycastManager raycastManager, Camera arCamera, VoxelWorld world, AppState state)
        {
            _raycastManager = raycastManager;
            _arCamera = arCamera;
            _world = world;
            _state = state;
        }

        private void Update()
        {
            if (!TryGetTap(out Vector2 screenPos))
            {
                return;
            }

            Ray ray = _arCamera.ScreenPointToRay(screenPos);
            float half = VoxelSettings.VoxelSize * 0.5f;

            if (Physics.Raycast(ray, out RaycastHit hit, 20f))
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

            // Nothing solid tapped: in Add mode, seed a block on an AR plane.
            if (_state.Mode == InteractionMode.Add &&
                _raycastManager != null &&
                _raycastManager.Raycast(screenPos, _hits, TrackableType.PlaneWithinPolygon))
            {
                Vector3 point = _hits[0].pose.position;
                _world.SetBlockAtWorld(point, _state.SelectedBlock);
            }
        }

        private bool TryGetTap(out Vector2 screenPos)
        {
            screenPos = default;

            Pointer pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
            {
                return false;
            }

            screenPos = pointer.position.ReadValue();

            // Ignore taps that land on UI (buttons, panels).
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            return true;
        }
    }
}
