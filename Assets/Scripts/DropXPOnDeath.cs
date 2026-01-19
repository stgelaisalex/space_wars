using UnityEngine;

public class DropXPOnDeath : MonoBehaviour
{
    public GameObject xpPrefab;   // your XP orb prefab
    public int xpAmount = 3;

    public int minOrbs = 1;
    public int maxOrbs = 3;
    public float scatterRadius = 0.4f;

    private Health _health;

    void Awake()
    {
        _health = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (_health != null) _health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        if (_health != null) _health.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        if (xpPrefab == null) return;

        int orbs = Random.Range(minOrbs, maxOrbs + 1);

        int baseAmt = Mathf.Max(1, xpAmount / orbs);
        int remainder = xpAmount - (baseAmt * orbs);

        for (int i = 0; i < orbs; i++)
        {
            int amt = baseAmt + (i < remainder ? 1 : 0);

            Vector2 offset = Random.insideUnitCircle * scatterRadius;
            var go = Instantiate(xpPrefab, transform.position + (Vector3)offset, Quaternion.identity);

            var orb = go.GetComponent<XPOrb>();
            if (orb != null) orb.SetAmount(amt);
        }
    }
}
