using System;
using ArBlokEvren.Blocks;
using ArBlokEvren.Themes;
using UnityEngine;

namespace ArBlokEvren.World
{
    /// <summary>Renders and collides a single chunk. Rebuilds its mesh on demand.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public sealed class ChunkRenderer : MonoBehaviour
    {
        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private MeshCollider _collider;
        private Mesh _mesh;

        public Chunk Chunk { get; private set; }

        public void Init(Chunk chunk, Material opaque, Material transparent)
        {
            Chunk = chunk;
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<MeshCollider>();

            _mesh = new Mesh { name = $"ChunkMesh_{chunk.Coord}" };
            _filter.sharedMesh = _mesh;
            _renderer.sharedMaterials = new[] { opaque, transparent };
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public void Rebuild(Func<int, int, int, BlockType> sampleGlobal, TextureAtlas atlas)
        {
            ChunkMeshBuilder.Build(Chunk, sampleGlobal, atlas, _mesh);

            // MeshCollider only supports one submesh geometry; feed it the opaque mesh.
            _collider.sharedMesh = null;
            _collider.sharedMesh = _mesh.vertexCount > 0 ? _mesh : null;
            Chunk.ClearDirty();
        }
    }
}
