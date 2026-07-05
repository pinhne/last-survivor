using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public const float LEVEL_TIME_LIMIT = 300f;
    public const int TOTAL_LEVELS = 2;

    public int CurrentLevel { get; private set; }
    public float TimeRemaining { get; private set; }
    public int EnemiesAlive { get; private set; }
    public bool IsBossPhase { get; private set; }
    public bool IsWaveIntermission { get; private set; }

    public static event Action<float> OnTimerUpdated;
    public static event Action<int> OnEnemyCountChanged;
    public static event Action OnAllNormalWavesCleared;
    public static event Action OnLevelVictory;
    public static event Action OnLevelFailed;
    public static event Action<bool> OnWaveIntermissionStateChanged;

    private bool _levelEnded = false;

    [SerializeField] private float victoryLoadDelay = 10f;
    [SerializeField] private float gameOverLoadDelay = 2f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += TriggerGameOver;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= TriggerGameOver;
    }

    private void Start()
    {
        TimeRemaining = LEVEL_TIME_LIMIT;

        // Đọc level hiện tại từ tên scene.
        // Hiện game chỉ có 2 level gameplay: Desert = 1, Warzone = 2.
        CurrentLevel = SceneManager.GetActiveScene().name == "Desert" ? 1 : 2;

        OnTimerUpdated?.Invoke(TimeRemaining);
        OnEnemyCountChanged?.Invoke(EnemiesAlive);
    }

    private void Update()
    {
        if (_levelEnded) return;

        // Trong pha nghỉ giữa wave, timer màn chơi đứng lại.
        // SpawnManager vẫn chạy countdown 15 giây riêng cho intermission.
        if (IsWaveIntermission) return;

        TimeRemaining -= Time.deltaTime;
        TimeRemaining = Mathf.Max(0f, TimeRemaining);
        OnTimerUpdated?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
            TriggerGameOver();
    }

    public void RegisterEnemySpawned()
    {
        EnemiesAlive++;
        OnEnemyCountChanged?.Invoke(EnemiesAlive);
    }

    public void RegisterEnemyKilled()
    {
        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
        OnEnemyCountChanged?.Invoke(EnemiesAlive);
    }

    // SpawnManager gọi hàm này khi bắt đầu/kết thúc pha nghỉ giữa wave.
    public void SetWaveIntermission(bool isIntermission)
    {
        if (_levelEnded) return;
        if (IsWaveIntermission == isIntermission) return;

        IsWaveIntermission = isIntermission;
        OnWaveIntermissionStateChanged?.Invoke(IsWaveIntermission);
    }

    // Gọi hàm này sau khi toàn bộ wave thường đã spawn + dọn sạch.
    public void NotifyAllWavesCleared()
    {
        if (_levelEnded) return;

        SetWaveIntermission(false);
        IsBossPhase = true;
        OnAllNormalWavesCleared?.Invoke();
    }

    public void RegisterBossDefeated() => TriggerVictory();

    public void TriggerVictory()
    {
        if (_levelEnded) return;

        SetWaveIntermission(false);
        _levelEnded = true;
        OnLevelVictory?.Invoke();

        Debug.Log($"[LevelManager] Victory triggered. Loading next scene in {victoryLoadDelay}s");
        Invoke(nameof(LoadNextScene), victoryLoadDelay);
    }

    public void TriggerGameOver()
    {
        if (_levelEnded) return;

        SetWaveIntermission(false);
        _levelEnded = true;
        OnLevelFailed?.Invoke();

        Debug.Log($"[LevelManager] GameOver triggered. Loading GameOver in {gameOverLoadDelay}s");
        Invoke(nameof(LoadGameOver), gameOverLoadDelay);
    }

    private void LoadNextScene()
    {
        if (CurrentLevel < TOTAL_LEVELS)
            SceneManager.LoadScene("Warzone");
        else
            SceneManager.LoadScene("Victory");
    }

    private void LoadGameOver() => SceneManager.LoadScene("GameOver");
}
