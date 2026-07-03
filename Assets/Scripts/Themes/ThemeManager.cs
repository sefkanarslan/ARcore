using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArBlokEvren.Themes
{
    /// <summary>
    /// Owns the shared materials used by every chunk and swaps their atlas texture
    /// when the active theme changes. Because all themes share the same atlas UV
    /// layout, switching a theme only swaps textures — chunk meshes are untouched.
    /// </summary>
    public sealed class ThemeManager : MonoBehaviour
    {
        public event Action ThemeChanged;

        public Material OpaqueMaterial { get; private set; }
        public Material TransparentMaterial { get; private set; }

        public int CurrentIndex { get; private set; }
        public ThemeDefinition Current => ThemeLibrary.All[CurrentIndex];
        public int ThemeCount => ThemeLibrary.All.Count;

        private readonly Dictionary<string, TextureAtlas> _atlasCache = new();
        private TextureAtlas _activeAtlas;

        private void Awake()
        {
            OpaqueMaterial = new Material(Shader.Find("ArBlokEvren/VoxelUnlit"))
            {
                name = "VoxelOpaque"
            };
            TransparentMaterial = new Material(Shader.Find("ArBlokEvren/VoxelUnlitTransparent"))
            {
                name = "VoxelTransparent"
            };

            ApplyTheme(0);
        }

        public TextureAtlas ActiveAtlas => _activeAtlas;

        public void ApplyTheme(int index)
        {
            index = ((index % ThemeCount) + ThemeCount) % ThemeCount;
            CurrentIndex = index;

            ThemeDefinition theme = ThemeLibrary.All[index];
            if (!_atlasCache.TryGetValue(theme.Id, out TextureAtlas atlas))
            {
                atlas = new TextureAtlas(theme);
                _atlasCache[theme.Id] = atlas;
            }

            _activeAtlas = atlas;
            OpaqueMaterial.mainTexture = atlas.Texture;
            TransparentMaterial.mainTexture = atlas.Texture;
            ThemeChanged?.Invoke();
        }

        public void NextTheme() => ApplyTheme(CurrentIndex + 1);
    }
}
