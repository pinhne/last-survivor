using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected float maxHP = 60f;

    [Header("Reward")]
    [SerializeField] protected int reward = 15;

    [Header("Death VFX")]
    [SerializeField] protected GameObject deathEffectPrefab;
    [SerializeField] protected Vector3 deathEffectOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] protected float deathEffectDestroyDelay = 5f;

    protected float _currentHP;
    protected bool _isDead = false;

    protected virtual void Start()
    {
        _currentHP = maxHP;
        _isDead = false;
    }

    public virtual void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHP -= damage;

        if (_currentHP <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;

        SpawnDeathEffect();

        GetComponent<EnemyAI>()?.SetDead();

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        EconomyManager.Instance?.AddMoney(reward);
        LevelManager.Instance?.RegisterEnemyKilled();

        Destroy(gameObject, 3f);
    }

    protected void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null)
            return;

        GameObject effect = Instantiate(
            deathEffectPrefab,
            transform.position + deathEffectOffset,
            Quaternion.identity
        );

        if (deathEffectDestroyDelay > 0f)
            Destroy(effect, deathEffectDestroyDelay);
    }
}
