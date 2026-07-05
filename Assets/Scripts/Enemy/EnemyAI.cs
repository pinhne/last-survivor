using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chase, Attack, Dead }

    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;

    private EnemyState _state = EnemyState.Idle;
    private NavMeshAgent _navAgent;
    private Transform _playerTransform;
    private float _attackTimer = 0f;

    private void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    private void Update()
    {
        if (_state == EnemyState.Dead) return;
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        if (dist <= attackRange)
            EnterAttack();
        else if (dist <= detectionRadius)
            EnterChase();
        else
            EnterIdle();

        if (_state == EnemyState.Chase)
            _navAgent.SetDestination(_playerTransform.position);

        if (_state == EnemyState.Attack)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                DoAttack();
                _attackTimer = attackCooldown;
            }
        }
    }

    private void EnterIdle()
    {
        if (_state == EnemyState.Idle) return;
        _state = EnemyState.Idle;
        _navAgent.isStopped = true;
    }

    private void EnterChase()
    {
        if (_state == EnemyState.Chase) return;
        _state = EnemyState.Chase;
        _navAgent.isStopped = false;
    }

    private void EnterAttack()
    {
        if (_state == EnemyState.Attack) return;
        _state = EnemyState.Attack;
        _navAgent.isStopped = true;
        transform.LookAt(_playerTransform);
        _attackTimer = 0f;
    }

    private void DoAttack()
    {
        transform.LookAt(_playerTransform);
        _playerTransform.GetComponent<PlayerHealth>()?.TakeDamage(damage);
    }

    public void SetDead()
    {
        if (_state == EnemyState.Dead) return;
        _state = EnemyState.Dead;
        _navAgent.isStopped = true;
    }
}