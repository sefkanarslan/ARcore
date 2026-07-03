using UnityEngine;

namespace ArSpacePlanner.Util
{
    /// <summary>
    /// Builds simple runtime materials so the project ships with no authored assets.
    /// Uses Built-in Render Pipeline shaders that are always available.
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

        /// <summary>Opaque, lit material so furniture gets real shading from the scene light.</summary>
        public static Material Lit(Color color)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Diffuse");
            }

            var material = new Material(shader) { color = color };
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.15f);
            }
            return material;
        }

        /// <summary>Semi-transparent coloured material (e.g. wall/plane visualization).</summary>
        public static Material Transparent(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");
            var material = new Material(shader) { color = color };
            material.renderQueue = 3000;
            return material;
        }
    }
}
