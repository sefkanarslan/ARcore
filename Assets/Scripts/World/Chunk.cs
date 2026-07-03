using ArBlokEvren.Blocks;

namespace ArBlokEvren.World
{
    /// <summary>
    /// A fixed-size cubic region of blocks. Stores block ids in a flat array and
    /// tracks whether its mesh needs to be rebuilt.
    /// </summary>
    public sealed class Chunk
    {
        public const int Size = VoxelSettings.ChunkSize;
        private const int Volume = Size * Size * Size;

        public readonly ChunkCoord Coord;
        private readonly BlockType[] _blocks = new BlockType[Volume];

        /// <summary>True when the chunk contents changed and the mesh is stale.</summary>
        public bool Dirty { get; private set; }

        /// <summary>Number of non-air blocks currently stored.</summary>
        public int SolidCount { get; private set; }

        public Chunk(ChunkCoord coord)
        {
            Coord = coord;
        }

        private static int Index(int x, int y, int z) => x + Size * (y + Size * z);

        public static bool InBounds(int x, int y, int z)
        {
            return x >= 0 && x < Size && y >= 0 && y < Size && z >= 0 && z < Size;
        }

        public BlockType Get(int x, int y, int z)
        {
            if (!InBounds(x, y, z))
            {
                return BlockType.Air;
            }

            return _blocks[Index(x, y, z)];
        }

        /// <returns>True if the value actually changed.</returns>
        public bool Set(int x, int y, int z, BlockType type)
        {
            if (!InBounds(x, y, z))
            {
                return false;
            }

            int i = Index(x, y, z);
            BlockType previous = _blocks[i];
            if (previous == type)
            {
                return false;
            }

            if (previous.IsSolid())
            {
                SolidCount--;
            }

            if (type.IsSolid())
            {
                SolidCount++;
            }

            _blocks[i] = type;
            Dirty = true;
            return true;
        }

        public void MarkDirty() => Dirty = true;

        public void ClearDirty() => Dirty = false;
    }
}
