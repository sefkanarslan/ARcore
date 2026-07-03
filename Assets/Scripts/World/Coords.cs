using UnityEngine;

namespace ArBlokEvren.World
{
    /// <summary>Conversions between world space, global block coords and chunk coords.</summary>
    public static class Coords
    {
        /// <summary>World position -> global integer block coordinate.</summary>
        public static Vector3Int WorldToBlock(Vector3 world)
        {
            return new Vector3Int(
                Mathf.FloorToInt(world.x / VoxelSettings.VoxelSize),
                Mathf.FloorToInt(world.y / VoxelSettings.VoxelSize),
                Mathf.FloorToInt(world.z / VoxelSettings.VoxelSize));
        }

        /// <summary>Centre of a block, in world space.</summary>
        public static Vector3 BlockToWorldCenter(Vector3Int block)
        {
            return (new Vector3(block.x, block.y, block.z) + Vector3.one * 0.5f) * VoxelSettings.VoxelSize;
        }

        public static ChunkCoord BlockToChunk(Vector3Int block)
        {
            return new ChunkCoord(
                FloorDiv(block.x, VoxelSettings.ChunkSize),
                FloorDiv(block.y, VoxelSettings.ChunkSize),
                FloorDiv(block.z, VoxelSettings.ChunkSize));
        }

        /// <summary>Local coordinate of a global block inside its chunk (0..ChunkSize-1).</summary>
        public static Vector3Int BlockToLocal(Vector3Int block)
        {
            return new Vector3Int(
                Mod(block.x, VoxelSettings.ChunkSize),
                Mod(block.y, VoxelSettings.ChunkSize),
                Mod(block.z, VoxelSettings.ChunkSize));
        }

        public static Vector3 ChunkOriginWorld(ChunkCoord coord)
        {
            return new Vector3(coord.X, coord.Y, coord.Z) * VoxelSettings.ChunkWorldSize;
        }

        private static int FloorDiv(int a, int b)
        {
            int q = a / b;
            if ((a % b != 0) && ((a < 0) != (b < 0)))
            {
                q--;
            }

            return q;
        }

        private static int Mod(int a, int b)
        {
            int r = a % b;
            return r < 0 ? r + b : r;
        }
    }
}
