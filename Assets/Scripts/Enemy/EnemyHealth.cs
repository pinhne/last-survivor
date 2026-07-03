using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] protected float maxHP = 60f;
    [SerializeField] protected int reward = 15;
    protected float _currentHP;

    protected virtual void Start() => _currentHP = maxHP;

    public virtual void TakeDamage(float damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0f) Die();
    }

    protected virtual void Die()
    {
        GetComponent<EnemyAI>()?.SetDead();

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        EconomyManager.Instance?.AddMoney(reward);
        LevelManager.Instance?.RegisterEnemyKilled();

        Destroy(gameObject, 3f);
    }
}