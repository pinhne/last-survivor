using System.Collections;
using UnityEngine;

public class SandstormBrute : EliteBossBase
{
    [SerializeField] private float damage = 35f;

    protected override IEnumerator BossLoop()
    {
        while (true)
        {
            // Di chuyển đến player
            _navAgent.SetDestination(_playerTransform.position);
            yield return new WaitForSeconds(1.5f);

            float dist = Vector3.Distance(transform.position, _playerTransform.position);

            if (dist <= attackRange)
            {
                // Đủ gần thì đánh thẳng
                _playerTransform.GetComponent<PlayerHealth>()?.TakeDamage(damage);
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                // Charge dash vào player
                yield return StartCoroutine(ChargeDash());

                // Kiểm tra lại sau khi dash
                dist = Vector3.Distance(transform.position, _playerTransform.position);
                if (dist <= attackRange)
                    _playerTransform.GetComponent<PlayerHealth>()?.TakeDamage(damage);

                yield return new WaitForSeconds(2f);
            }
        }
    }
}