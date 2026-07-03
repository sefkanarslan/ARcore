using System.Collections.Generic;
using ArSpacePlanner.Core;
using ArSpacePlanner.Input;
using ArSpacePlanner.Util;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArSpacePlanner.Measure
{
    /// <summary>
    /// The measuring tool. Each tap raycasts against detected AR planes, drops an
    /// anchored point, and chains it to the previous one to build a poly-line. It can
    /// close the chain into a polygon to report enclosed area and perimeter.
    /// </summary>
    public sealed class MeasurementManager : MonoBehaviour
    {
        private static readonly Color LineColor = new Color(0.15f, 0.85f, 1f);
        private static readonly Color MarkerColor = new Color(1f, 0.85f, 0.2f);

        private ARRaycastManager _raycastManager;
        private Camera _camera;
        private AppState _state;

        private readonly List<Transform> _points = new List<Transform>();
        private readonly List<MeasureSegment> _segments = new List<MeasureSegment>();
        private readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private bool _areaClosed;

        public bool Active { get; set; }

        public int PointCount => _points.Count;
        public bool AreaClosed => _areaClosed;

        /// <summary>Total measured length along the current chain, metres.</summary>
        public float TotalLength => Geometry.PolylineLength(Positions());

        /// <summary>Enclosed area once the chain is closed into a polygon, m\u00B2 (else 0).</summary>
        public float Area => _areaClosed ? Geometry.PolygonArea(Positions()) : 0f;

        private List<Vector3> Positions()
        {
            var positions = new List<Vector3>(_points.Count);
            foreach (Transform t in _points)
            {
                if (t != null)
                {
                    positions.Add(t.position);
                }
            }
            return positions;
        }

        public void Init(ARRaycastManager raycastManager, Camera camera, AppState state)
        {
            _raycastManager = raycastManager;
            _camera = camera;
            _state = state;
        }

        private void Update()
        {
            if (!Active)
            {
                return;
            }

            if (InputRouter.TryGetTap(out Vector2 screenPos))
            {
                TryPlaceAt(screenPos);
            }
        }

        private void TryPlaceAt(Vector2 screenPos)
        {
            if (_raycastManager == null ||
                !_raycastManager.Raycast(screenPos, _hits, TrackableType.PlaneWithinPolygon))
            {
                _state.SetStatus("Y\u00fczey bulunamad\u0131 \u2014 telefonu yava\u015f\u00e7a gezdirip zemini/duvar\u0131 tarat.");
                return;
            }

            if (_areaClosed)
            {
                ClearInternal();
            }

            Pose pose = _hits[0].pose;
            Transform point = CreateAnchorMarker(pose);
            _points.Add(point);

            if (_points.Count >= 2)
            {
                AddSegment(_points[_points.Count - 2], point);
            }

            ReportRunning();
        }

        private Transform CreateAnchorMarker(Pose pose)
        {
            var anchorGo = new GameObject("MeasurePoint");
            anchorGo.transform.SetPositionAndRotation(pose.position, pose.rotation);
            anchorGo.AddComponent<ARAnchor>();

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Marker";
            Destroy(marker.GetComponent<Collider>());
            marker.transform.SetParent(anchorGo.transform, false);
            marker.transform.localScale = Vector3.one * 0.02f;
            marker.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Unlit(MarkerColor);

            return anchorGo.transform;
        }

        private void AddSegment(Transform a, Transform b)
        {
            var segGo = new GameObject("Segment");
            var segment = segGo.AddComponent<MeasureSegment>();
            segment.Init(a, b, _camera, _state, LineColor);
            _segments.Add(segment);
        }

        /// <summary>Close the current poly-line into a polygon and report area.</summary>
        public void CloseArea()
        {
            if (_points.Count < 3)
            {
                _state.SetStatus("Alan i\u00e7in en az 3 nokta gerekli.");
                return;
            }

            if (_areaClosed)
            {
                return;
            }

            AddSegment(_points[_points.Count - 1], _points[0]);
            _areaClosed = true;

            var positions = new List<Vector3>(_points.Count);
            foreach (Transform t in _points)
            {
                positions.Add(t.position);
            }

            float area = Geometry.PolygonArea(positions);
            float perimeter = Geometry.PolygonPerimeter(positions);
            _state.SetStatus(
                $"Alan: {Units.FormatArea(area, _state.Unit)}  \u2022  \u00c7evre: {Units.FormatLength(perimeter, _state.Unit)}");
        }

        public void Undo()
        {
            if (_areaClosed)
            {
                // Removing the closing edge first turns the polygon back into a chain.
                RemoveLastSegment();
                _areaClosed = false;
                ReportRunning();
                return;
            }

            if (_points.Count == 0)
            {
                return;
            }

            RemoveLastSegment();

            Transform last = _points[_points.Count - 1];
            _points.RemoveAt(_points.Count - 1);
            if (last != null)
            {
                Destroy(last.gameObject);
            }

            ReportRunning();
        }

        public void Clear()
        {
            ClearInternal();
            _state.SetStatus("\u00d6l\u00e7\u00fcm temizlendi. Noktalar\u0131 se\u00e7mek i\u00e7in y\u00fczeye dokun.");
        }

        private void RemoveLastSegment()
        {
            if (_segments.Count == 0)
            {
                return;
            }

            MeasureSegment seg = _segments[_segments.Count - 1];
            _segments.RemoveAt(_segments.Count - 1);
            if (seg != null)
            {
                Destroy(seg.gameObject);
            }
        }

        private void ClearInternal()
        {
            foreach (MeasureSegment seg in _segments)
            {
                if (seg != null)
                {
                    Destroy(seg.gameObject);
                }
            }
            _segments.Clear();

            foreach (Transform point in _points)
            {
                if (point != null)
                {
                    Destroy(point.gameObject);
                }
            }
            _points.Clear();

            _areaClosed = false;
        }

        private void ReportRunning()
        {
            if (_points.Count == 0)
            {
                _state.SetStatus("Bir noktaya ba\u015flamak i\u00e7in y\u00fczeye dokun.");
                return;
            }

            if (_points.Count == 1)
            {
                _state.SetStatus("\u0130lk nokta kondu \u2014 ikinci noktaya dokun.");
                return;
            }

            var positions = new List<Vector3>(_points.Count);
            foreach (Transform t in _points)
            {
                positions.Add(t.position);
            }

            float total = Geometry.PolylineLength(positions);
            _state.SetStatus($"Toplam: {Units.FormatLength(total, _state.Unit)}  ({_points.Count} nokta)");
        }
    }
}
