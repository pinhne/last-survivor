using System;
using UnityEngine;

/// <summary>
/// API contract — BÌNH implement logic. Thu Hà chỉ lắng nghe event.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public const float MAX_HP                = 100f;
    public const float MAX_SHIELD            = 100f;
    public const float HP_POTION_HEAL        = 50f;
    public const float SHIELD_RECHARGE_AMOUNT = 100f;

    public float CurrentHP     { get; private set; } = MAX_HP;
    public float CurrentShield { get; private set; } = MAX_SHIELD;

    public static event Action<float, float> OnHealthChanged;
    public static event Action<float, float> OnShieldChanged;
    public static event Action OnPlayerDeath;

    public void TakeDamage(float damage) { }

    public void Heal(float amount)
    {
        CurrentHP = Mathf.Min(MAX_HP, CurrentHP + amount);
        OnHealthChanged?.Invoke(CurrentHP, MAX_HP);
    }

    public void RechargeShield(float amount)
    {
        CurrentShield = Mathf.Min(MAX_SHIELD, CurrentShield + amount);
        OnShieldChanged?.Invoke(CurrentShield, MAX_SHIELD);
    }

    public static void DebugFireHealthChanged(float current, float max)
    {
        var ph = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        ph?.SyncDebugHealth(current, max);
        OnHealthChanged?.Invoke(current, max);
    }

    public static void DebugFireShieldChanged(float current, float max)
    {
        var ph = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        ph?.SyncDebugShield(current, max);
        OnShieldChanged?.Invoke(current, max);
    }

    public void SyncDebugHealth(float current, float max)
    {
        CurrentHP = Mathf.Clamp(current, 0f, max);
    }

    public void SyncDebugShield(float current, float max)
    {
        CurrentShield = Mathf.Clamp(current, 0f, max);
    }
}
