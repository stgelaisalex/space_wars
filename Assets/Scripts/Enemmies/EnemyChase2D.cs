using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase3D : MonoBehaviour
{
    [Header("Refs")]
    public string shipTag = "Ship";

    [Header("Movement")]
    public float speed = 3.5f;

    [Header("Separation")]
    [Tooltip("How much separation influences movement. 0 = none, 1 = normal, 2 = strong.")]
    public float separationWeight = 1.0f;

    [Header("Facing")]
    public bool rotateToTarget = true;
    public float turnSpeed = 720f; // degrees/sec
    public float yawOffset = 0f;   // if your model faces +Z use 0; if faces +X try 90/-90

    Transform ship;
    Rigidbody rb;
    EnemySeparation sep;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sep = GetComponent<EnemySeparation>();

        rb.useGravity = false;

        // Top-down XZ plane
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        var s = GameObject.FindGameObjectWithTag(shipTag);
        if (s) ship = s.transform;
    }

    void FixedUpdate()
    {
        if (!ship) return;

        // to ship (XZ)
        Vector3 toShip = ship.position - rb.position;
        toShip.y = 0f;

        Vector3 dirToShip = (toShip.sqrMagnitude > 0.0001f) ? toShip.normalized : Vector3.zero;

        // separation (XZ)
        Vector3 sepVec = Vector3.zero;
        if (sep != null && separationWeight > 0.0001f)
            sepVec = sep.ComputeSeparation(); // already XZ

        Vector3 desired = dirToShip;

        if (sepVec.sqrMagnitude > 0.0001f)
            desired = (desired + sepVec * separationWeight);

        if (desired.sqrMagnitude > 0.0001f)
            desired = desired.normalized;

        SetLinearVelocity(desired * speed);

        if (rotateToTarget && dirToShip.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dirToShip, Vector3.up);
            if (Mathf.Abs(yawOffset) > 0.001f)
                look *= Quaternion.Euler(0f, yawOffset, 0f);

            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, turnSpeed * Time.fixedDeltaTime));
        }
    }

    void SetLinearVelocity(Vector3 v)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = v;
#else
        rb.velocity = v;
#endif
    }
}
