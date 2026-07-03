using ArBlokEvren.Blocks;
using UnityEngine;

namespace ArBlokEvren.Themes
{
    /// <summary>Visual style applied when the atlas tiles are painted.</summary>
    public enum ThemeStyle
    {
        /// <summary>Noisy, pixel-jittered blocks (classic voxel look).</summary>
        Pixel,

        /// <summary>Flat cel-shaded cells with a dark outline (comic / cartoon).</summary>
        Comic,

        /// <summary>Soft gradient tiles (smooth animation look).</summary>
        Smooth
    }

    /// <summary>
    /// Data describing how a single "universe" (theme) looks: a base colour per
    /// block type plus a painting style. Textures are generated procedurally from
    /// this data at runtime, so the project ships no third-party image assets.
    /// </summary>
    public sealed class ThemeDefinition
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly ThemeStyle Style;

        private readonly Color[] _palette;

        public ThemeDefinition(string id, string displayName, ThemeStyle style, Color[] palette)
        {
            Id = id;
            DisplayName = displayName;
            Style = style;
            _palette = palette;
        }

        public Color BaseColor(BlockType type)
        {
            int i = (int)type;
            if (_palette != null && i >= 0 && i < _palette.Length)
            {
                return _palette[i];
            }

            return Color.magenta;
        }
    }
}
