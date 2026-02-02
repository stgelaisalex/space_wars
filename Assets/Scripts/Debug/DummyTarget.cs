using UnityEngine;
using UnityEngine.Events;

public class DummyTarget : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 50;
    public int hp;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1.2f;
    public float projectileSpeed = 10f;
    public int damage = 10;

    [Header("Feedback")]
    public UnityEvent OnHit;
    public UnityEvent OnDeath;

    [Header("Range")]
    public float attackRange = 18f;
    public Transform target;
    float fireTimer;

    void Awake()
    {
        hp = maxHp;

        var player = GameObject.FindWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    void Update()
    {
        if (target == null) return;

        float sqrDist = (target.position - transform.position).sqrMagnitude;
        if (sqrDist > attackRange * attackRange)
            return; // out of range → do nothing

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    void AimAtTarget()
    {
        if (target == null || firePoint == null) return;

        Vector3 to = target.position - firePoint.position;
        to.y = 0f; // top-down: ignore vertical
        if (to.sqrMagnitude < 0.0001f) return;

        firePoint.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
    }

    void Shoot()
    {
        if (!projectilePrefab || !firePoint) return;

        AimAtTarget(); // 👈 add this

        var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (proj.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = firePoint.forward * projectileSpeed;

        if (proj.TryGetComponent<Projectile>(out var p))
            p.damage = damage;
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        OnHit?.Invoke();

        if (hp <= 0)
            Die();
    }

    void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
