using ArBlokEvren.Blocks;
using UnityEngine;

namespace ArBlokEvren.Themes
{
    /// <summary>
    /// Builds a procedural texture atlas for a theme and exposes per-face UV rects.
    /// Tiles are painted from the theme palette; no external image files are used.
    /// </summary>
    public sealed class TextureAtlas
    {
        private const int BaseTileCount = (int)BlockType.Count;
        private const int GrassTop = BaseTileCount + 0;
        private const int GrassSide = BaseTileCount + 1;
        private const int WoodTop = BaseTileCount + 2;
        private const int TotalTiles = BaseTileCount + 3;

        private const int TilesPerRow = 4;
        private const int TileSize = 32;
        private const int AtlasSize = TilesPerRow * TileSize;
        private const float Inset = 0.5f;

        public Texture2D Texture { get; }

        public TextureAtlas(ThemeDefinition theme)
        {
            Texture = new Texture2D(AtlasSize, AtlasSize, TextureFormat.RGBA32, false)
            {
                name = $"Atlas_{theme.Id}",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = theme.Style == ThemeStyle.Smooth ? FilterMode.Bilinear : FilterMode.Point
            };

            for (int t = 0; t < TotalTiles; t++)
            {
                PaintTile(t, theme);
            }

            Texture.Apply(false);
        }

        public Rect GetUv(BlockType type, FaceDir dir)
        {
            int tile = TileIndex(type, dir);
            int col = tile % TilesPerRow;
            int row = tile / TilesPerRow;
            float uMin = (col * TileSize + Inset) / AtlasSize;
            float vMin = (row * TileSize + Inset) / AtlasSize;
            float uMax = ((col + 1) * TileSize - Inset) / AtlasSize;
            float vMax = ((row + 1) * TileSize - Inset) / AtlasSize;
            return new Rect(uMin, vMin, uMax - uMin, vMax - vMin);
        }

        private static int TileIndex(BlockType type, FaceDir dir)
        {
            switch (type)
            {
                case BlockType.Grass:
                    if (dir == FaceDir.Up) return GrassTop;
                    if (dir == FaceDir.Down) return (int)BlockType.Dirt;
                    return GrassSide;
                case BlockType.Wood:
                    return (dir == FaceDir.Up || dir == FaceDir.Down) ? WoodTop : (int)BlockType.Wood;
                default:
                    return (int)type;
            }
        }

        private void PaintTile(int tile, ThemeDefinition theme)
        {
            int col = tile % TilesPerRow;
            int row = tile / TilesPerRow;
            int ox = col * TileSize;
            int oy = row * TileSize;

            var rng = new System.Random(theme.Id.GetHashCode() ^ (tile * 92821));

            for (int y = 0; y < TileSize; y++)
            {
                for (int x = 0; x < TileSize; x++)
                {
                    Color c = PixelColor(tile, x, y, theme, rng);
                    Texture.SetPixel(ox + x, oy + y, c);
                }
            }
        }

        private Color PixelColor(int tile, int x, int y, ThemeDefinition theme, System.Random rng)
        {
            Color baseColor = TileBaseColor(tile, x, y, theme);

            switch (theme.Style)
            {
                case ThemeStyle.Pixel:
                {
                    float jitter = (float)(rng.NextDouble() * 0.22 - 0.11);
                    if (rng.NextDouble() < 0.06)
                    {
                        jitter -= 0.18f;
                    }

                    return Shade(baseColor, jitter);
                }
                case ThemeStyle.Comic:
                {
                    bool border = x == 0 || y == 0 || x == TileSize - 1 || y == TileSize - 1;
                    if (border)
                    {
                        return Shade(baseColor, -0.55f);
                    }

                    // Small highlight cell in the upper-left for a hand-drawn feel.
                    if (x > 3 && x < 12 && y > TileSize - 12 && y < TileSize - 4)
                    {
                        return Shade(baseColor, 0.22f);
                    }

                    return baseColor;
                }
                default: // Smooth
                {
                    float g = y / (float)(TileSize - 1);
                    return Shade(baseColor, Mathf.Lerp(0.15f, -0.15f, g));
                }
            }
        }

        private static Color TileBaseColor(int tile, int x, int y, ThemeDefinition theme)
        {
            if (tile == GrassSide)
            {
                // Top strip = grass, rest = dirt.
                return y >= TileSize - 8 ? theme.BaseColor(BlockType.Grass) : theme.BaseColor(BlockType.Dirt);
            }

            if (tile == GrassTop)
            {
                return theme.BaseColor(BlockType.Grass);
            }

            if (tile == WoodTop)
            {
                return Shade(theme.BaseColor(BlockType.Wood), 0.12f);
            }

            return theme.BaseColor((BlockType)tile);
        }

        private static Color Shade(Color c, float amount)
        {
            if (amount >= 0f)
            {
                return new Color(
                    Mathf.Lerp(c.r, 1f, amount),
                    Mathf.Lerp(c.g, 1f, amount),
                    Mathf.Lerp(c.b, 1f, amount),
                    c.a);
            }

            float k = 1f + amount;
            return new Color(c.r * k, c.g * k, c.b * k, c.a);
        }
    }
}
