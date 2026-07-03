using System.Collections.Generic;
using UnityEngine;

namespace ArSpacePlanner.Util
{
    /// <summary>Geometry helpers used by the measurement tools.</summary>
    public static class Geometry
    {
        /// <summary>Total length of an open poly-line (sum of segment lengths), metres.</summary>
        public static float PolylineLength(IReadOnlyList<Vector3> points)
        {
            float total = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                total += Vector3.Distance(points[i - 1], points[i]);
            }
            return total;
        }

        /// <summary>
        /// Area of a closed polygon whose vertices are (approximately) coplanar in 3D.
        /// Uses the Newell/cross-product method, which is robust for planar polygons in
        /// any orientation and does not require projecting to 2D. Result in m\u00B2.
        /// </summary>
        public static float PolygonArea(IReadOnlyList<Vector3> points)
        {
            if (points.Count < 3)
            {
                return 0f;
            }

            Vector3 cross = Vector3.zero;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 current = points[i];
                Vector3 next = points[(i + 1) % points.Count];
                cross += Vector3.Cross(current, next);
            }

            return Mathf.Abs(cross.magnitude) * 0.5f;
        }

        /// <summary>Perimeter of a closed polygon, metres.</summary>
        public static float PolygonPerimeter(IReadOnlyList<Vector3> points)
        {
            if (points.Count < 2)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 current = points[i];
                Vector3 next = points[(i + 1) % points.Count];
                total += Vector3.Distance(current, next);
            }
            return total;
        }
    }
}
