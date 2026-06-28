using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] protected float maxHP = 60f;
    protected float _currentHP;

    protected virtual void Start() => _currentHP = maxHP;

    public virtual void TakeDamage(float damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0f) Die();
    }

    protected virtual void Die()
    {
        EconomyManager.Instance?.AddMoney(15); // override ở subclass để đổi reward
        LevelManager.Instance?.RegisterEnemyKilled();
        // Play animation rồi destroy
        Destroy(gameObject, 3f);
    }
}