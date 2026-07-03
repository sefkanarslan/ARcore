using System.Collections.Generic;
using UnityEngine;

namespace ArBlokEvren.Themes
{
    /// <summary>
    /// The catalogue of available "universes". Add a new entry here to add a new
    /// theme; the UI theme selector is generated from this list automatically.
    /// Palette arrays are indexed by (int)BlockType.
    /// </summary>
    public static class ThemeLibrary
    {
        private static Color Rgb(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f, 1f);

        private static List<ThemeDefinition> _all;

        public static IReadOnlyList<ThemeDefinition> All
        {
            get
            {
                _all ??= Build();
                return _all;
            }
        }

        private static List<ThemeDefinition> Build()
        {
            return new List<ThemeDefinition>
            {
                // Index order: Air, Stone, Dirt, Grass, Wood, Leaves, Sand, Water, Metal, Glow
                new ThemeDefinition("minecraft", "Minecraft", ThemeStyle.Pixel, new[]
                {
                    Color.clear,
                    Rgb(127, 127, 127),
                    Rgb(134, 96, 67),
                    Rgb(95, 159, 53),
                    Rgb(160, 127, 81),
                    Rgb(56, 118, 29),
                    Rgb(219, 205, 158),
                    Rgb(64, 118, 220),
                    Rgb(160, 165, 172),
                    Rgb(255, 214, 90),
                }),
                new ThemeDefinition("marvel", "Marvel", ThemeStyle.Comic, new[]
                {
                    Color.clear,
                    Rgb(60, 66, 90),
                    Rgb(90, 30, 40),
                    Rgb(0, 132, 176),
                    Rgb(120, 70, 40),
                    Rgb(0, 160, 140),
                    Rgb(240, 190, 60),
                    Rgb(0, 180, 220),
                    Rgb(200, 30, 45),
                    Rgb(255, 205, 40),
                }),
                new ThemeDefinition("cartoon", "Cizgi Film", ThemeStyle.Comic, new[]
                {
                    Color.clear,
                    Rgb(150, 150, 170),
                    Rgb(180, 120, 70),
                    Rgb(120, 220, 90),
                    Rgb(200, 140, 80),
                    Rgb(70, 200, 120),
                    Rgb(250, 230, 150),
                    Rgb(90, 200, 250),
                    Rgb(200, 200, 210),
                    Rgb(255, 240, 120),
                }),
                new ThemeDefinition("animation", "Animasyon", ThemeStyle.Smooth, new[]
                {
                    Color.clear,
                    Rgb(190, 190, 205),
                    Rgb(210, 170, 150),
                    Rgb(170, 225, 170),
                    Rgb(215, 185, 160),
                    Rgb(160, 220, 190),
                    Rgb(245, 235, 200),
                    Rgb(170, 215, 240),
                    Rgb(220, 220, 230),
                    Rgb(255, 245, 200),
                }),
            };
        }
    }
}
