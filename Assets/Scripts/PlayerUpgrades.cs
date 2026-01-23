using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    public float moveSpeedPct;
    public float attackSpeedPct;
    public float xpGainPct;
    public float magnetRadiusPct;
    public float dodgeChancePct;
    public float healthRegenHpPerSec;

    public float damagePct;        // % (NOT flat anymore)
    public float healthBonusPct;   // %

    public float DodgeChance01 => Mathf.Clamp01(dodgeChancePct / 100f);

    public float MoveSpeedMultiplier => 1f + (moveSpeedPct / 100f);
    public float HealthRegenPerSec => Mathf.Max(0f, healthRegenHpPerSec);
    public float AttackSpeedMultiplier => 1f + (attackSpeedPct / 100f);
    public float XpGainMultiplier => 1f + (xpGainPct / 100f);
    public float MagnetRadiusMultiplier => 1f + (magnetRadiusPct / 100f);
    public float DamageMultiplier => 1f + (damagePct / 100f);
    public float HealthMultiplier => 1f + (healthBonusPct / 100f);
}
