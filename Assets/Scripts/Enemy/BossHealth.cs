using System;
using UnityEngine;
using UnityEngine.AI;

public class BossHealth : EnemyHealth
{
    public static event Action<float, float> OnBossHealthChanged;
    public static event Action<string> OnBossAppeared;
    public static event Action OnBossDefeated;

    [Header("Boss Info")]
    [SerializeField] private string bossName = "Boss";
    [SerializeField] private int bossReward = 300;

    [Header("Death")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private string deathTriggerName = "Die";
    [SerializeField] private float destroyAfterDeathDelay = 10f;

    public float CurrentHP => _currentHP;
    public float MaxHP => maxHP;

    protected override void Start()
    {
        base.Start();

        if (bossAnimator == null)
            bossAnimator = GetComponentInChildren<Animator>();

        OnBossAppeared?.Invoke(bossName);
        OnBossHealthChanged?.Invoke(_currentHP, maxHP);
    }

    public override void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHP -= damage;
        _currentHP = Mathf.Max(0f, _currentHP);

        OnBossHealthChanged?.Invoke(_currentHP, maxHP);

        if (_currentHP <= 0f)
            Die();
    }

    protected override void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log($"[BossHealth] Boss defeated: {bossName}");

        StopBossAI();
        StopBossMovement();
        DisableBossCollider();
        PlayDeathAnimation();
        SpawnDeathEffect();

        OnBossDefeated?.Invoke();
        EconomyManager.Instance?.AddMoney(bossReward);
        LevelManager.Instance?.RegisterBossDefeated();

        Destroy(gameObject, destroyAfterDeathDelay);
    }

    private void StopBossAI()
    {
        SandstormBrute sandstormBrute = GetComponent<SandstormBrute>();
        if (sandstormBrute != null)
        {
            sandstormBrute.StopAllCoroutines();
            sandstormBrute.enabled = false;
        }

        EliteBossBase eliteBossBase = GetComponent<EliteBossBase>();
        if (eliteBossBase != null)
        {
            eliteBossBase.StopAllCoroutines();
            eliteBossBase.enabled = false;
        }
    }

    private void StopBossMovement()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent == null) return;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        agent.enabled = false;
    }

    private void DisableBossCollider()
    {
        Collider bossCollider = GetComponent<Collider>();
        if (bossCollider != null)
            bossCollider.enabled = false;
    }

    private void PlayDeathAnimation()
    {
        if (bossAnimator == null)
        {
            Debug.LogWarning("[BossHealth] Death animation skipped: bossAnimator is null.");
            return;
        }

        if (!HasAnimatorParameter(deathTriggerName, AnimatorControllerParameterType.Trigger))
        {
            Debug.LogWarning($"[BossHealth] Death animation skipped: Animator has no trigger '{deathTriggerName}'.");
            return;
        }

        bossAnimator.SetTrigger(deathTriggerName);
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (bossAnimator == null) return false;
        if (bossAnimator.runtimeAnimatorController == null) return false;

        foreach (AnimatorControllerParameter parameter in bossAnimator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
                return true;
        }

        return false;
    }
}
