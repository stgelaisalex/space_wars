using UnityEngine;

public class XPMagnet : MonoBehaviour
{
    [Header("Magnet")]
    public float radius = 3.5f;
    public LayerMask xpLayer; // set to XPOrb
    public float refreshRate = 0.05f; // 20 times/sec
    [Tooltip("Max XP orbs checked per scan. Increase if you have tons of orbs.")]
    public int maxOrbs = 64;

    float _t;

    PlayerUpgrades upgrades;
    float baseRadius;

    Collider[] hitsBuffer;

    void Awake()
    {
        upgrades = GetComponent<PlayerUpgrades>();
        if (upgrades == null) upgrades = GetComponentInParent<PlayerUpgrades>();

        baseRadius = radius;
        hitsBuffer = new Collider[Mathf.Max(8, maxOrbs)];
    }

    void Update()
    {
        _t += Time.deltaTime;
        if (_t < refreshRate) return;
        _t = 0f;

        float r = GetRadius();

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            r,
            hitsBuffer,
            xpLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            var orb = hitsBuffer[i] ? hitsBuffer[i].GetComponentInParent<XPOrb>() : null;
            if (orb != null) orb.StartMagnet(transform);
        }
    }

    float GetRadius()
    {
        float mult = 1f;
        if (upgrades != null && upgrades.magnetRadiusPct != 0f)
            mult += upgrades.magnetRadiusPct / 100f;

        return baseRadius * mult;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
