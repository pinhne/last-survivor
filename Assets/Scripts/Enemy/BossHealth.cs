using System;
using UnityEngine;

/// <summary>
/// API contract — KIỆT implement logic. Boss kế thừa EnemyHealth.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    public void TakeDamage(float damage) { }
}

public class BossHealth : EnemyHealth
{
    public static event Action<float, float> OnBossHealthChanged;
    public static event Action<string>        OnBossAppeared;
    public static event Action                OnBossDefeated;

    public static void DebugFireBossAppeared(string name)
        => OnBossAppeared?.Invoke(name);

    public static void DebugFireBossHealthChanged(float current, float max)
        => OnBossHealthChanged?.Invoke(current, max);

    public static void DebugFireBossDefeated()
        => OnBossDefeated?.Invoke();
}
