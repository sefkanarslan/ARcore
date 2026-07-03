using System.Collections.Generic;
using ArSpacePlanner.Util;
using UnityEngine;

namespace ArSpacePlanner.Planner
{
    /// <summary>
    /// A furniture instance placed in the scene. Its mesh is assembled from several
    /// boxes (see <see cref="FurnitureMeshBuilder"/>) with lit materials for real
    /// shading. A single bounds collider is used for picking and overlap tests; a
    /// yellow wireframe marks selection and a red overlay marks collisions.
    /// </summary>
    public sealed class PlacedFurniture : MonoBehaviour
    {
        private const float MinScale = 0.5f;
        private const float MaxScale = 2.0f;

        private FurnitureDefinition _definition;
        private Vector3 _boundsCenter;
        private Vector3 _boundsSize;

        private BoxCollider _collider;
        private GameObject _selectionBox;
        private Renderer _overlapBox;

        private float _scaleFactor = 1f;

        public string DefinitionId => _definition != null ? _definition.Id : null;
        public MountType Mount => _definition != null ? _definition.Mount : MountType.Floor;
        public float RotationY => transform.eulerAngles.y;
        public float ScaleFactor => _scaleFactor;

        public void Init(FurnitureDefinition definition)
        {
            _definition = definition;

            if (definition.Mount == MountType.Wall)
            {
                _boundsCenter = new Vector3(0f, 0f, definition.Size.z * 0.5f);
            }
            else
            {
                _boundsCenter = new Vector3(0f, definition.Size.y * 0.5f, 0f);
            }
            _boundsSize = definition.Size;

            BuildParts(definition);
            BuildBoundsCollider();
            BuildSelectionBox();
            BuildOverlapBox();

            ApplyScale();
        }

        private void BuildParts(FurnitureDefinition definition)
        {
            List<FurniturePart> parts = FurnitureMeshBuilder.Build(definition);
            foreach (FurniturePart part in parts)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Part";
                Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(transform, false);
                go.transform.localPosition = part.Center;
                go.transform.localScale = part.Size;
                go.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Lit(part.Color);
            }
        }

        private void BuildBoundsCollider()
        {
            _collider = gameObject.AddComponent<BoxCollider>();
            _collider.center = _boundsCenter;
            _collider.size = _boundsSize;
        }

        private void BuildSelectionBox()
        {
            _selectionBox = WireBox.Create(transform, _boundsCenter, _boundsSize, new Color(1f, 0.85f, 0.2f));
            _selectionBox.SetActive(false);
        }

        private void BuildOverlapBox()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "OverlapOverlay";
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(transform, false);
            go.transform.localPosition = _boundsCenter;
            go.transform.localScale = _boundsSize * 1.02f;
            _overlapBox = go.GetComponent<MeshRenderer>();
            _overlapBox.sharedMaterial = MaterialFactory.Transparent(new Color(1f, 0.15f, 0.15f, 0.35f));
            _overlapBox.enabled = false;
        }

        public void SetRotationY(float degrees)
        {
            transform.rotation = Quaternion.Euler(0f, degrees, 0f);
        }

        public void AddRotationY(float deltaDegrees)
        {
            transform.Rotate(Vector3.up, deltaDegrees, Space.World);
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void SetScale(float factor)
        {
            _scaleFactor = Mathf.Clamp(factor, MinScale, MaxScale);
            ApplyScale();
        }

        public void MultiplyScale(float multiplier)
        {
            SetScale(_scaleFactor * multiplier);
        }

        public void SetSelected(bool selected)
        {
            if (_selectionBox != null)
            {
                _selectionBox.SetActive(selected);
            }
        }

        /// <summary>True if this item's volume overlaps any other placed furniture.</summary>
        public bool Overlaps()
        {
            Vector3 halfExtents = (_boundsSize * _scaleFactor) * 0.5f;
            Vector3 center = transform.TransformPoint(_boundsCenter);
            Collider[] hits = Physics.OverlapBox(center, halfExtents * 0.95f, transform.rotation);

            foreach (Collider hit in hits)
            {
                var other = hit.GetComponentInParent<PlacedFurniture>();
                if (other != null && other != this)
                {
                    return true;
                }
            }
            return false;
        }

        public void RefreshColor()
        {
            if (_overlapBox != null)
            {
                _overlapBox.enabled = Overlaps();
            }
        }

        private void ApplyScale()
        {
            transform.localScale = Vector3.one * _scaleFactor;
        }
    }
}
