using UnityEngine;

namespace ArSpacePlanner.Planner
{
    /// <summary>
    /// Describes one catalog item at real-world scale. Meshes are generated from the
    /// <see cref="size"/> box at runtime, so the app needs no imported 3D models.
    /// </summary>
    public sealed class FurnitureDefinition
    {
        public readonly string Id;
        public readonly string DisplayName;

        /// <summary>Real dimensions in metres: X = width, Y = height, Z = depth.</summary>
        public readonly Vector3 Size;
        public readonly Color Color;

        public FurnitureDefinition(string id, string displayName, Vector3 size, Color color)
        {
            Id = id;
            DisplayName = displayName;
            Size = size;
            Color = color;
        }
    }
}
