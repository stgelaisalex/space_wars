using UnityEngine;

public class ShipWeapon : MonoBehaviour
{
    public Transform firePoint;

    [Header("Weapon")]
    public float range = 18f;
    public int damage = 10;
    public float fireRate = 0.91f;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public float projectileSpeed = 40f;

    [Header("Targeting")]
    public LayerMask targetLayer;

    float nextFire;

    void Update()
    {
        if (Time.time < nextFire) return;
        nextFire = Time.time + fireRate;

        if (TryGetTarget(out Transform target, out Vector3 hitPoint))
            Fire(target, hitPoint);
        else
            Fire(null, firePoint.position + firePoint.forward * 10f); // shoot forward
    }

   bool TryGetTarget(out Transform target, out Vector3 aimPoint)
    {
        target = null;
        aimPoint = Vector3.zero;

        // IMPORTANT: include triggers if your enemies use trigger colliders
        var hits = Physics.OverlapSphere(transform.position, range, targetLayer, QueryTriggerInteraction.Collide);

        if (hits.Length == 0) return false;

        float closest = float.MaxValue;
        Collider best = null;

        foreach (var h in hits)
        {
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < closest)
            {
                closest = d;
                best = h;
            }
        }

        if (!best) return false;

        // Use the collider’s transform (or parent) as the target
        target = best.attachedRigidbody ? best.attachedRigidbody.transform : best.transform;

        // Aim at closest point to the firepoint
        aimPoint = best.ClosestPoint(firePoint.position);
        return true;
    }

    void Fire(Transform target, Vector3 aimPoint)
    {
        if (!projectilePrefab || !firePoint) return;

        Vector3 dir = (aimPoint - firePoint.position);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = firePoint.forward;
            dir.y = 0f;
        }

        Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);

        var proj = Instantiate(projectilePrefab, firePoint.position, rot);
        proj.target = target; // can be null, Projectile handles it
        proj.speed = projectileSpeed;
        proj.damage = damage;
    }
}
