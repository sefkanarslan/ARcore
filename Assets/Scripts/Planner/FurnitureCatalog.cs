using System.Collections.Generic;
using UnityEngine;

namespace ArSpacePlanner.Planner
{
    /// <summary>
    /// Built-in catalog of furniture at realistic dimensions. Kept as plain code so the
    /// list is easy to extend without touching the editor.
    /// </summary>
    public static class FurnitureCatalog
    {
        public static readonly IReadOnlyList<FurnitureDefinition> Items = new List<FurnitureDefinition>
        {
            // Floor items.
            new FurnitureDefinition("table",    "Masa",     FurnitureKind.Table,    MountType.Floor, new Vector3(1.20f, 0.75f, 0.70f), new Color(0.62f, 0.42f, 0.24f)),
            new FurnitureDefinition("chair",    "Sandalye", FurnitureKind.Chair,    MountType.Floor, new Vector3(0.45f, 0.90f, 0.45f), new Color(0.30f, 0.50f, 0.78f)),
            new FurnitureDefinition("sofa",     "Koltuk",   FurnitureKind.Sofa,     MountType.Floor, new Vector3(2.00f, 0.85f, 0.90f), new Color(0.52f, 0.34f, 0.66f)),
            new FurnitureDefinition("bed",      "Yatak",    FurnitureKind.Bed,      MountType.Floor, new Vector3(1.60f, 0.55f, 2.00f), new Color(0.80f, 0.40f, 0.45f)),
            new FurnitureDefinition("wardrobe", "Dolap",    FurnitureKind.Wardrobe, MountType.Floor, new Vector3(1.00f, 2.00f, 0.60f), new Color(0.45f, 0.55f, 0.40f)),
            new FurnitureDefinition("desk",     "\u00c7al\u0131\u015fma Masas\u0131", FurnitureKind.Desk, MountType.Floor, new Vector3(1.40f, 0.75f, 0.65f), new Color(0.35f, 0.62f, 0.66f)),

            // Wall items.
            new FurnitureDefinition("tv",       "TV",       FurnitureKind.Tv,       MountType.Wall,  new Vector3(1.20f, 0.70f, 0.08f), new Color(0.10f, 0.10f, 0.12f)),
            new FurnitureDefinition("painting", "Tablo",    FurnitureKind.Painting, MountType.Wall,  new Vector3(0.70f, 0.50f, 0.04f), new Color(0.75f, 0.68f, 0.45f)),
            new FurnitureDefinition("shelf",    "Raf",      FurnitureKind.Shelf,    MountType.Wall,  new Vector3(0.90f, 0.30f, 0.25f), new Color(0.58f, 0.44f, 0.28f)),
        };

        public static FurnitureDefinition ById(string id)
        {
            foreach (FurnitureDefinition item in Items)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }

        public static int IndexOf(string id)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Id == id)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
