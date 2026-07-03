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
            new FurnitureDefinition("table",    "Masa",     new Vector3(1.20f, 0.75f, 0.70f), new Color(0.80f, 0.52f, 0.25f)),
            new FurnitureDefinition("chair",    "Sandalye", new Vector3(0.45f, 0.90f, 0.45f), new Color(0.30f, 0.55f, 0.85f)),
            new FurnitureDefinition("sofa",     "Koltuk",   new Vector3(2.00f, 0.85f, 0.90f), new Color(0.55f, 0.35f, 0.70f)),
            new FurnitureDefinition("bed",      "Yatak",    new Vector3(1.60f, 0.55f, 2.00f), new Color(0.85f, 0.40f, 0.45f)),
            new FurnitureDefinition("wardrobe", "Dolap",    new Vector3(1.00f, 2.00f, 0.60f), new Color(0.45f, 0.60f, 0.40f)),
            new FurnitureDefinition("desk",     "\u00c7al\u0131\u015fma Masas\u0131", new Vector3(1.40f, 0.75f, 0.65f), new Color(0.35f, 0.70f, 0.75f)),
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
