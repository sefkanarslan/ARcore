using ArBlokEvren.Blocks;
using ArBlokEvren.World;
using UnityEngine;

namespace ArBlokEvren.Demo
{
    /// <summary>
    /// Procedurally fills the voxel world with a small showcase scene (ground, sand,
    /// water, a tree, metal + glow accents) so the sandbox has something to look at
    /// and every block type / theme is visible without any AR hardware.
    /// </summary>
    public static class DemoWorld
    {
        private const int Width = 26;
        private const int Depth = 26;

        /// <param name="target">World-space point the sandbox camera should orbit.</param>
        public static void Generate(VoxelWorld world, out Vector3 target)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    world.SetBlock(new Vector3Int(x, 0, z), BlockType.Grass);
                    world.SetBlock(new Vector3Int(x, -1, z), BlockType.Dirt);
                    world.SetBlock(new Vector3Int(x, -2, z), BlockType.Stone);
                }
            }

            // Sand beach in one corner.
            for (int x = 2; x < 9; x++)
            {
                for (int z = 2; z < 9; z++)
                {
                    world.SetBlock(new Vector3Int(x, 0, z), BlockType.Sand);
                }
            }

            // Water pool.
            for (int x = 15; x < 22; x++)
            {
                for (int z = 4; z < 11; z++)
                {
                    world.SetBlock(new Vector3Int(x, 0, z), BlockType.Water);
                }
            }

            BuildTree(world, 9, 18);

            // Metal pillar + glowing blocks as accents.
            for (int y = 1; y <= 3; y++)
            {
                world.SetBlock(new Vector3Int(20, y, 20), BlockType.Metal);
            }

            world.SetBlock(new Vector3Int(13, 1, 13), BlockType.Glow);
            world.SetBlock(new Vector3Int(5, 1, 21), BlockType.Glow);
            world.SetBlock(new Vector3Int(21, 4, 20), BlockType.Glow);

            target = new Vector3(Width * 0.5f, 3f, Depth * 0.5f) * VoxelSettings.VoxelSize;
        }

        private static void BuildTree(VoxelWorld world, int tx, int tz)
        {
            for (int y = 1; y <= 5; y++)
            {
                world.SetBlock(new Vector3Int(tx, y, tz), BlockType.Wood);
            }

            const int cy = 6;
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dz = -2; dz <= 2; dz++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx * dx + dz * dz + dy * dy <= 5)
                        {
                            world.SetBlock(new Vector3Int(tx + dx, cy + dy, tz + dz), BlockType.Leaves);
                        }
                    }
                }
            }
        }
    }
}
