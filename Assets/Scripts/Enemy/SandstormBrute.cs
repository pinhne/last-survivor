using System.Collections;
using UnityEngine;

public class SandstormBrute : EliteBossBase
{
    [Header("Attack")]
    [SerializeField] private float damage = 35f;

    [SerializeField]
    private string attackTriggerName = "Attack";

    [SerializeField]
    private float attackCooldown = 1.5f;

    [SerializeField]
    private float attackAnimationTimeout = 3f;

    [SerializeField]
    private float hitRangeTolerance = 0.75f;

    private PlayerHealth _playerHealth;

    private bool _isAttacking;
    private bool _attackFinished;

    protected override void Start()
    {
        base.Start();

        if (_playerTransform != null)
        {
            _playerHealth =
                _playerTransform.GetComponent<PlayerHealth>();
        }

        if (_playerHealth == null)
        {
            Debug.LogError(
                "[SandstormBrute] Player không có PlayerHealth."
            );
        }

        if (!HasAnimatorTrigger(attackTriggerName))
        {
            Debug.LogError(
                $"[SandstormBrute] Animator không có Trigger " +
                $"'{attackTriggerName}'."
            );
        }
    }

    protected override IEnumerator BossLoop()
    {
        while (true)
        {
            if (_playerTransform == null ||
                _navAgent == null ||
                !_navAgent.enabled ||
                !_navAgent.isOnNavMesh)
            {
                yield break;
            }

            // Boss đi về phía Player.
            ResumeMovement();

            _navAgent.SetDestination(
                _playerTransform.position
            );

            yield return new WaitForSeconds(1.5f);

            float distance = Vector3.Distance(
                transform.position,
                _playerTransform.position
            );

            if (distance <= attackRange)
            {
                // Đủ gần: phát animation Attack.
                yield return StartCoroutine(
                    PerformMeleeAttack()
                );
            }
            else
            {
                // Còn xa: charge về phía Player.
                yield return StartCoroutine(
                    ChargeDash()
                );

                distance = Vector3.Distance(
                    transform.position,
                    _playerTransform.position
                );

                if (distance <=
                    attackRange + hitRangeTolerance)
                {
                    // Sau charge, phát animation đánh.
                    yield return StartCoroutine(
                        PerformMeleeAttack()
                    );
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }
    }

    private IEnumerator PerformMeleeAttack()
    {
        if (_isAttacking)
            yield break;

        if (_animator == null)
            yield break;

        if (!HasAnimatorTrigger(attackTriggerName))
            yield break;

        _isAttacking = true;
        _attackFinished = false;

        StopMovement();
        FacePlayerImmediately();

        _animator.ResetTrigger(attackTriggerName);
        _animator.SetTrigger(attackTriggerName);

        float elapsed = 0f;

        // Chờ Animation Event báo animation đã kết thúc.
        // Timeout để tránh Boss bị kẹt nếu thiếu event.
        while (!_attackFinished &&
               elapsed < attackAnimationTimeout)
        {
            FacePlayerImmediately();

            elapsed += Time.deltaTime;
            yield return null;
        }

        _isAttacking = false;

        ResumeMovement();

        yield return new WaitForSeconds(
            attackCooldown
        );
    }

    /// <summary>
    /// Animation Event gọi đúng tại frame tay Boss
    /// chạm Player.
    /// </summary>
    public void ApplyAttackDamageFromAnimation()
    {
    Debug.Log(
        $"[SandstormBrute] Damage event nhận được. " +
        $"PlayerHealth={_playerHealth != null}, " +
        $"Damage={damage}"
    );

    if (_playerHealth == null)
    {
        Debug.LogError(
            "[SandstormBrute] Không tìm thấy PlayerHealth."
        );

        return;
    }

    _playerHealth.TakeDamage(damage);

    Debug.Log(
        $"[SandstormBrute] Đã gọi TakeDamage({damage})."
    );
    }

    /// <summary>
    /// Animation Event gọi gần frame cuối của Attack.
    /// </summary>
    public void FinishAttackFromAnimation()
    {
        _attackFinished = true;
    }

    private bool HasAnimatorTrigger(
        string parameterName
    )
    {
        if (_animator == null ||
            _animator.runtimeAnimatorController == null)
        {
            return false;
        }

        foreach (
            AnimatorControllerParameter parameter
            in _animator.parameters
        )
        {
            if (parameter.name == parameterName &&
                parameter.type ==
                AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }
}