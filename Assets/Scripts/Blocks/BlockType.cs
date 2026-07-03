namespace ArBlokEvren.Blocks
{
    /// <summary>
    /// Logical block identifiers. The visual appearance of each block depends on
    /// the currently active <see cref="ArBlokEvren.Themes.Theme"/>; the same id can
    /// look like stone in the Minecraft theme and like something else in another.
    /// </summary>
    public enum BlockType : byte
    {
        Air = 0,
        Stone = 1,
        Dirt = 2,
        Grass = 3,
        Wood = 4,
        Leaves = 5,
        Sand = 6,
        Water = 7,
        Metal = 8,
        Glow = 9,

        Count = 10
    }

    public static class BlockTypeExtensions
    {
        public static bool IsSolid(this BlockType type)
        {
            return type != BlockType.Air;
        }

        /// <summary>Blocks that should be rendered semi-transparently.</summary>
        public static bool IsTransparent(this BlockType type)
        {
            return type == BlockType.Water;
        }
    }
}
