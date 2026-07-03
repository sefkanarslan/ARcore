using System;
using System.Collections.Generic;
using ArBlokEvren.Blocks;
using ArBlokEvren.Themes;
using UnityEngine;

namespace ArBlokEvren.World
{
    /// <summary>
    /// Turns a chunk's block data into a Mesh. Hidden faces (touching another solid
    /// block) are skipped. Vertex colours bake a per-face brightness so faces read
    /// as lit even under the unlit voxel shader. Submesh 0 = opaque, 1 = transparent.
    /// </summary>
    public static class ChunkMeshBuilder
    {
        // 8 corners of a unit cube.
        private static readonly Vector3[] Corners =
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0),
            new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1)
        };

        // Per face: the 4 corner indices (b3agz layout, tris 0,1,2,2,1,3).
        private static readonly int[][] FaceCorners =
        {
            new[] { 0, 3, 1, 2 }, // Back  -Z
            new[] { 5, 6, 4, 7 }, // Front +Z
            new[] { 3, 7, 2, 6 }, // Top   +Y
            new[] { 1, 5, 0, 4 }, // Bottom-Y
            new[] { 4, 7, 0, 3 }, // Left  -X
            new[] { 1, 2, 5, 6 }  // Right +X
        };

        private static readonly Vector3Int[] FaceNormals =
        {
            new(0, 0, -1), new(0, 0, 1), new(0, 1, 0), new(0, -1, 0), new(-1, 0, 0), new(1, 0, 0)
        };

        private static readonly FaceDir[] FaceDirs =
        {
            FaceDir.North, FaceDir.South, FaceDir.Up, FaceDir.Down, FaceDir.West, FaceDir.East
        };

        private static readonly float[] FaceBrightness = { 0.8f, 0.8f, 1.0f, 0.5f, 0.65f, 0.65f };

        public static void Build(
            Chunk chunk,
            Func<int, int, int, BlockType> sampleGlobal,
            TextureAtlas atlas,
            Mesh target)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var opaqueTris = new List<int>();
            var transparentTris = new List<int>();

            int baseX = chunk.Coord.X * Chunk.Size;
            int baseY = chunk.Coord.Y * Chunk.Size;
            int baseZ = chunk.Coord.Z * Chunk.Size;

            for (int z = 0; z < Chunk.Size; z++)
            for (int y = 0; y < Chunk.Size; y++)
            for (int x = 0; x < Chunk.Size; x++)
            {
                BlockType block = chunk.Get(x, y, z);
                if (!block.IsSolid())
                {
                    continue;
                }

                bool blockTransparent = block.IsTransparent();
                int gx = baseX + x, gy = baseY + y, gz = baseZ + z;

                for (int f = 0; f < 6; f++)
                {
                    Vector3Int n = FaceNormals[f];
                    BlockType neighbor = sampleGlobal(gx + n.x, gy + n.y, gz + n.z);

                    if (!ShouldDrawFace(block, blockTransparent, neighbor))
                    {
                        continue;
                    }

                    List<int> tris = blockTransparent ? transparentTris : opaqueTris;
                    AddFace(f, x, y, z, block, atlas, verts, uvs, colors, tris);
                }
            }

            target.Clear();
            if (verts.Count == 0)
            {
                return;
            }

            target.indexFormat = verts.Count > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            target.SetVertices(verts);
            target.SetUVs(0, uvs);
            target.SetColors(colors);
            target.subMeshCount = 2;
            target.SetTriangles(opaqueTris, 0);
            target.SetTriangles(transparentTris, 1);
            target.RecalculateNormals();
            target.RecalculateBounds();
        }

        private static bool ShouldDrawFace(BlockType block, bool blockTransparent, BlockType neighbor)
        {
            if (!neighbor.IsSolid())
            {
                return true;
            }

            // Opaque block hidden by any solid neighbour; transparent block only
            // draws against a different transparent neighbour or air.
            if (blockTransparent)
            {
                return !neighbor.IsTransparent();
            }

            return neighbor.IsTransparent();
        }

        private static void AddFace(
            int f, int lx, int ly, int lz, BlockType block, TextureAtlas atlas,
            List<Vector3> verts, List<Vector2> uvs, List<Color> colors, List<int> tris)
        {
            int vStart = verts.Count;
            int[] corners = FaceCorners[f];
            var origin = new Vector3(lx, ly, lz);

            for (int i = 0; i < 4; i++)
            {
                verts.Add((origin + Corners[corners[i]]) * VoxelSettings.VoxelSize);
            }

            Rect uv = atlas.GetUv(block, FaceDirs[f]);
            uvs.Add(new Vector2(uv.xMin, uv.yMin));
            uvs.Add(new Vector2(uv.xMin, uv.yMax));
            uvs.Add(new Vector2(uv.xMax, uv.yMin));
            uvs.Add(new Vector2(uv.xMax, uv.yMax));

            float b = FaceBrightness[f];
            var c = new Color(b, b, b, 1f);
            for (int i = 0; i < 4; i++)
            {
                colors.Add(c);
            }

            tris.Add(vStart + 0);
            tris.Add(vStart + 1);
            tris.Add(vStart + 2);
            tris.Add(vStart + 2);
            tris.Add(vStart + 1);
            tris.Add(vStart + 3);
        }
    }
}
