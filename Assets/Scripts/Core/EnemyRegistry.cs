using UnityEngine;

public static class EnemyRegistry
{
    public static int AliveCount { get; private set; }

    public static void OnSpawn() => AliveCount++;
    public static void OnDeath() => AliveCount = Mathf.Max(0, AliveCount - 1);
}