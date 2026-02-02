using UnityEngine;

public class ShipWeapon : MonoBehaviour
{
    public Transform firePoint;

    [Header("Weapon")]
    public float range = 18f;
    public int damage = 10;
    // NOTE: This is an interval in seconds (lower = faster)
    public float fireRate = 0.91f;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public float projectileSpeed = 40f;

    [Header("Targeting")]
    public LayerMask targetLayer;
    [Tooltip("Max colliders checked per scan. Increase if you have lots of enemies in range.")]
    public int maxTargets = 64;

    float nextFire;

    // Upgrades
    PlayerUpgrades upgrades;
    float baseFireInterval;
    int baseDamage;

    Collider[] hitsBuffer;

    void Awake()
    {
        upgrades = GetComponent<PlayerUpgrades>();
        if (upgrades == null) upgrades = GetComponentInParent<PlayerUpgrades>();

        baseFireInterval = fireRate;
        baseDamage = damage;

        hitsBuffer = new Collider[Mathf.Max(8, maxTargets)];
    }

    void OnValidate()
    {
        // keep buffer size consistent in editor if maxTargets changes
        if (maxTargets < 1) maxTargets = 1;
        if (hitsBuffer == null || hitsBuffer.Length != Mathf.Max(8, maxTargets))
            hitsBuffer = new Collider[Mathf.Max(8, maxTargets)];
    }

    void Update()
    {
        // If your game pauses via timeScale = 0, this will naturally stop firing.
        float interval = GetFireInterval();
        if (Time.time < nextFire) return;
        nextFire = Time.time + interval;

        if (TryGetTarget(out Transform target, out Vector3 hitPoint))
            Fire(target, hitPoint);
        else
            Fire(null, firePoint.position + firePoint.forward * 10f); // shoot forward
    }

    float GetFireInterval()
    {
        // interval goes DOWN when attack speed goes UP
        float mult = 1f;

        if (upgrades != null && upgrades.attackSpeedPct != 0f)
            mult += upgrades.attackSpeedPct / 100f;

        float interval = baseFireInterval / mult;

        // safety clamp so it never becomes absurdly small
        return Mathf.Max(0.05f, interval);
    }

    int GetDamage()
    {
        // Damage is now a PERCENT upgrade, not flat.
        float mult = 1f;
        if (upgrades != null)
            mult = upgrades.DamageMultiplier; // 1 + damagePct/100

        float final = baseDamage * mult;

        // Projectile.damage is int in your setup, so round to nearest
        return Mathf.Max(1, Mathf.RoundToInt(final));
    }

    bool TryGetTarget(out Transform target, out Vector3 aimPoint)
    {
        target = null;
        aimPoint = Vector3.zero;

        if (!firePoint) return false;
        if (hitsBuffer == null || hitsBuffer.Length == 0) return false;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            range,
            hitsBuffer,
            targetLayer,
            QueryTriggerInteraction.Collide
        );

        if (count <= 0) return false;

        float closest = float.MaxValue;
        Collider best = null;

        Vector3 p = transform.position;

        for (int i = 0; i < count; i++)
        {
            var h = hitsBuffer[i];
            if (!h) continue;

            float d = (p - h.transform.position).sqrMagnitude;
            if (d < closest)
            {
                closest = d;
                best = h;
            }
        }

        if (!best) return false;

        target = best.attachedRigidbody ? best.attachedRigidbody.transform : best.transform;
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
        proj.damage = GetDamage();
    }
}
