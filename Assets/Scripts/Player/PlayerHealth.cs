using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // ── Constants (Source of Truth — không ai tự đặt số khác) ──────────────
    public const float MAX_HP     = 100f;
    public const float MAX_SHIELD = 100f;
    public const float HP_POTION_HEAL = 50f;
    public const float SHIELD_RECHARGE_AMOUNT = 100f;

    // ── Static Events (Thu Hà lắng nghe để update UI) ──────────────────────
    public static event Action<float, float> OnHealthChanged;   // (currentHP, maxHP)
    public static event Action<float, float> OnShieldChanged;   // (currentShield, maxShield)
    public static event Action               OnPlayerDeath;

    // ── Properties (đọc được từ bên ngoài, không set được) ─────────────────
    public float CurrentHP     { get; private set; }
    public float CurrentShield { get; private set; }

    // ── Private ─────────────────────────────────────────────────────────────
    private bool _isDead = false;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Start()
    {
        // Khởi tạo full HP và Shield khi vào màn
        CurrentHP     = MAX_HP;
        CurrentShield = MAX_SHIELD;

        // Bắn event ngay lúc Start để UI cập nhật giá trị ban đầu
        OnHealthChanged?.Invoke(CurrentHP, MAX_HP);
        OnShieldChanged?.Invoke(CurrentShield, MAX_SHIELD);
    }

    // ── Public Methods ───────────────────────────────────────────────────────

    /// <summary>
    /// Kiệt gọi từ EnemyAI khi enemy tấn công player.
    /// Shield hấp thụ damage trước, phần dư mới trừ vào HP.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        // Shield hấp thụ trước
        if (CurrentShield > 0)
        {
            float absorbed = Mathf.Min(CurrentShield, damage);
            CurrentShield -= absorbed;
            damage        -= absorbed;

            OnShieldChanged?.Invoke(CurrentShield, MAX_SHIELD);
        }

        // Phần damage còn lại mới trừ vào HP
        if (damage > 0)
        {
            CurrentHP = Mathf.Max(CurrentHP - damage, 0f);
            OnHealthChanged?.Invoke(CurrentHP, MAX_HP);
        }

        // Kiểm tra chết
        if (CurrentHP <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Thu Hà / ShopManager gọi khi player mua HP Potion.
    /// </summary>
    public void Heal(float amount)
    {
        if (_isDead) return;

        CurrentHP = Mathf.Min(CurrentHP + amount, MAX_HP);
        OnHealthChanged?.Invoke(CurrentHP, MAX_HP);
    }

    /// <summary>
    /// Thu Hà / ShopManager gọi khi player mua Shield Recharge.
    /// </summary>
    public void RechargeShield(float amount)
    {
        if (_isDead) return;

        CurrentShield = Mathf.Min(CurrentShield + amount, MAX_SHIELD);
        OnShieldChanged?.Invoke(CurrentShield, MAX_SHIELD);
    }


    // ── UI Debug Helpers (dùng cho UI test, không thay core gameplay) ─────────
    public static void DebugFireHealthChanged(float current, float max)
    {
        PlayerHealth playerHealth = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        playerHealth?.SyncDebugHealth(current, max);
        OnHealthChanged?.Invoke(current, max);
    }

    public static void DebugFireShieldChanged(float current, float max)
    {
        PlayerHealth playerHealth = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        playerHealth?.SyncDebugShield(current, max);
        OnShieldChanged?.Invoke(current, max);
    }

    public void SyncDebugHealth(float current, float max)
    {
        CurrentHP = Mathf.Clamp(current, 0f, max);
        if (CurrentHP > 0f)
            _isDead = false;
    }

    public void SyncDebugShield(float current, float max)
    {
        CurrentShield = Mathf.Clamp(current, 0f, max);
    }

    // ── Private Methods ──────────────────────────────────────────────────────

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;
        OnPlayerDeath?.Invoke();

        Debug.Log("Player đã chết — GameOver");
        // LevelManager sẽ lắng nghe OnPlayerDeath để gọi TriggerGameOver()
        // Không gọi LevelManager trực tiếp ở đây để giữ loose coupling
    }
}