using System.Collections.Generic;
using ArBlokEvren.Blocks;
using ArBlokEvren.Themes;
using UnityEngine;

namespace ArBlokEvren.World
{
    /// <summary>
    /// The block database plus its rendered representation. Blocks are addressed in
    /// global integer coordinates; chunks and their renderers are created lazily.
    /// </summary>
    public sealed class VoxelWorld : MonoBehaviour
    {
        [Tooltip("How many dirty chunks may be remeshed per frame.")]
        public int RebuildsPerFrame = 4;

        private readonly Dictionary<ChunkCoord, Chunk> _chunks = new();
        private readonly Dictionary<ChunkCoord, ChunkRenderer> _renderers = new();
        private readonly HashSet<ChunkCoord> _dirty = new();

        private static readonly Vector3Int[] Neighbor6 =
        {
            new(1, 0, 0), new(-1, 0, 0),
            new(0, 1, 0), new(0, -1, 0),
            new(0, 0, 1), new(0, 0, -1)
        };

        private ThemeManager _themes;
        private System.Func<int, int, int, BlockType> _sampler;

        public int ChunkCount => _chunks.Count;

        public void Init(ThemeManager themes)
        {
            _themes = themes;
            _sampler = SampleGlobal;
        }

        public BlockType GetBlock(int gx, int gy, int gz)
        {
            var block = new Vector3Int(gx, gy, gz);
            ChunkCoord coord = Coords.BlockToChunk(block);
            if (!_chunks.TryGetValue(coord, out Chunk chunk))
            {
                return BlockType.Air;
            }

            Vector3Int l = Coords.BlockToLocal(block);
            return chunk.Get(l.x, l.y, l.z);
        }

        private BlockType SampleGlobal(int gx, int gy, int gz) => GetBlock(gx, gy, gz);

        /// <returns>True when the block value actually changed.</returns>
        public bool SetBlock(Vector3Int global, BlockType type)
        {
            ChunkCoord coord = Coords.BlockToChunk(global);
            Chunk chunk = EnsureChunk(coord);
            Vector3Int l = Coords.BlockToLocal(global);

            if (!chunk.Set(l.x, l.y, l.z, type))
            {
                return false;
            }

            _dirty.Add(coord);
            MarkNeighborsIfOnBorder(global);
            return true;
        }

        public bool SetBlockAtWorld(Vector3 worldPos, BlockType type)
        {
            return SetBlock(Coords.WorldToBlock(worldPos), type);
        }

        private void MarkNeighborsIfOnBorder(Vector3Int global)
        {
            ChunkCoord self = Coords.BlockToChunk(global);
            foreach (Vector3Int d in Neighbor6)
            {
                ChunkCoord nc = Coords.BlockToChunk(global + d);
                if (!nc.Equals(self) && _chunks.ContainsKey(nc))
                {
                    _chunks[nc].MarkDirty();
                    _dirty.Add(nc);
                }
            }
        }

        private Chunk EnsureChunk(ChunkCoord coord)
        {
            if (_chunks.TryGetValue(coord, out Chunk chunk))
            {
                return chunk;
            }

            chunk = new Chunk(coord);
            _chunks[coord] = chunk;

            var go = new GameObject($"Chunk {coord}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Coords.ChunkOriginWorld(coord);

            var renderer = go.AddComponent<ChunkRenderer>();
            renderer.Init(chunk, _themes.OpaqueMaterial, _themes.TransparentMaterial);
            _renderers[coord] = renderer;
            return chunk;
        }

        private void Update()
        {
            if (_dirty.Count == 0)
            {
                return;
            }

            int budget = RebuildsPerFrame;
            var processed = new List<ChunkCoord>();
            foreach (ChunkCoord coord in _dirty)
            {
                if (budget-- <= 0)
                {
                    break;
                }

                if (_renderers.TryGetValue(coord, out ChunkRenderer renderer))
                {
                    renderer.Rebuild(_sampler, _themes.ActiveAtlas);
                }

                processed.Add(coord);
            }

            foreach (ChunkCoord coord in processed)
            {
                _dirty.Remove(coord);
            }
        }

        public void Clear()
        {
            foreach (ChunkRenderer renderer in _renderers.Values)
            {
                if (renderer != null)
                {
                    Destroy(renderer.gameObject);
                }
            }

            _chunks.Clear();
            _renderers.Clear();
            _dirty.Clear();
        }
    }
}
