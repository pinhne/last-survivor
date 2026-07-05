using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] protected float maxHP = 60f;

    // VFX khi enemy chết
    [SerializeField] private GameObject deathEffectPrefab;

    protected float _currentHP;

    protected virtual void Start()
    {
        _currentHP = maxHP;
    }

    public virtual void TakeDamage(float damage)
    {
        _currentHP -= damage;

        if (_currentHP <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        EconomyManager.Instance?.AddMoney(15);
        LevelManager.Instance?.RegisterEnemyKilled();

        // Phát hiệu ứng khi enemy chết
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Chờ 3 giây rồi hủy enemy
        Destroy(gameObject, 3f);
    }
}