using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform Ship;
    public GameObject[] enemyPrefabs;

    [Header("Spawn Tuning")]
    public float spawnRadius = 14f;          // distance from Ship
    public float spawnJitter = 2.5f;         // random variance
    public float spawnInterval = 1.0f;       // base spawn rate
    public int maxAlive = 80;

    [Header("Offscreen Spawn")]
    public float offscreenPadding = 4f;   // how far beyond screen edge they appear
    public float minSpawnDistance = 8f; 

    [Header("Difficulty Ramp")]
    public float intervalMin = 0.25f;
    public float rampEverySeconds = 20f;
    public float intervalStep = 0.1f;

    float timer;
    float rampTimer;

    void Reset()
    {
        // if you drop this on a GO in scene, it tries to auto-find Ship
        var p = GameObject.FindGameObjectWithTag("Ship");
        if (p) Ship = p.transform;
    }

    void Update()
    {
        if (!Ship || enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Basic cap
        if (EnemyRegistry.AliveCount >= maxAlive) return;

        timer += Time.deltaTime;
        rampTimer += Time.deltaTime;

        if (rampTimer >= rampEverySeconds)
        {
            rampTimer = 0f;
            spawnInterval = Mathf.Max(intervalMin, spawnInterval - intervalStep);
        }

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        Camera cam = Camera.main;
        if (!cam) return;

        // We'll spawn on the XZ plane at Ship's Y
        float y = Ship.position.y;

        // Get world positions of viewport corners projected onto the plane Y = shipY
        Vector3 bl = ViewportPointOnPlane(cam, new Vector2(0f, 0f), y);
        Vector3 tr = ViewportPointOnPlane(cam, new Vector2(1f, 1f), y);

        float minX = Mathf.Min(bl.x, tr.x) - offscreenPadding;
        float maxX = Mathf.Max(bl.x, tr.x) + offscreenPadding;
        float minZ = Mathf.Min(bl.z, tr.z) - offscreenPadding;
        float maxZ = Mathf.Max(bl.z, tr.z) + offscreenPadding;

        // Pick a random point on the perimeter (top/bottom/left/right)
        Vector3 pos = RandomPointOnRectPerimeter(minX, maxX, minZ, maxZ);
        pos.y = y;

        // Safety: if too close to ship, push it out
        Vector3 toPos = pos - Ship.position;
        toPos.y = 0f;
        if (toPos.magnitude < minSpawnDistance)
            pos = Ship.position + toPos.normalized * minSpawnDistance;

        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(prefab, pos, Quaternion.identity);
    }

    static Vector3 ViewportPointOnPlane(Camera cam, Vector2 viewport, float planeY)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }

    static Vector3 RandomPointOnRectPerimeter(float minX, float maxX, float minZ, float maxZ)
    {
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: // top
                return new Vector3(Random.Range(minX, maxX), 0f, maxZ);
            case 1: // bottom
                return new Vector3(Random.Range(minX, maxX), 0f, minZ);
            case 2: // left
                return new Vector3(minX, 0f, Random.Range(minZ, maxZ));
            default: // right
                return new Vector3(maxX, 0f, Random.Range(minZ, maxZ));
        }
    }

}
