using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class IroncladSpecter : EliteBossBase
{
    [SerializeField] private float damage = 45f;
    [SerializeField] private float shootRange = 20f;
    [SerializeField] private GameObject[] summonPrefabs;

    private bool _hasTriggeredSummon = false;

    protected override IEnumerator BossLoop()
    {
        while (true)
        {
            if (_playerTransform == null || _navAgent == null)
                yield break;

            // Di chuyển đến player nếu agent còn hợp lệ.
            if (_navAgent.enabled && _navAgent.isOnNavMesh)
                _navAgent.SetDestination(_playerTransform.position);

            yield return new WaitForSeconds(2f);

            float dist = Vector3.Distance(transform.position, _playerTransform.position);

            if (dist <= shootRange)
                ShootAtPlayer();

            // Triệu hồi quái khi HP <= 50%.
            if (!_hasTriggeredSummon && _bossHealth != null)
            {
                if (_bossHealth.CurrentHP <= _bossHealth.MaxHP * 0.5f)
                    TriggerSummon();
            }

            yield return new WaitForSeconds(2f);
        }
    }

    private void ShootAtPlayer()
    {
        if (_playerTransform == null) return;

        Vector3 targetPosition = _playerTransform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);

        Vector3 rayOrigin = transform.position + Vector3.up;
        Vector3 direction = (_playerTransform.position + Vector3.up - rayOrigin).normalized;

        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, shootRange))
        {
            PlayerHealth playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }
    }

    private void TriggerSummon()
    {
        if (_hasTriggeredSummon) return;
        _hasTriggeredSummon = true;

        if (summonPrefabs == null || summonPrefabs.Length == 0)
        {
            Debug.LogWarning("[IroncladSpecter] Không có summonPrefabs để triệu hồi.");
            return;
        }

        int spawnedCount = 0;

        for (int i = 0; i < summonPrefabs.Length; i++)
        {
            if (summonPrefabs[i] == null) continue;

            Vector3 spawnPosition = GetValidSummonPosition(i);

            GameObject minion = Instantiate(
                summonPrefabs[i],
                spawnPosition,
                Quaternion.identity
            );

            LevelManager.Instance?.RegisterEnemySpawned();
            spawnedCount++;
        }

        Debug.Log($"[IroncladSpecter] Summoned {spawnedCount} minions.");
    }
    private Vector3 GetValidSummonPosition(int index)
    {
        float angle = index * Mathf.PI * 2f / Mathf.Max(1, summonPrefabs.Length);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 4f;
        Vector3 rawPosition = transform.position + offset;

        if (NavMesh.SamplePosition(rawPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position + offset;
    }
}
