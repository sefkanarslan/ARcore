using UnityEngine;

namespace ArSpacePlanner.Util
{
    /// <summary>
    /// Builds simple runtime materials so the project ships with no authored assets.
    /// Uses the Built-in Render Pipeline unlit shaders that are always available.
    /// </summary>
    public static class MaterialFactory
    {
        public static Material Unlit(Color color)
        {
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            var material = new Material(shader) { color = color };
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
            return material;
        }

        /// <summary>Semi-transparent coloured material for furniture volumes.</summary>
        public static Material Transparent(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");
            var material = new Material(shader) { color = color };
            material.renderQueue = 3000;
            return material;
        }
    }
}
