using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BandAutoTiling : MonoBehaviour
{
    public float textureWorldSize = 2f; // how many world units per texture repeat

    MeshRenderer mr;

    void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        UpdateTiling();
    }

    void LateUpdate()
    {
        UpdateTiling();
    }

    void UpdateTiling()
    {
        Vector3 scale = transform.localScale;

        // XZ plane bands:
        float tileX = Mathf.Max(1f, scale.x / textureWorldSize);
        float tileY = Mathf.Max(1f, scale.y / textureWorldSize);

        mr.material.SetTextureScale("_BaseMap", new Vector2(tileX, tileY));
    }
}
