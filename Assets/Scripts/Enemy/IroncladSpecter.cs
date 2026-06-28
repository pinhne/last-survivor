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
            // Di chuyển đến player
            _navAgent.SetDestination(_playerTransform.position);
            yield return new WaitForSeconds(2f);

            float dist = Vector3.Distance(transform.position, _playerTransform.position);

            if (dist <= shootRange)
                ShootAtPlayer();

            // Triệu hồi quái khi HP <= 50%
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
        transform.LookAt(_playerTransform);
        Vector3 dir = (_playerTransform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, shootRange))
        {
            if (hit.collider.CompareTag("Player"))
                _playerTransform.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }

    private void TriggerSummon()
    {
        if (_hasTriggeredSummon) return;
        _hasTriggeredSummon = true;

        for (int i = 0; i < 3; i++)
        {
            if (summonPrefabs == null || summonPrefabs.Length == 0) break;

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
    }
}