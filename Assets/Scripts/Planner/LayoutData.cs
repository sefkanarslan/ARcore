using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArSpacePlanner.Planner
{
    /// <summary>Serializable snapshot of one placed item.</summary>
    [Serializable]
    public struct FurnitureEntry
    {
        public string id;
        public Vector3 position;
        public float rotationY;
        public float scale;
    }

    /// <summary>Serializable snapshot of a full furniture arrangement.</summary>
    [Serializable]
    public sealed class LayoutData
    {
        public List<FurnitureEntry> items = new List<FurnitureEntry>();
    }
}
