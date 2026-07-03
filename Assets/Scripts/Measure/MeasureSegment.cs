using ArSpacePlanner.Core;
using ArSpacePlanner.Util;
using UnityEngine;

namespace ArSpacePlanner.Measure
{
    /// <summary>
    /// A single measured span between two anchored points: a line plus a floating 3D
    /// label that always shows the live distance in the user's chosen unit and faces
    /// the camera. It re-reads endpoint positions every frame so it stays correct even
    /// as ARCore refines the underlying anchors. Uses <see cref="TextMesh"/> so it needs
    /// no imported font/TMP assets.
    /// </summary>
    public sealed class MeasureSegment : MonoBehaviour
    {
        private Transform _a;
        private Transform _b;
        private Camera _camera;
        private AppState _state;

        private LineRenderer _line;
        private TextMesh _label;

        public void Init(Transform a, Transform b, Camera camera, AppState state, Color color)
        {
            _a = a;
            _b = b;
            _camera = camera;
            _state = state;

            _line = gameObject.AddComponent<LineRenderer>();
            _line.material = MaterialFactory.Unlit(color);
            _line.widthMultiplier = 0.008f;
            _line.positionCount = 2;
            _line.numCapVertices = 4;
            _line.useWorldSpace = true;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            _label = labelGo.AddComponent<TextMesh>();
            _label.font = UI.UIFactory.DefaultFont();
            _label.GetComponent<MeshRenderer>().sharedMaterial = _label.font.material;
            _label.fontSize = 90;
            _label.characterSize = 0.006f;
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.color = Color.white;
        }

        public float Length =>
            (_a != null && _b != null) ? Vector3.Distance(_a.position, _b.position) : 0f;

        private void LateUpdate()
        {
            if (_a == null || _b == null)
            {
                return;
            }

            _line.SetPosition(0, _a.position);
            _line.SetPosition(1, _b.position);

            Vector3 mid = (_a.position + _b.position) * 0.5f;
            _label.transform.position = mid + Vector3.up * 0.02f;
            if (_camera != null)
            {
                _label.transform.rotation = Quaternion.LookRotation(
                    _label.transform.position - _camera.transform.position, Vector3.up);
            }

            MeasureUnit unit = _state != null ? _state.Unit : MeasureUnit.Meters;
            _label.text = Units.FormatLength(Length, unit);
        }
    }
}
