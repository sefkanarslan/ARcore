using System.Collections.Generic;
using UnityEngine;

namespace ArSpacePlanner.Planner
{
    /// <summary>One box that makes up a piece of furniture, in the item's local space.</summary>
    public struct FurniturePart
    {
        public Vector3 Center;
        public Vector3 Size;
        public Color Color;

        public FurniturePart(Vector3 center, Vector3 size, Color color)
        {
            Center = center;
            Size = size;
            Color = color;
        }
    }

    /// <summary>
    /// Builds a recognizable piece of furniture out of a handful of boxes instead of a
    /// single block, so items read as tables/chairs/sofas in AR. Everything is procedural.
    ///
    /// Local frame conventions:
    ///  - Floor items: origin at the bottom-centre (y = 0 at the floor).
    ///  - Wall items:  origin at the back-centre (z = 0 on the wall), extending in +z.
    /// </summary>
    public static class FurnitureMeshBuilder
    {
        public static List<FurniturePart> Build(FurnitureDefinition def)
        {
            Vector3 s = def.Size;
            Color body = def.Color;
            Color dark = Color.Lerp(body, Color.black, 0.35f);
            Color light = Color.Lerp(body, Color.white, 0.30f);

            var parts = new List<FurniturePart>();
            switch (def.Kind)
            {
                case FurnitureKind.Table:
                    AddTopAndLegs(parts, s, body, dark, legInset: 0.05f, legThickness: 0.06f);
                    break;

                case FurnitureKind.Desk:
                    parts.Add(new FurniturePart(new Vector3(0f, s.y - 0.03f, 0f), new Vector3(s.x, 0.06f, s.z), body));
                    parts.Add(new FurniturePart(new Vector3(s.x * 0.5f - 0.2f, (s.y - 0.06f) * 0.5f, 0f),
                        new Vector3(0.4f, s.y - 0.06f, s.z - 0.05f), dark));
                    AddLeg(parts, -(s.x * 0.5f - 0.04f), (s.z * 0.5f - 0.04f), s.y - 0.06f, 0.05f, dark);
                    AddLeg(parts, -(s.x * 0.5f - 0.04f), -(s.z * 0.5f - 0.04f), s.y - 0.06f, 0.05f, dark);
                    break;

                case FurnitureKind.Chair:
                {
                    float seatH = s.y * 0.5f;
                    parts.Add(new FurniturePart(new Vector3(0f, seatH, 0f), new Vector3(s.x, 0.06f, s.z), body));
                    parts.Add(new FurniturePart(new Vector3(0f, seatH + (s.y - seatH) * 0.5f, -s.z * 0.5f + 0.03f),
                        new Vector3(s.x, s.y - seatH, 0.06f), body));
                    AddFourLegs(parts, s.x, s.z, seatH, inset: 0.04f, thickness: 0.05f, color: dark);
                    break;
                }

                case FurnitureKind.Sofa:
                    parts.Add(new FurniturePart(new Vector3(0f, 0.20f, 0f), new Vector3(s.x, 0.40f, s.z), body));
                    parts.Add(new FurniturePart(new Vector3(0f, 0.55f, -s.z * 0.5f + 0.12f), new Vector3(s.x, 0.55f, 0.24f), body));
                    parts.Add(new FurniturePart(new Vector3(s.x * 0.5f - 0.10f, 0.42f, 0f), new Vector3(0.20f, 0.55f, s.z), light));
                    parts.Add(new FurniturePart(new Vector3(-(s.x * 0.5f - 0.10f), 0.42f, 0f), new Vector3(0.20f, 0.55f, s.z), light));
                    parts.Add(new FurniturePart(new Vector3(0f, 0.45f, 0.06f), new Vector3(s.x - 0.44f, 0.12f, s.z - 0.32f), light));
                    break;

                case FurnitureKind.Bed:
                    parts.Add(new FurniturePart(new Vector3(0f, 0.12f, 0f), new Vector3(s.x, 0.24f, s.z), dark));
                    parts.Add(new FurniturePart(new Vector3(0f, 0.38f, 0.05f), new Vector3(s.x - 0.10f, 0.22f, s.z - 0.10f), light));
                    parts.Add(new FurniturePart(new Vector3(0f, 0.52f, -s.z * 0.5f + 0.28f), new Vector3(s.x - 0.5f, 0.10f, 0.35f),
                        Color.Lerp(body, Color.white, 0.6f)));
                    break;

                case FurnitureKind.Wardrobe:
                    parts.Add(new FurniturePart(new Vector3(0f, s.y * 0.5f, 0f), new Vector3(s.x, s.y, s.z), body));
                    parts.Add(new FurniturePart(new Vector3(s.x * 0.25f, s.y * 0.5f, s.z * 0.5f - 0.005f),
                        new Vector3(s.x * 0.5f - 0.02f, s.y - 0.06f, 0.02f), dark));
                    parts.Add(new FurniturePart(new Vector3(-s.x * 0.25f, s.y * 0.5f, s.z * 0.5f - 0.005f),
                        new Vector3(s.x * 0.5f - 0.02f, s.y - 0.06f, 0.02f), dark));
                    parts.Add(new FurniturePart(new Vector3(0.03f, s.y * 0.5f, s.z * 0.5f + 0.01f), new Vector3(0.03f, 0.20f, 0.03f), light));
                    parts.Add(new FurniturePart(new Vector3(-0.03f, s.y * 0.5f, s.z * 0.5f + 0.01f), new Vector3(0.03f, 0.20f, 0.03f), light));
                    break;

                case FurnitureKind.Tv:
                    parts.Add(new FurniturePart(new Vector3(0f, 0f, 0.025f), new Vector3(s.x, s.y, 0.05f), dark));
                    parts.Add(new FurniturePart(new Vector3(0f, 0f, 0.055f), new Vector3(s.x - 0.06f, s.y - 0.06f, 0.01f),
                        new Color(0.15f, 0.25f, 0.4f)));
                    break;

                case FurnitureKind.Painting:
                    parts.Add(new FurniturePart(new Vector3(0f, 0f, 0.015f), new Vector3(s.x, s.y, 0.03f), body));
                    parts.Add(new FurniturePart(new Vector3(0f, 0f, 0.035f), new Vector3(s.x - 0.08f, s.y - 0.08f, 0.01f), light));
                    break;

                case FurnitureKind.Shelf:
                    parts.Add(new FurniturePart(new Vector3(0f, s.y * 0.5f - 0.02f, s.z * 0.5f), new Vector3(s.x, 0.03f, s.z), body));
                    parts.Add(new FurniturePart(new Vector3(0f, -s.y * 0.5f + 0.02f, s.z * 0.5f), new Vector3(s.x, 0.03f, s.z), body));
                    parts.Add(new FurniturePart(new Vector3(0f, 0f, 0.02f), new Vector3(s.x, s.y, 0.02f), dark));
                    parts.Add(new FurniturePart(new Vector3(s.x * 0.5f - 0.015f, 0f, s.z * 0.5f), new Vector3(0.03f, s.y, s.z), body));
                    parts.Add(new FurniturePart(new Vector3(-s.x * 0.5f + 0.015f, 0f, s.z * 0.5f), new Vector3(0.03f, s.y, s.z), body));
                    break;

                default:
                    parts.Add(new FurniturePart(new Vector3(0f, s.y * 0.5f, 0f), s, body));
                    break;
            }

            return parts;
        }

        private static void AddTopAndLegs(List<FurniturePart> parts, Vector3 s, Color top, Color legColor, float legInset, float legThickness)
        {
            parts.Add(new FurniturePart(new Vector3(0f, s.y - 0.03f, 0f), new Vector3(s.x, 0.06f, s.z), top));
            AddFourLegs(parts, s.x, s.z, s.y - 0.06f, legInset, legThickness, legColor);
        }

        private static void AddFourLegs(List<FurniturePart> parts, float width, float depth, float legHeight, float inset, float thickness, Color color)
        {
            float x = width * 0.5f - inset;
            float z = depth * 0.5f - inset;
            AddLeg(parts, x, z, legHeight, thickness, color);
            AddLeg(parts, x, -z, legHeight, thickness, color);
            AddLeg(parts, -x, z, legHeight, thickness, color);
            AddLeg(parts, -x, -z, legHeight, thickness, color);
        }

        private static void AddLeg(List<FurniturePart> parts, float x, float z, float legHeight, float thickness, Color color)
        {
            parts.Add(new FurniturePart(new Vector3(x, legHeight * 0.5f, z), new Vector3(thickness, legHeight, thickness), color));
        }
    }
}
