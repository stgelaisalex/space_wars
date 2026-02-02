using System;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] int level = 1;
    [SerializeField] int currentXP = 0;

    int xpToNextLevel;

    public int Level => level;
    public int CurrentXP => currentXP;
    public int XpToNextLevel => xpToNextLevel;

    // UI (and other systems) can subscribe to this
    public event Action Changed;
    public event Action LeveledUp;

    [Header("XP Curve (tunable)")]
    [SerializeField] int baseXP = 5;          // XP for level 1
    [SerializeField] float linear = 1.2f;     // steady growth
    [SerializeField] float quadratic = 0.15f; // late-game ramp

    void Awake()
    {
        xpToNextLevel = GetXPToNextLevel(level);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        // Allow multiple level-ups from a big XP burst
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            level++;
            xpToNextLevel = GetXPToNextLevel(level);

            LeveledUp?.Invoke();
        }

        Changed?.Invoke();
    }

    public float Progress01()
    {
        return xpToNextLevel <= 0
            ? 0f
            : Mathf.Clamp01(currentXP / (float)xpToNextLevel);
    }

    int GetXPToNextLevel(int lvl)
    {
        int x = lvl - 1;
        return Mathf.RoundToInt(baseXP + linear * x + quadratic * x * x);
    }
}
