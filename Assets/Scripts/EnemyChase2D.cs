using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase3D : MonoBehaviour
{
    [Header("Refs")]
    public string shipTag = "Ship";

    [Header("Movement")]
    public float speed = 3.5f;

    [Header("Separation (anti-clump)")]
    public float separationRadius = 1.2f;
    public float separationStrength = 4.0f;
    public LayerMask enemyLayer;

    [Header("Facing")]
    public bool rotateToTarget = true;
    public float turnSpeed = 720f; // degrees/sec
    public float yawOffset = 0f;   // if your model faces +Z use 0; if faces +X try 90/-90

    Transform ship;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // XZ top-down plane: keep height fixed, allow yaw rotation (Y), prevent tumbling
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        EnemyRegistry.OnSpawn();
    }

    void Start()
    {
        var s = GameObject.FindGameObjectWithTag(shipTag);
        if (s) ship = s.transform;
    }

    void FixedUpdate()
    {
        if (!ship) return;

        // --- Chase ship center (XZ only) ---
        Vector3 toShip = ship.position - rb.position;
        toShip.y = 0f;

        Vector3 dirToShip = toShip.sqrMagnitude > 0.0001f ? toShip.normalized : Vector3.zero;

        // --- Separation (XZ only) ---
        Vector3 sep = Vector3.zero;
        Collider[] hits = Physics.OverlapSphere(rb.position, separationRadius, enemyLayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].attachedRigidbody == rb) continue;

            Vector3 away = rb.position - hits[i].transform.position;
            away.y = 0f;

            float d = away.magnitude;
            if (d > 0.0001f)
                sep += away / (d * d);
        }

        if (sep.sqrMagnitude > 0.0001f)
            sep = sep.normalized * separationStrength;

        // --- Final steering: seek center + separation ---
        Vector3 desiredDir = dirToShip + sep;
        if (desiredDir.sqrMagnitude > 0.0001f)
            desiredDir = desiredDir.normalized;

        rb.linearVelocity = desiredDir * speed;

        // --- Face the ship (yaw around Y) ---
        if (rotateToTarget && dirToShip.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dirToShip, Vector3.up);
            if (Mathf.Abs(yawOffset) > 0.001f)
                look *= Quaternion.Euler(0f, yawOffset, 0f);

            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, turnSpeed * Time.fixedDeltaTime));
        }
    }

    void OnDestroy()
    {
        EnemyRegistry.OnDeath();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
#endif
}
