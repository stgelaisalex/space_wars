using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Lightweight helper for upgrade definitions (rarity -> value).
/// This is optional; the LevelUpOverlayController already embeds the same tables.
/// Keep it if you want to reuse upgrade data elsewhere (tooltips, balancing screens, etc.).
/// </summary>
public static class UpgradeSystem
{
    public enum UpgradeType { MoveSpeed, DodgeChance, AttackSpeed, XpGain, MagnetRadius, Damage, Health }
    public enum Rarity { Normal, Rare, Epic, Legendary }

    // Values are rarity tiers: Normal, Rare, Epic, Legendary
    public static readonly IReadOnlyDictionary<UpgradeType, float[]> ValuesByRarity = new Dictionary<UpgradeType, float[]>
    {
        { UpgradeType.MoveSpeed,    new[] { 5f, 10f, 15f, 20f } },   // %
        { UpgradeType.DodgeChance,  new[] { 2f, 4f, 6f, 8f } },      // %
        { UpgradeType.AttackSpeed,  new[] { 10f, 15f, 20f, 25f } },  // %
        { UpgradeType.XpGain,       new[] { 8f, 16f, 24f, 32f } },   // %
        { UpgradeType.MagnetRadius, new[] { 10f, 15f, 20f, 25f } },  // %
        { UpgradeType.Damage,       new[] { 9f, 15f, 21f, 28f } },   // flat
        { UpgradeType.Health,       new[] { 15f, 20f, 25f, 30f } },  // %
    };

    public static bool IsPercent(UpgradeType t) => t != UpgradeType.Damage;

    public static float GetValue(UpgradeType type, Rarity rarity)
        => ValuesByRarity[type][(int)rarity];

    // Rarity weights: Normal 50%, Rare 30%, Epic 15%, Legendary 5%
    public static Rarity RollRarity(System.Random rng)
    {
        int roll = rng.Next(0, 100);
        if (roll < 50) return Rarity.Normal;
        if (roll < 80) return Rarity.Rare;
        if (roll < 95) return Rarity.Epic;
        return Rarity.Legendary;
    }
}
