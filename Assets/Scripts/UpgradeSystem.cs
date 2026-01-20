using System;
using System.Collections.Generic;
using UnityEngine;

public static class UpgradeSystem
{
    public enum UpgradeType { MoveSpeed, DodgeChance, AttackSpeed, XpGain, MagnetRadius }
    public enum Rarity { Normal, Rare, Epic, Legendary }

    public struct Offer
    {
        public UpgradeType type;
        public Rarity rarity;
        public float percentValue; // e.g. 15 means +15%

        public override string ToString()
            => $"{type} +{percentValue}% ({rarity})";
    }

    // ---- Data (rarity -> value mapping) ----
    // Index = (int)Rarity: Normal, Rare, Epic, Legendary
    static readonly Dictionary<UpgradeType, float[]> ValuesByRarity = new()
    {
        { UpgradeType.MoveSpeed,    new[] { 5f, 10f, 15f, 20f } },
        { UpgradeType.DodgeChance,  new[] { 2f, 4f, 6f, 8f } },
        { UpgradeType.AttackSpeed,  new[] { 10f, 15f, 20f, 25f } },
        { UpgradeType.XpGain,       new[] { 8f, 16f, 24f, 32f } },
        { UpgradeType.MagnetRadius, new[] { 10f, 15f, 20f, 25f } },
    };

    public static float GetValue(UpgradeType type, Rarity rarity)
        => ValuesByRarity[type][(int)rarity];

    // ---- Rarity roll (50/30/15/5) ----
    public static Rarity RollRarity(System.Random rng)
    {
        int roll = rng.Next(0, 100); // 0..99
        if (roll < 50) return Rarity.Normal;
        if (roll < 80) return Rarity.Rare;      // 50..79
        if (roll < 95) return Rarity.Epic;      // 80..94
        return Rarity.Legendary;                // 95..99
    }

    // ---- Offer generation ----
    public static Offer GenerateOffer(System.Random rng, UpgradeType type)
    {
        var rarity = RollRarity(rng);
        return new Offer
        {
            type = type,
            rarity = rarity,
            percentValue = GetValue(type, rarity)
        };
    }

    /// <summary>
    /// Generates N offers. By default, no duplicates per roll (cleaner choice UI).
    /// </summary>
    public static List<Offer> GenerateChoices(System.Random rng, int count = 3, bool allowDuplicates = false)
    {
        var offers = new List<Offer>(count);

        if (allowDuplicates)
        {
            for (int i = 0; i < count; i++)
            {
                var type = (UpgradeType)rng.Next(0, Enum.GetValues(typeof(UpgradeType)).Length);
                offers.Add(GenerateOffer(rng, type));
            }
            return offers;
        }

        // no duplicates: shuffle-pick
        var types = new List<UpgradeType>((UpgradeType[])Enum.GetValues(typeof(UpgradeType)));
        for (int i = 0; i < count && types.Count > 0; i++)
        {
            int idx = rng.Next(types.Count);
            var type = types[idx];
            types.RemoveAt(idx);
            offers.Add(GenerateOffer(rng, type));
        }

        return offers;
    }

    // ---- Player stats container + Apply ----
    [Serializable]
    public class PlayerStats
    {
        public float moveSpeedBonusPct;
        public float dodgeChancePct;
        public float attackSpeedBonusPct;
        public float xpGainBonusPct;
        public float magnetRadiusBonusPct;

        // Optional caps (tweak whenever)
        public float dodgeCapPct = 60f;
    }

    public static void Apply(PlayerStats stats, Offer offer)
    {
        float v = offer.percentValue;

        switch (offer.type)
        {
            case UpgradeType.MoveSpeed:
                stats.moveSpeedBonusPct += v;
                break;

            case UpgradeType.DodgeChance:
                stats.dodgeChancePct = Mathf.Min(stats.dodgeChancePct + v, stats.dodgeCapPct);
                break;

            case UpgradeType.AttackSpeed:
                stats.attackSpeedBonusPct += v;
                break;

            case UpgradeType.XpGain:
                stats.xpGainBonusPct += v;
                break;

            case UpgradeType.MagnetRadius:
                stats.magnetRadiusBonusPct += v;
                break;
        }
    }

    // ---- Helpers to turn those into gameplay multipliers ----
    public static float PercentToMultiplier(float pct) => 1f + (pct / 100f);

    public static float MoveSpeedMultiplier(PlayerStats stats) => PercentToMultiplier(stats.moveSpeedBonusPct);
    public static float AttackSpeedMultiplier(PlayerStats stats) => PercentToMultiplier(stats.attackSpeedBonusPct);
    public static float XpGainMultiplier(PlayerStats stats) => PercentToMultiplier(stats.xpGainBonusPct);
    public static float MagnetRadiusMultiplier(PlayerStats stats) => PercentToMultiplier(stats.magnetRadiusBonusPct);
    public static float DodgeChance01(PlayerStats stats) => Mathf.Clamp01(stats.dodgeChancePct / 100f);
}
