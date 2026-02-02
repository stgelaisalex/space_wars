using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public enum Team { Player, Enemy }

    [Header("Stats")]
    public float speed = 40f;
    public int damage = 10;
    public float lifeTime = 2f;

    [Header("Homing")]
    public Transform target;
    public float turnSpeed = 720f;
    public float hitRadius = 0.6f;

    [Header("Ownership")]
    public Team team = Team.Player;
    public Transform ownerRoot; // set when spawning

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        // ensure trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        if (target != null)
        {
            Vector3 aim = target.position;
            aim.y = transform.position.y;

            Vector3 to = aim - transform.position;
            to.y = 0f;

            if (to.magnitude <= hitRadius)
            {
                TryDamageTarget(target);
                Destroy(gameObject);
                return;
            }

            Quaternion desired = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);

            transform.position += transform.forward * step;
        }
        else
        {
            transform.position += transform.forward * step;
        }
    }

   void OnTriggerEnter(Collider other)
    {
        // Ignore triggers (XP magnet, etc.)
        if (other.isTrigger) return;

        // Ignore anything belonging to the player ship
        if (other.transform.root.CompareTag("Ship")) return;

        var h = other.GetComponentInParent<Health>();
        if (h == null) return;

        h.TakeDamage(damage);
        Destroy(gameObject);
    }

   void TryDamageTarget(Transform t)
    {
        if (t.root.CompareTag("Ship")) return;

        var h = t.GetComponentInParent<Health>();
        if (h != null) h.TakeDamage(damage);
    }
}
