using ArSpacePlanner.Util;
using UnityEngine;

namespace ArSpacePlanner.Planner
{
    /// <summary>
    /// A furniture instance placed in the scene. The root sits on the floor plane; a
    /// child box mesh, sized to the definition, is offset up so the item rests on the
    /// ground. Handles selection/overlap highlighting and box-overlap testing.
    /// </summary>
    public sealed class PlacedFurniture : MonoBehaviour
    {
        private const float MinScale = 0.5f;
        private const float MaxScale = 2.0f;

        private FurnitureDefinition _definition;
        private Transform _box;
        private MeshRenderer _renderer;
        private Material _material;

        private float _scaleFactor = 1f;
        private bool _selected;

        public string DefinitionId => _definition != null ? _definition.Id : null;
        public float RotationY => transform.eulerAngles.y;
        public float ScaleFactor => _scaleFactor;

        public void Init(FurnitureDefinition definition)
        {
            _definition = definition;

            var boxGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxGo.name = "Volume";
            _box = boxGo.transform;
            _box.SetParent(transform, false);

            _renderer = boxGo.GetComponent<MeshRenderer>();
            _material = MaterialFactory.Transparent(WithAlpha(definition.Color, 0.6f));
            _renderer.sharedMaterial = _material;

            ApplyTransform();
        }

        public void SetRotationY(float degrees)
        {
            transform.rotation = Quaternion.Euler(0f, degrees, 0f);
        }

        public void AddRotationY(float deltaDegrees)
        {
            transform.Rotate(Vector3.up, deltaDegrees, Space.World);
        }

        public void SetScale(float factor)
        {
            _scaleFactor = Mathf.Clamp(factor, MinScale, MaxScale);
            ApplyTransform();
        }

        public void MultiplyScale(float multiplier)
        {
            SetScale(_scaleFactor * multiplier);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            RefreshColor();
        }

        /// <summary>True if this item's volume overlaps any other placed furniture.</summary>
        public bool Overlaps()
        {
            Vector3 halfExtents = (_definition.Size * _scaleFactor) * 0.5f;
            Vector3 center = transform.position + transform.up * halfExtents.y;
            Collider[] hits = Physics.OverlapBox(center, halfExtents * 0.98f, transform.rotation);

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
            if (_material == null)
            {
                return;
            }

            Color baseColor = _definition.Color;
            if (Overlaps())
            {
                baseColor = Color.Lerp(baseColor, Color.red, 0.6f);
            }

            float alpha = _selected ? 0.85f : 0.55f;
            _material.color = WithAlpha(baseColor, alpha);
        }

        private void ApplyTransform()
        {
            Vector3 size = _definition.Size * _scaleFactor;
            _box.localScale = size;
            _box.localPosition = new Vector3(0f, size.y * 0.5f, 0f);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
