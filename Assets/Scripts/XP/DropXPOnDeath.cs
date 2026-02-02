using UnityEngine;

public enum EnemyType
{
    Probe,
    Scout,
    Bomb,
    Explorer,
    Destroyer,
    Juggernaut,
    Behemoth,
    WorldEnder
}

public class DropXPOnDeath : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Probe;

    [Tooltip("If >= 0, overrides the default XP table for this enemy.")]
    public int xpOverride = -1;

    [Header("Drop")]
    public GameObject xpPrefab;                 // XP orb prefab (optional if you want only instant XP)
    [Range(0f, 1f)] public float instantFraction = 0.33f; // 1/3 direct XP on kill

    public int minOrbs = 1;
    public int maxOrbs = 3;
    public float scatterRadius = 0.4f;

    private Health _health;
    private PlayerXP _player;

    void Awake()
    {
        _health = GetComponent<Health>();

        // Simple: find the player's XP component once
        _player = FindFirstObjectByType<PlayerXP>();
        if (_player == null)
            Debug.LogWarning("DropXPOnDeath: No PlayerXP found in scene.");
    }

    void OnEnable()
    {
        if (_health != null) _health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        if (_health != null) _health.OnDied -= HandleDied;
    }

    int GetXPAmount()
    {
        if (xpOverride >= 0) return xpOverride;

        return enemyType switch
        {
            EnemyType.Probe => 3,
            EnemyType.Scout => 5,
            EnemyType.Bomb => 6,
            EnemyType.Explorer => 8,
            EnemyType.Destroyer => 12,
            EnemyType.Juggernaut => 18,
            EnemyType.Behemoth => 30,
            EnemyType.WorldEnder => 60,
            _ => 3
        };
    }

    void HandleDied()
    {
        int totalXP = Mathf.Max(0, GetXPAmount());
        if (totalXP <= 0) return;

        // 1) Instant XP on kill
        int instantXP = Mathf.FloorToInt(totalXP * instantFraction);
        int dropXP = totalXP - instantXP;

        if (_player != null && instantXP > 0)
            _player.AddXP(instantXP);

        // 2) Drop remaining as orbs
        if (xpPrefab == null || dropXP <= 0) return;

        int orbs = Random.Range(minOrbs, maxOrbs + 1);
        orbs = Mathf.Max(1, orbs);

        int baseAmt = Mathf.Max(1, dropXP / orbs);
        int remainder = dropXP - (baseAmt * orbs);

        for (int i = 0; i < orbs; i++)
        {
            int amt = baseAmt + (i < remainder ? 1 : 0);
            if (amt <= 0) continue;

            Vector2 offset = Random.insideUnitCircle * scatterRadius;
            var go = Instantiate(xpPrefab, transform.position + (Vector3)offset, Quaternion.identity);

            var orb = go.GetComponent<XPOrb>();
            if (orb != null) orb.SetAmount(amt);
        }
    }
}
