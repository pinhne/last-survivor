using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class EliteBossBase : MonoBehaviour
{
    protected NavMeshAgent _navAgent;
    protected Transform _playerTransform;
    protected BossHealth _bossHealth;

    [SerializeField] protected float attackRange = 3f;
    [SerializeField] protected float chargeDashSpeed = 10f;

    protected virtual void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _bossHealth = GetComponent<BossHealth>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(BossLoop());
    }

    protected abstract IEnumerator BossLoop();

    protected IEnumerator ChargeDash()
    {
        _navAgent.isStopped = true;
        Vector3 dir = (_playerTransform.position - transform.position).normalized;
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            transform.position += dir * chargeDashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _navAgent.isStopped = false;
    }
}