using System;
using UnityEngine;

public class BossHealth : EnemyHealth
{
    public static event Action<float, float> OnBossHealthChanged;
    public static event Action<string> OnBossAppeared;
    public static event Action OnBossDefeated;

    [SerializeField] private string bossName = "Boss";
    [SerializeField] private int bossReward = 300;

    public float CurrentHP => _currentHP;
    public float MaxHP => maxHP;

    protected override void Start()
    {
        base.Start();
        OnBossAppeared?.Invoke(bossName);
        OnBossHealthChanged?.Invoke(_currentHP, maxHP);
    }

    public override void TakeDamage(float damage)
    {
        _currentHP -= damage;
        _currentHP = Mathf.Max(0f, _currentHP);
        OnBossHealthChanged?.Invoke(_currentHP, maxHP);
        if (_currentHP <= 0f) Die();
    }

    protected override void Die()
    {
        OnBossDefeated?.Invoke();
        EconomyManager.Instance?.AddMoney(bossReward);
        LevelManager.Instance?.RegisterBossDefeated();
        Destroy(gameObject, 3f);
    }
}