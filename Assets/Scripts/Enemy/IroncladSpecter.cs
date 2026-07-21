using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class IroncladSpecter : EliteBossBase
{
    [Header("Ranged Attack")]
    [SerializeField] private float damage = 45f;
    [SerializeField] private float shootRange = 20f;

    [SerializeField]
    private string attackTriggerName = "Attack";

    [SerializeField]
    private float attackCooldown = 2f;

    [SerializeField]
    private float attackAnimationTimeout = 3f;

    [SerializeField]
    private float rayOriginHeight = 1.5f;

    [SerializeField]
    private float playerTargetHeight = 1f;

    [Header("Summon")]
    [SerializeField] private GameObject[] summonPrefabs;

    private bool _hasTriggeredSummon;
    private bool _isAttacking;
    private bool _attackFinished;

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

            TryTriggerSummon();

            float distance =
                GetHorizontalDistanceToPlayer();

            if (distance <= shootRange)
            {
                yield return StartCoroutine(
                    PerformRangedAttack()
                );
            }
            else
            {
                ResumeMovement();

                _navAgent.SetDestination(
                    _playerTransform.position
                );

                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    private IEnumerator PerformRangedAttack()
    {
        if (_isAttacking)
            yield break;

        if (_animator == null)
        {
            Debug.LogWarning(
                "[IroncladSpecter] Không tìm thấy Animator."
            );

            yield break;
        }

        if (!HasAnimatorTrigger(attackTriggerName))
        {
            Debug.LogWarning(
                $"[IroncladSpecter] Animator không có Trigger " +
                $"'{attackTriggerName}'."
            );

            yield break;
        }

        _isAttacking = true;
        _attackFinished = false;

        StopMovement();
        FacePlayerImmediately();

        _animator.ResetTrigger(attackTriggerName);
        _animator.SetTrigger(attackTriggerName);

        float elapsed = 0f;

        while (!_attackFinished &&
               elapsed < attackAnimationTimeout)
        {
            // Trước mắt cho Boss tiếp tục nhìn Player
            // trong lúc phát animation bắn thường.
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
    /// Animation Event gọi tại đúng frame Boss bắn.
    /// </summary>
    public void ExecuteRangedAttackHit()
    {
        if (_playerTransform == null)
        {
            Debug.LogWarning(
                "[IroncladSpecter] Không tìm thấy Player."
            );

            return;
        }

        Vector3 rayOrigin =
            transform.position +
            Vector3.up * rayOriginHeight;

        Vector3 targetPosition =
            _playerTransform.position +
            Vector3.up * playerTargetHeight;

        Vector3 direction =
            (targetPosition - rayOrigin).normalized;

        Debug.DrawRay(
            rayOrigin,
            direction * shootRange,
            Color.red,
            1.5f
        );

        if (Physics.Raycast(
                rayOrigin,
                direction,
                out RaycastHit hit,
                shootRange,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            PlayerHealth playerHealth =
                hit.collider
                    .GetComponentInParent<PlayerHealth>();

            Debug.Log(
                $"[IroncladSpecter] Raycast trúng: " +
                $"{hit.collider.name}, " +
                $"PlayerHealth={playerHealth != null}"
            );

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);

                Debug.Log(
                    $"[IroncladSpecter] Đã gây " +
                    $"{damage} damage."
                );
            }
        }
        else
        {
            Debug.Log(
                "[IroncladSpecter] Raycast không trúng mục tiêu."
            );
        }
    }

    public void FinishRangedAttackFromAnimation()
    {
        _attackFinished = true;
    }

    private void TryTriggerSummon()
    {
        if (_hasTriggeredSummon)
            return;

        if (_bossHealth == null)
            return;

        // Không kiểm tra HP trước khi BossHealth
        // khởi tạo xong.
        if (!_bossHealth.IsInitialized)
            return;

        float maxHealth =
            _bossHealth.MaxHP;

        float currentHealth =
            _bossHealth.CurrentHP;

        if (maxHealth <= 0f)
            return;

        // Boss chết thì không triệu hồi.
        if (currentHealth <= 0f)
            return;

        float healthRatio =
            currentHealth / maxHealth;

        Debug.Log(
            $"[IroncladSpecter] Summon check | " +
            $"HP={currentHealth}/{maxHealth} | " +
            $"Ratio={healthRatio:F2}"
        );

        if (healthRatio <= 0.5f)
        {
            TriggerSummon();
        }
    }

    private void TriggerSummon()
    {
        if (_hasTriggeredSummon)
            return;

        _hasTriggeredSummon = true;

        if (summonPrefabs == null ||
            summonPrefabs.Length == 0)
        {
            Debug.LogWarning(
                "[IroncladSpecter] Không có summonPrefabs."
            );

            return;
        }

        int spawnedCount = 0;

        for (int i = 0;
             i < summonPrefabs.Length;
             i++)
        {
            if (summonPrefabs[i] == null)
                continue;

            Vector3 spawnPosition =
                GetValidSummonPosition(i);

            Instantiate(
                summonPrefabs[i],
                spawnPosition,
                Quaternion.identity
            );

            LevelManager.Instance
                ?.RegisterEnemySpawned();

            spawnedCount++;
        }

        Debug.Log(
            $"[IroncladSpecter] Đã triệu hồi " +
            $"{spawnedCount} quái."
        );
    }

    private Vector3 GetValidSummonPosition(
        int index
    )
    {
        float angle =
            index *
            Mathf.PI *
            2f /
            Mathf.Max(1, summonPrefabs.Length);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle),
            0f,
            Mathf.Sin(angle)
        ) * 4f;

        Vector3 rawPosition =
            transform.position + offset;

        if (NavMesh.SamplePosition(
                rawPosition,
                out NavMeshHit hit,
                3f,
                NavMesh.AllAreas))
        {
            return hit.position;
        }

        return rawPosition;
    }

    private float GetHorizontalDistanceToPlayer()
    {
        Vector3 bossPosition =
            transform.position;

        Vector3 playerPosition =
            _playerTransform.position;

        bossPosition.y = 0f;
        playerPosition.y = 0f;

        return Vector3.Distance(
            bossPosition,
            playerPosition
        );
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
            in _animator.parameters)
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