using UnityEngine;

public class ParallaxUVScroll : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Renderer renderer;                 // drag Stars_Far quad here
        [Range(0f, 1f)] public float parallax = 0.15f;
        public Vector2 tiling = new Vector2(1, 1);
        public float worldToUV = 0.0025f;         // how much UV moves per world unit
        public string textureProp = "_BaseMap";   // URP Unlit uses _BaseMap
    }

    public Transform followTarget; // Ship
    public Layer[] layers;

    void Start()
    {
        // Apply tiling once (optional)
        foreach (var layer in layers)
        {
            if (!layer.renderer) continue;

            var mat = layer.renderer.material; // instance per renderer (safe to modify)
            if (mat.HasProperty(layer.textureProp))
                mat.SetTextureScale(layer.textureProp, layer.tiling);

            // Fallback for older shaders
            if (!mat.HasProperty(layer.textureProp) && mat.HasProperty("_MainTex"))
                mat.SetTextureScale("_MainTex", layer.tiling);
        }
    }

    void LateUpdate()
    {
        if (!followTarget) return;

        Vector3 p = followTarget.position;

        foreach (var layer in layers)
        {
            if (!layer.renderer) continue;

            var mat = layer.renderer.material;

            Vector2 uv = new Vector2(p.x, p.z) * (layer.worldToUV * layer.parallax);

            if (mat.HasProperty(layer.textureProp))
                mat.SetTextureOffset(layer.textureProp, uv);
            else if (mat.HasProperty("_MainTex"))
                mat.SetTextureOffset("_MainTex", uv);
        }
    }
}
