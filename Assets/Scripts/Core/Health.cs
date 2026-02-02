using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHp = 100;
    public int hp;

    [Header("Regen")]
    [SerializeField] float baseRegenHpPerSec = 0.5f;   // base stat
    [SerializeField] float regenDelayAfterHit = 2.0f;  // seconds without taking damage before regen starts

    public event Action<float> OnHealthChanged;
    public event Action<int> OnDamaged; // damage amount
    public event Action OnDied;

    bool _dead;

    // Upgrades (optional)
    PlayerUpgrades upgrades;
    int baseMaxHp;
    float lastHealthBonusPct = float.NaN;
    float regenBank = 0f;
    float lastDamageTime = -999f; // time of last damage taken

    void Awake()
    {
        upgrades = GetComponent<PlayerUpgrades>();
        if (upgrades == null) upgrades = GetComponentInParent<PlayerUpgrades>();

        baseMaxHp = maxHp;

        ApplyHealthBonusIfNeeded(force: true);

        hp = maxHp;
        _dead = (hp <= 0);
        Notify();
    }

    void Update()
    {
        // Only the player ship should have upgrades; enemies won't, so this is cheap.
        ApplyHealthBonusIfNeeded(force: false);

        TryRegen();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (_dead) return;

        // Dodge chance (only applies if PlayerUpgrades exists)
        if (upgrades != null)
        {
            float dodge01 = upgrades.DodgeChance01;
            if (dodge01 > 0f && UnityEngine.Random.value < dodge01)
            {
                // dodged: no damage, no events, and does not reset regen timer
                return;
            }
        }

        hp = Mathf.Max(hp - amount, 0);
        lastDamageTime = Time.time;

        OnDamaged?.Invoke(amount);
        Notify();

        if (hp == 0)
        {
            _dead = true;
            OnDied?.Invoke();
        }
    }

    void TryRegen()
    {
        if (_dead) return;

        // Nothing to do if already full
        if (hp >= maxHp)
        {
            regenBank = 0f;
            return;
        }

        // Wait a bit after taking damage
        if (Time.time - lastDamageTime < regenDelayAfterHit)
            return;

        float upgradeRegen = (upgrades != null) ? Mathf.Max(0f, upgrades.healthRegenHpPerSec) : 0f;
        float regenPerSec = Mathf.Max(0f, baseRegenHpPerSec + upgradeRegen);
        if (regenPerSec <= 0f)
            return;

        // Regen pauses when paused
        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        // Accumulate fractional regen so low values still work properly
        regenBank += regenPerSec * dt;

        int healInt = Mathf.FloorToInt(regenBank);
        if (healInt <= 0)
            return;

        int before = hp;
        hp = Mathf.Min(maxHp, hp + healInt);

        // Consume what we used, keep remainder
        regenBank -= healInt;

        if (hp != before)
            Notify();
    }

    void ApplyHealthBonusIfNeeded(bool force)
    {
        if (upgrades == null) return;

        float bonus = upgrades.healthBonusPct;
        if (!force && Mathf.Approximately(bonus, lastHealthBonusPct)) return;

        // Preserve current HP percentage when max changes
        float pct = (maxHp > 0) ? (hp / (float)maxHp) : 1f;

        int newMax = Mathf.Max(1, Mathf.RoundToInt(baseMaxHp * (1f + bonus / 100f)));

        maxHp = newMax;
        hp = Mathf.Clamp(Mathf.RoundToInt(pct * maxHp), 0, maxHp);

        lastHealthBonusPct = bonus;
        Notify();
    }

    void Notify()
    {
        OnHealthChanged?.Invoke(maxHp <= 0 ? 0f : (float)hp / maxHp);
    }
}
