using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public const float MIN_SPAWN_DIST_FROM_PLAYER = 10f;
    public const float SPAWN_INTERVAL_BASE = 1.5f;
    public const float INTERMISSION_DURATION = 15f;

    public static event Action<int, int, float> OnWaveIntermissionStarted; // clearedWave, nextWave, duration
    public static event Action<float> OnWaveIntermissionTimerUpdated;       // remaining seconds
    public static event Action OnWaveIntermissionEnded;

    [Header("Wave Intermission")]
    [SerializeField] private float waveIntermissionDuration = INTERMISSION_DURATION;
    [SerializeField] private bool enableTemporaryContinueKey = true;
    [SerializeField] private KeyCode temporaryContinueKey = KeyCode.C;

    private Transform[] _spawnPoints;
    private Transform _playerTransform;
    private LevelData _levelData;
    private bool _allWavesSpawned = false;
    private int _totalSpawnedThisLevel = 0;
    private Coroutine _spawnRoutine;
    private bool _skipIntermissionRequested = false;

    public bool IsIntermissionActive { get; private set; }
    public float IntermissionRemaining { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        // Phím C chỉ là nút Continue tạm để test khi chưa có UI.
        // Sau này UI Button chỉ cần gọi SpawnManager.Instance.SkipIntermission().
        if (IsIntermissionActive && enableTemporaryContinueKey && Input.GetKeyDown(temporaryContinueKey))
            SkipIntermission();
    }

    public void StartLevel(LevelData levelData)
    {
        StopAllSpawning();

        _levelData = levelData;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("[SpawnManager] Không tìm thấy Player. Kiểm tra Player tag = Player.");
            return;
        }

        _playerTransform = playerObject.transform;

        // Lấy tất cả SpawnPoints do map đặt trong scene.
        GameObject spawnParent = GameObject.Find("SpawnPoints");
        if (spawnParent == null || spawnParent.transform.childCount == 0)
        {
            Debug.LogError("[SpawnManager] Không tìm thấy SpawnPoints hoặc SpawnPoints không có con.");
            return;
        }

        _spawnPoints = new Transform[spawnParent.transform.childCount];
        for (int i = 0; i < _spawnPoints.Length; i++)
            _spawnPoints[i] = spawnParent.transform.GetChild(i);

        _spawnRoutine = StartCoroutine(SpawnAllWaves());
    }

    private IEnumerator SpawnAllWaves()
    {
        if (_levelData == null)
        {
            Debug.LogError("[SpawnManager] LevelData is null.");
            yield break;
        }

        int waveCount = CountWaves();
        if (waveCount <= 0)
        {
            Debug.LogWarning("[SpawnManager] LevelData không có wave. Spawn boss ngay để test flow.");
            _allWavesSpawned = true;
            LevelManager.Instance?.NotifyAllWavesCleared();
            SpawnBoss();
            yield break;
        }

        int waveNumber = 0;

        foreach (WaveData wave in _levelData.waves)
        {
            waveNumber++;

            if (wave.delayBeforeWave > 0f)
                yield return new WaitForSeconds(wave.delayBeforeWave);

            Debug.Log($"[SpawnManager] Wave {waveNumber}/{waveCount} started.");

            for (int i = 0; i < wave.enemyCount; i++)
            {
                SpawnEnemy(wave.enemyPrefab);

                if (wave.spawnInterval > 0f)
                    yield return new WaitForSeconds(wave.spawnInterval);
            }

            // Điểm khác chính so với bản cũ:
            // Spawn xong 1 wave thì chờ người chơi dọn sạch wave đó,
            // không spawn dồn toàn bộ wave của level nữa.
            yield return new WaitUntil(() => LevelManager.Instance != null && LevelManager.Instance.EnemiesAlive == 0);

            Debug.Log($"[SpawnManager] Wave {waveNumber}/{waveCount} cleared.");

            bool isLastWave = waveNumber >= waveCount;
            if (!isLastWave)
                yield return StartCoroutine(RunWaveIntermission(waveNumber, waveNumber + 1));
        }

        _allWavesSpawned = true;

        // Sau wave cuối, chuyển thẳng sang boss phase.
        LevelManager.Instance?.NotifyAllWavesCleared();
        SpawnBoss();
    }

    private int CountWaves()
    {
        if (_levelData == null || _levelData.waves == null) return 0;

        int count = 0;
        foreach (WaveData _ in _levelData.waves)
            count++;

        return count;
    }

    private IEnumerator RunWaveIntermission(int clearedWaveNumber, int nextWaveNumber)
    {
        float duration = Mathf.Max(0f, waveIntermissionDuration);
        if (duration <= 0f) yield break;

        IsIntermissionActive = true;
        _skipIntermissionRequested = false;
        IntermissionRemaining = duration;

        LevelManager.Instance?.SetWaveIntermission(true);
        OnWaveIntermissionStarted?.Invoke(clearedWaveNumber, nextWaveNumber, duration);
        OnWaveIntermissionTimerUpdated?.Invoke(IntermissionRemaining);

        Debug.Log($"[SpawnManager] Intermission started after wave {clearedWaveNumber}. Next wave: {nextWaveNumber}. Duration: {duration}s. Press {temporaryContinueKey} to continue.");

        while (IntermissionRemaining > 0f && !_skipIntermissionRequested)
        {
            IntermissionRemaining = Mathf.Max(0f, IntermissionRemaining - Time.deltaTime);
            OnWaveIntermissionTimerUpdated?.Invoke(IntermissionRemaining);
            yield return null;
        }

        EndIntermission();
    }

    public void SkipIntermission()
    {
        if (!IsIntermissionActive) return;

        _skipIntermissionRequested = true;
        Debug.Log("[SpawnManager] Intermission skipped by Continue.");
    }

    private void EndIntermission()
    {
        if (!IsIntermissionActive) return;

        IsIntermissionActive = false;
        _skipIntermissionRequested = false;
        IntermissionRemaining = 0f;

        LevelManager.Instance?.SetWaveIntermission(false);
        OnWaveIntermissionTimerUpdated?.Invoke(0f);
        OnWaveIntermissionEnded?.Invoke();

        Debug.Log("[SpawnManager] Intermission ended. Spawning next wave.");
    }

    private void SpawnEnemy(GameObject prefab)
    {
        Transform spawnPoint = GetValidSpawnPoint();
        if (spawnPoint == null) return;

        Vector3 spawnPosition = spawnPoint.position;

        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            spawnPosition = hit.position;
        }
        else
        {
            Debug.LogWarning($"[SpawnManager] Không tìm thấy NavMesh gần {spawnPoint.name}. Enemy có thể bị lỗi.");
        }

        GameObject enemy = Instantiate(prefab, spawnPosition, spawnPoint.rotation);

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null && NavMesh.SamplePosition(spawnPosition, out NavMeshHit agentHit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(agentHit.position);
        }

        LevelManager.Instance.RegisterEnemySpawned();
    }

    public void SpawnBoss()
    {
        GameObject arena = GameObject.FindGameObjectWithTag("BossArena");
        if (arena == null)
        {
            Debug.LogError("Không tìm thấy BossArena!");
            return;
        }

        Vector3 spawnPosition = arena.transform.position;

        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 8f, NavMesh.AllAreas))
        {
            spawnPosition = hit.position;
        }
        else
        {
            Debug.LogWarning("[SpawnManager] Không tìm thấy NavMesh gần BossArena. Boss có thể không di chuyển được.");
        }

        GameObject boss = Instantiate(_levelData.eliteBossPrefab, spawnPosition, Quaternion.identity);

        NavMeshAgent agent = boss.GetComponent<NavMeshAgent>();
        if (agent != null && NavMesh.SamplePosition(spawnPosition, out NavMeshHit bossHit, 8f, NavMesh.AllAreas))
        {
            agent.Warp(bossHit.position);
        }
    }


    // ── UI Debug Helpers (dùng cho UI layout/test) ───────────────────────────
    public static void DebugFireIntermissionStarted(int clearedWave, int nextWave, float duration)
    {
        OnWaveIntermissionStarted?.Invoke(clearedWave, nextWave, duration);
    }

    public static void DebugFireIntermissionTimerUpdated(float remaining)
    {
        OnWaveIntermissionTimerUpdated?.Invoke(remaining);
    }

    public static void DebugFireIntermissionEnded()
    {
        OnWaveIntermissionEnded?.Invoke();
    }

    public void StopAllSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        StopAllCoroutines();

        if (IsIntermissionActive)
            EndIntermission();

        _allWavesSpawned = false;
        _totalSpawnedThisLevel = 0;
    }

    private Transform GetValidSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnManager] SpawnPoints chưa được setup.");
            return null;
        }

        // Thử tối đa 10 lần để tìm điểm spawn đủ xa player.
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Transform point = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];

            if (_playerTransform == null)
                return point;

            float dist = Vector3.Distance(point.position, _playerTransform.position);
            if (dist >= MIN_SPAWN_DIST_FROM_PLAYER)
                return point;
        }

        return _spawnPoints[0]; // fallback
    }
}
