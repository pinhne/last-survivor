using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public const float MIN_SPAWN_DIST_FROM_PLAYER = 10f;
    public const float SPAWN_INTERVAL_BASE = 1.5f;

    private Transform[] _spawnPoints;
    private Transform _playerTransform;
    private LevelData _levelData;
    private bool _allWavesSpawned = false;
    private int _totalSpawnedThisLevel = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartLevel(LevelData levelData)
    {
        _levelData = levelData;
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Lấy tất cả SpawnPoints do Quân đặt
        GameObject spawnParent = GameObject.Find("SpawnPoints");
        _spawnPoints = new Transform[spawnParent.transform.childCount];
        for (int i = 0; i < _spawnPoints.Length; i++)
            _spawnPoints[i] = spawnParent.transform.GetChild(i);

        StartCoroutine(SpawnAllWaves());
    }

    private IEnumerator SpawnAllWaves()
    {
        foreach (WaveData wave in _levelData.waves)
        {
            yield return new WaitForSeconds(wave.delayBeforeWave);

            for (int i = 0; i < wave.enemyCount; i++)
            {
                SpawnEnemy(wave.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        _allWavesSpawned = true;
        // Sau khi spawn xong, chờ đến khi EnemiesAlive == 0 rồi báo LevelManager
        StartCoroutine(WaitForAllEnemiesDead());
    }

    private IEnumerator WaitForAllEnemiesDead()
    {
        yield return new WaitUntil(() => LevelManager.Instance.EnemiesAlive == 0);
        LevelManager.Instance.NotifyAllWavesCleared();
        SpawnBoss();
    }

    private void SpawnEnemy(GameObject prefab)
    {
        Transform spawnPoint = GetValidSpawnPoint();
        if (spawnPoint == null) return;

        GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        LevelManager.Instance.RegisterEnemySpawned();
    }

    public void SpawnBoss()
    {
        // Spawn Boss tại BossArena do Quân đặt tag
        GameObject arena = GameObject.FindGameObjectWithTag("BossArena");
        if (arena == null) { Debug.LogError("Không tìm thấy BossArena!"); return; }

        Instantiate(_levelData.eliteBossPrefab, arena.transform.position, Quaternion.identity);
    }

    private Transform GetValidSpawnPoint()
    {
        // Thử tối đa 10 lần để tìm điểm spawn đủ xa player
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            float dist = Vector3.Distance(point.position, _playerTransform.position);
            if (dist >= MIN_SPAWN_DIST_FROM_PLAYER)
                return point;
        }
        return _spawnPoints[0]; // fallback
    }
}