using System.Collections.Generic;
using UnityEngine;

public class ProceduralAsteroidField : MonoBehaviour
{
    [Header("Refs")]
    public Camera cameraToFollow;
    public GameObject asteroidPrefab;         // a Quad prefab
    public Texture2D[] asteroidTextures;      // your 7 asteroid pngs

    [Header("Field")]
    public int count = 80;
    public Vector2 fieldSize = new Vector2(220, 140);   // world units (X,Z)
    public float zDepth = 30f;                          // how far away (Z in camera-forward space or world Z)
    public float minSpacing = 2.5f;                     // in world units

    [Header("Look")]
    public Vector2 scaleRange = new Vector2(0.6f, 2.2f);
    public Vector2 alphaRange = new Vector2(0.25f, 0.65f);
    public bool randomY = true;
    public Vector2 yRange = new Vector2(-10f, -30f);    // if your background planes sit around Y=-50, adjust

    [Header("Parallax")]
    [Range(0f, 1f)] public float parallax = 0.25f;      // 0 = locked to camera, 1 = world-static
    public bool infiniteWrap = true;

    // internal
    private readonly List<Transform> instances = new();
    private readonly List<Vector3> baseOffsets = new(); // offset from camera anchor
    private Vector3 lastCamPos;
    private MaterialPropertyBlock mpb;

    // shader property names (URP Unlit usually uses _BaseMap and _BaseColor)
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        if (!cameraToFollow) cameraToFollow = Camera.main;
        mpb = new MaterialPropertyBlock();

        Spawn();
        lastCamPos = cameraToFollow.transform.position;
        LateUpdate(); // place once immediately
    }

    void Spawn()
    {
        // Clean if re-run
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        instances.Clear();
        baseOffsets.Clear();

        if (asteroidTextures == null || asteroidTextures.Length == 0) return;

        // simple rejection sampling for spacing
        var placed = new List<Vector2>();
        int tries = 0;

        while (instances.Count < count && tries < count * 80)
        {
            tries++;

            float x = Random.Range(-fieldSize.x * 0.5f, fieldSize.x * 0.5f);
            float z = Random.Range(-fieldSize.y * 0.5f, fieldSize.y * 0.5f);
            var p2 = new Vector2(x, z);

            bool ok = true;
            float min2 = minSpacing * minSpacing;
            for (int j = 0; j < placed.Count; j++)
            {
                if ((placed[j] - p2).sqrMagnitude < min2) { ok = false; break; }
            }
            if (!ok) continue;

            placed.Add(p2);

            var go = Instantiate(asteroidPrefab, transform);
            go.name = $"Asteroid_{instances.Count:D3}";

            var t = go.transform;
            instances.Add(t);

            // store base offset relative to camera anchor (we’ll parallax it later)
            float y = randomY ? Random.Range(yRange.x, yRange.y) : transform.position.y;
            var offset = new Vector3(x, y, zDepth + z);
            baseOffsets.Add(offset);

            // random rotation + scale
            float s = Random.Range(scaleRange.x, scaleRange.y);
            t.localScale = new Vector3(s, s, s);
            t.localRotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f); // quad faces up; rotate around Y

            // assign texture + alpha using MaterialPropertyBlock
            var r = go.GetComponent<Renderer>();
            if (r)
            {
                mpb.Clear();
                mpb.SetTexture(BaseMap, asteroidTextures[Random.Range(0, asteroidTextures.Length)]);
                float a = Random.Range(alphaRange.x, alphaRange.y);
                mpb.SetColor(BaseColor, new Color(1f, 1f, 1f, a));
                r.SetPropertyBlock(mpb);
            }
        }
    }

    void LateUpdate()
    {
        if (!cameraToFollow) return;

        // Move this field in parallax relative to camera
        Vector3 camPos = cameraToFollow.transform.position;

        // Anchor is camera position, but you can lock XZ only if you prefer
        Vector3 anchor = camPos * parallax;

        for (int i = 0; i < instances.Count; i++)
        {
            Vector3 pos = anchor + baseOffsets[i];

            if (infiniteWrap)
            {
                // Wrap around camera space so it never ends
                // We wrap in X and Z relative to anchor
                float halfX = fieldSize.x * 0.5f;
                float halfZ = fieldSize.y * 0.5f;

                float dx = pos.x - camPos.x * parallax;
                float dz = pos.z - camPos.z * parallax;

                dx = Wrap(dx, -halfX, halfX);
                dz = Wrap(dz, -halfZ, halfZ);

                pos.x = camPos.x * parallax + dx;
                pos.z = camPos.z * parallax + dz;

                // keep the stored offset in sync (so it doesn’t drift)
                baseOffsets[i] = new Vector3(dx, baseOffsets[i].y, (pos.z - camPos.z * parallax));
            }

            instances[i].position = pos;
        }

        lastCamPos = camPos;
    }

    static float Wrap(float v, float min, float max)
    {
        float range = max - min;
        if (range <= 0f) return v;
        while (v < min) v += range;
        while (v > max) v -= range;
        return v;
    }
}
