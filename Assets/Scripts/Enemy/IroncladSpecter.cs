using System.Collections;
using UnityEngine;

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

        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 4f;
            offset.y = 0;
            int idx = Random.Range(0, summonPrefabs.Length);

            GameObject minion = Instantiate(
                summonPrefabs[idx],
                transform.position + offset,
                Quaternion.identity
            );

            LevelManager.Instance?.RegisterEnemySpawned();
        }

        Debug.Log("[IroncladSpecter] Summoned 3 minions.");
    }
}
