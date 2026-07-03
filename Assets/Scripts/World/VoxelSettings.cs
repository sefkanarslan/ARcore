namespace ArBlokEvren.World
{
    /// <summary>Global tuning constants for the voxel world.</summary>
    public static class VoxelSettings
    {
        /// <summary>Edge length of a single block/voxel, in metres.</summary>
        public const float VoxelSize = 0.06f;

        /// <summary>Number of blocks along one edge of a chunk.</summary>
        public const int ChunkSize = 16;

        /// <summary>World-space edge length of a chunk, in metres.</summary>
        public const float ChunkWorldSize = ChunkSize * VoxelSize;
    }
}
