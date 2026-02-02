using UnityEngine;

[DisallowMultipleComponent]
public class EnemySeparation : MonoBehaviour
{
    [Header("Separation")]
    public float radius = 1.2f;          // how close before we push away
    public float strength = 6f;          // how hard we push away
    public int maxNeighbors = 24;        // perf cap
    public LayerMask enemyMask;          // set to Enemy layer

    Collider[] hits;

    void Awake()
    {
        hits = new Collider[Mathf.Max(8, maxNeighbors)];
    }

    // Returns a separation vector on the XZ plane
    public Vector3 ComputeSeparation()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, enemyMask);

        if (count <= 1) return Vector3.zero;

        Vector3 away = Vector3.zero;
        Vector3 p = transform.position;

        for (int i = 0; i < count; i++)
        {
            var c = hits[i];
            if (!c) continue;

            // ignore self
            if (c.attachedRigidbody && c.attachedRigidbody.transform == transform) continue;
            if (c.transform == transform) continue;

            Vector3 d = p - c.ClosestPoint(p);
            d.y = 0f; // XZ only

            float sqr = d.sqrMagnitude;
            if (sqr < 0.0001f) continue;

            // stronger when closer (1/dist)
            away += d / sqr;
        }

        away.y = 0f;
        return away.normalized * strength;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
