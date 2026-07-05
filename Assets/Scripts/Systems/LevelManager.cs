using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public const float LEVEL_TIME_LIMIT = 300f;
    public const int TOTAL_LEVELS = 2;

    public int CurrentLevel   { get; private set; }
    public float TimeRemaining { get; private set; }
    public int EnemiesAlive   { get; private set; }
    public bool IsBossPhase   { get; private set; }

    public static event Action<float> OnTimerUpdated;
    public static event Action<int>   OnEnemyCountChanged;
    public static event Action        OnAllNormalWavesCleared;
    public static event Action        OnLevelVictory;
    public static event Action        OnLevelFailed;

    private bool _levelEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        TimeRemaining = LEVEL_TIME_LIMIT;
        // Đọc level hiện tại từ tên scene
        CurrentLevel = SceneManager.GetActiveScene().name == "Desert" ? 1 : 2;
    }

    private void Update()
    {
        if (_levelEnded) return;

        TimeRemaining -= Time.deltaTime;
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

    // Gọi hàm này sau khi toàn bộ wave thường đã spawn + dọn sạch
    public void NotifyAllWavesCleared()
    {
        IsBossPhase = true;
        OnAllNormalWavesCleared?.Invoke();
    }

    public void RegisterBossDefeated() => TriggerVictory();

    public void TriggerVictory()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        OnLevelVictory?.Invoke();
        // Delay rồi load scene tiếp theo
        Invoke(nameof(LoadNextScene), 2f);
    }

    public void TriggerGameOver()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        OnLevelFailed?.Invoke();
        Invoke(nameof(LoadGameOver), 2f);
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