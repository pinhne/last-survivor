using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class EliteBossBase : MonoBehaviour
{
    protected NavMeshAgent _navAgent;
    protected Transform _playerTransform;
    protected BossHealth _bossHealth;
    protected Animator _animator;

    [SerializeField] protected float attackRange = 3f;
    [SerializeField] protected float chargeDashSpeed = 10f;

    protected virtual void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _bossHealth = GetComponent<BossHealth>();
        _animator = GetComponentInChildren<Animator>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError(
                $"[{name}] Không tìm thấy GameObject có tag Player."
            );

            enabled = false;
            return;
        }

        if (_navAgent == null)
        {
            Debug.LogError(
                $"[{name}] Không tìm thấy NavMeshAgent."
            );

            enabled = false;
            return;
        }

        if (_animator == null)
        {
            Debug.LogError(
                $"[{name}] Không tìm thấy Animator trong Boss."
            );

            enabled = false;
            return;
        }

        _playerTransform = playerObject.transform;

        StartCoroutine(BossLoop());
    }

    protected abstract IEnumerator BossLoop();

    protected void StopMovement()
    {
        if (_navAgent == null)
            return;

        if (!_navAgent.enabled || !_navAgent.isOnNavMesh)
            return;

        _navAgent.isStopped = true;
        _navAgent.ResetPath();
    }

    protected void ResumeMovement()
    {
        if (_navAgent == null)
            return;

        if (!_navAgent.enabled || !_navAgent.isOnNavMesh)
            return;

        _navAgent.isStopped = false;
    }

    protected void FacePlayerImmediately()
    {
        if (_playerTransform == null)
            return;

        Vector3 direction =
            _playerTransform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    protected IEnumerator ChargeDash()
    {
        if (_navAgent == null ||
            !_navAgent.enabled ||
            !_navAgent.isOnNavMesh ||
            _playerTransform == null)
        {
            yield break;
        }

        StopMovement();

        Vector3 direction =
            _playerTransform.position - transform.position;

        direction.y = 0f;
        direction.Normalize();

        transform.rotation = Quaternion.LookRotation(direction);

        float elapsed = 0f;
        const float chargeDuration = 0.6f;

        while (elapsed < chargeDuration)
        {
            if (_navAgent == null ||
                !_navAgent.enabled ||
                !_navAgent.isOnNavMesh)
            {
                yield break;
            }

            _navAgent.Move(
                direction *
                chargeDashSpeed *
                Time.deltaTime
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        ResumeMovement();
    }
}