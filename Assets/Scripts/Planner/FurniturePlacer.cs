using System.Collections.Generic;
using ArSpacePlanner.Core;
using ArSpacePlanner.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace ArSpacePlanner.Planner
{
    /// <summary>
    /// The space-planning tool. Places catalog furniture onto detected planes and lets
    /// the user select an item, drag it along the floor with one finger, and pinch /
    /// twist with two fingers to scale and rotate. Overlapping items are highlighted in
    /// red. Arrangements can be saved to and restored from disk.
    /// </summary>
    public sealed class FurniturePlacer : MonoBehaviour
    {
        private const float TapMoveThreshold = 20f; // pixels

        private ARRaycastManager _raycastManager;
        private Camera _camera;
        private AppState _state;

        private readonly List<PlacedFurniture> _placed = new List<PlacedFurniture>();
        private readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private PlacedFurniture _selected;
        private PlacedFurniture _dragging;
        private Vector2 _pressStart;
        private bool _pressWasOnItem;

        private float _prevPinchDistance;
        private float _prevTwistAngle;
        private bool _gestureActive;

        public bool Active { get; set; }

        public int SelectedCatalogIndex { get; private set; }

        public void Init(ARRaycastManager raycastManager, Camera camera, AppState state)
        {
            _raycastManager = raycastManager;
            _camera = camera;
            _state = state;
        }

        public void SetActiveTool(bool active)
        {
            Active = active;
            SetVisible(active);
            if (!active)
            {
                Select(null);
            }
        }

        public void SelectCatalog(int index)
        {
            SelectedCatalogIndex = Mathf.Clamp(index, 0, FurnitureCatalog.Items.Count - 1);
            FurnitureDefinition def = FurnitureCatalog.Items[SelectedCatalogIndex];
            _state.SetStatus($"Se\u00e7ili: {def.DisplayName} \u2014 yerle\u015ftirmek i\u00e7in zemine dokun.");
        }

        private void Update()
        {
            if (!Active)
            {
                return;
            }

            var touches = ETouch.Touch.activeTouches;
            if (touches.Count >= 2)
            {
                HandlePinch(touches[0], touches[1]);
                return;
            }

            _gestureActive = false;

            if (touches.Count == 1)
            {
                HandleSingleTouch(touches[0]);
            }
            else
            {
                HandleMouse();
            }
        }

        // ---- one finger: tap = place/select, drag = move ----------------------

        private void HandleSingleTouch(ETouch.Touch touch)
        {
            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    BeginPress(touch.screenPosition);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
                case UnityEngine.InputSystem.TouchPhase.Stationary:
                    ContinuePress(touch.screenPosition);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Ended:
                    EndPress(touch.screenPosition);
                    break;
            }
        }

        private void HandleMouse()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            Vector2 pos = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame)
            {
                BeginPress(pos);
            }
            else if (mouse.leftButton.isPressed)
            {
                ContinuePress(pos);
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                EndPress(pos);
            }
        }

        private void BeginPress(Vector2 screenPos)
        {
            if (InputRouter.IsOverUI(screenPos))
            {
                _dragging = null;
                _pressWasOnItem = false;
                return;
            }

            _pressStart = screenPos;
            PlacedFurniture hit = PickFurniture(screenPos);
            _pressWasOnItem = hit != null;

            if (hit != null)
            {
                Select(hit);
                _dragging = hit;
            }
            else
            {
                _dragging = null;
            }
        }

        private void ContinuePress(Vector2 screenPos)
        {
            if (_dragging == null)
            {
                return;
            }

            if (Vector2.Distance(screenPos, _pressStart) < TapMoveThreshold)
            {
                return;
            }

            if (TryRaycastToWorld(screenPos, out Pose pose))
            {
                _dragging.transform.position = pose.position;
                _dragging.RefreshColor();
            }
        }

        private void EndPress(Vector2 screenPos)
        {
            bool wasTap = Vector2.Distance(screenPos, _pressStart) < TapMoveThreshold;
            _dragging = null;

            if (!wasTap || _pressWasOnItem)
            {
                RefreshAllColors();
                return;
            }

            // Tap on empty space -> place a new item.
            if (TryRaycastToWorld(screenPos, out Pose pose))
            {
                Place(pose.position);
            }
            else
            {
                _state.SetStatus("Y\u00fczey bulunamad\u0131 \u2014 zemini biraz daha tarat.");
            }
        }

        // ---- two fingers: scale + rotate selected -----------------------------

        private void HandlePinch(ETouch.Touch a, ETouch.Touch b)
        {
            if (_selected == null)
            {
                return;
            }

            float distance = Vector2.Distance(a.screenPosition, b.screenPosition);
            Vector2 delta = b.screenPosition - a.screenPosition;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            if (!_gestureActive)
            {
                _gestureActive = true;
                _prevPinchDistance = distance;
                _prevTwistAngle = angle;
                return;
            }

            if (_prevPinchDistance > 0.01f)
            {
                _selected.MultiplyScale(distance / _prevPinchDistance);
            }

            float deltaAngle = Mathf.DeltaAngle(_prevTwistAngle, angle);
            _selected.AddRotationY(-deltaAngle);

            _prevPinchDistance = distance;
            _prevTwistAngle = angle;
            _selected.RefreshColor();
        }

        // ---- placement / selection helpers ------------------------------------

        public PlacedFurniture Place(Vector3 position)
        {
            FurnitureDefinition def = FurnitureCatalog.Items[SelectedCatalogIndex];
            return Spawn(def, position, 0f, 1f, select: true);
        }

        private PlacedFurniture Spawn(FurnitureDefinition def, Vector3 position, float rotationY, float scale, bool select)
        {
            var go = new GameObject("Furniture_" + def.Id);
            go.transform.position = position;
            var item = go.AddComponent<PlacedFurniture>();
            item.Init(def);
            item.SetRotationY(rotationY);
            item.SetScale(scale);
            go.SetActive(Active || !select);

            _placed.Add(item);
            if (select)
            {
                Select(item);
            }
            RefreshAllColors();
            _state.SetStatus($"{def.DisplayName} yerle\u015ftirildi. S\u00fcr\u00fckle: ta\u015f\u0131, iki parmak: d\u00f6nd\u00fcr/\u00f6l\u00e7ekle.");
            return item;
        }

        private void Select(PlacedFurniture item)
        {
            if (_selected == item)
            {
                if (item != null)
                {
                    item.SetSelected(true);
                }
                return;
            }

            if (_selected != null)
            {
                _selected.SetSelected(false);
            }
            _selected = item;
            if (_selected != null)
            {
                _selected.SetSelected(true);
            }
        }

        private PlacedFurniture PickFurniture(Vector2 screenPos)
        {
            Ray ray = _camera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 30f))
            {
                return hit.collider.GetComponentInParent<PlacedFurniture>();
            }
            return null;
        }

        /// <summary>AR-plane raycast, falling back to a y=0 ground plane off-device.</summary>
        private bool TryRaycastToWorld(Vector2 screenPos, out Pose pose)
        {
            if (_raycastManager != null &&
                _raycastManager.Raycast(screenPos, _hits, TrackableType.PlaneWithinPolygon))
            {
                pose = _hits[0].pose;
                return true;
            }

            Ray ray = _camera.ScreenPointToRay(screenPos);
            var ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float enter))
            {
                pose = new Pose(ray.GetPoint(enter), Quaternion.identity);
                return true;
            }

            pose = default;
            return false;
        }

        private void RefreshAllColors()
        {
            foreach (PlacedFurniture item in _placed)
            {
                if (item != null)
                {
                    item.RefreshColor();
                }
            }
        }

        private void SetVisible(bool visible)
        {
            foreach (PlacedFurniture item in _placed)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(visible);
                }
            }
        }

        // ---- commands used by the UI ------------------------------------------

        public void DeleteSelected()
        {
            if (_selected == null)
            {
                _state.SetStatus("Silmek i\u00e7in \u00f6nce bir e\u015fyaya dokun.");
                return;
            }

            _placed.Remove(_selected);
            Destroy(_selected.gameObject);
            _selected = null;
            RefreshAllColors();
            _state.SetStatus("E\u015fya silindi.");
        }

        public void ClearAll()
        {
            foreach (PlacedFurniture item in _placed)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _placed.Clear();
            _selected = null;
            _state.SetStatus("T\u00fcm e\u015fyalar kald\u0131r\u0131ld\u0131.");
        }

        public void Save()
        {
            var data = new LayoutData();
            foreach (PlacedFurniture item in _placed)
            {
                if (item == null)
                {
                    continue;
                }
                data.items.Add(new FurnitureEntry
                {
                    id = item.DefinitionId,
                    position = item.transform.position,
                    rotationY = item.RotationY,
                    scale = item.ScaleFactor
                });
            }

            LayoutStore.Save(data);
            _state.SetStatus($"Plan kaydedildi ({data.items.Count} e\u015fya).");
        }

        public void Load()
        {
            LayoutData data = LayoutStore.Load();
            if (data.items.Count == 0)
            {
                _state.SetStatus("Kay\u0131tl\u0131 plan bulunamad\u0131.");
                return;
            }

            ClearAll();
            foreach (FurnitureEntry entry in data.items)
            {
                FurnitureDefinition def = FurnitureCatalog.ById(entry.id);
                if (def == null)
                {
                    continue;
                }
                Spawn(def, entry.position, entry.rotationY, entry.scale, select: false);
            }

            RefreshAllColors();
            _state.SetStatus($"Plan y\u00fcklendi ({data.items.Count} e\u015fya).");
        }
    }
}
