using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyContactDamage : MonoBehaviour
{
    public int damagePerTick = 8;
    public float tickSeconds = 0.5f;

    float nextTickTime;

    void OnTriggerStay(Collider other)
    {
        // Only hurt the ship
        if (!other.CompareTag("Ship")) return;

        if (Time.time < nextTickTime) return;
        nextTickTime = Time.time + tickSeconds;

        // Ship has Health.cs
        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            health.TakeDamage(damagePerTick);
        }
    }
}
