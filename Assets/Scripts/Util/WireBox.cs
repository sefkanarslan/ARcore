using UnityEngine;

namespace ArSpacePlanner.Util
{
    /// <summary>
    /// Builds a box wireframe (12 edges) as line renderers in local space, used to
    /// outline a selected object. Because the lines are parented and use local space,
    /// they follow the owner's transform, including scale.
    /// </summary>
    public static class WireBox
    {
        public static GameObject Create(Transform parent, Vector3 center, Vector3 size, Color color)
        {
            var root = new GameObject("WireBox");
            root.transform.SetParent(parent, false);

            Vector3 h = size * 0.5f;
            Vector3[] c =
            {
                center + new Vector3(-h.x, -h.y, -h.z),
                center + new Vector3(h.x, -h.y, -h.z),
                center + new Vector3(h.x, -h.y, h.z),
                center + new Vector3(-h.x, -h.y, h.z),
                center + new Vector3(-h.x, h.y, -h.z),
                center + new Vector3(h.x, h.y, -h.z),
                center + new Vector3(h.x, h.y, h.z),
                center + new Vector3(-h.x, h.y, h.z),
            };

            int[,] edges =
            {
                { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 }, // bottom
                { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 }, // top
                { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }  // verticals
            };

            Material material = MaterialFactory.Unlit(color);
            for (int i = 0; i < edges.GetLength(0); i++)
            {
                var edgeGo = new GameObject("Edge", typeof(LineRenderer));
                edgeGo.transform.SetParent(root.transform, false);
                var line = edgeGo.GetComponent<LineRenderer>();
                line.useWorldSpace = false;
                line.widthMultiplier = 0.006f;
                line.positionCount = 2;
                line.sharedMaterial = material;
                line.SetPosition(0, c[edges[i, 0]]);
                line.SetPosition(1, c[edges[i, 1]]);
            }

            return root;
        }
    }
}
