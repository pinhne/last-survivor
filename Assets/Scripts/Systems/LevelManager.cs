using System;
using System.Collections;
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
    private bool _victorySequenceStarted = false;

    [Header("End Flow")]
    [SerializeField] private float victoryUiDelay = 4f;
    [SerializeField] private float gameOverUiDelay = 0f;

    private Coroutine _victoryRoutine;
    private Coroutine _gameOverRoutine;

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
        Time.timeScale = 1f;
        TimeRemaining = LEVEL_TIME_LIMIT;

        // Hiện game chỉ có 2 level gameplay: Desert = 1, Warzone = 2.
        CurrentLevel = SceneManager.GetActiveScene().name == "Desert" ? 1 : 2;

        OnTimerUpdated?.Invoke(TimeRemaining);
        OnEnemyCountChanged?.Invoke(EnemiesAlive);
    }

    private void Update()
    {
        if (_levelEnded) return;

        // Trong pha nghỉ giữa wave, timer màn chơi đứng lại.
        if (IsWaveIntermission) return;

        TimeRemaining -= Time.deltaTime;
        TimeRemaining = Mathf.Max(0f, TimeRemaining);
        OnTimerUpdated?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
            TriggerGameOver();
    }

    public void RegisterEnemySpawned()
    {
        if (_levelEnded) return;

        EnemiesAlive++;
        OnEnemyCountChanged?.Invoke(EnemiesAlive);
    }

    public void RegisterEnemyKilled()
    {
        if (_levelEnded) return;

        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
        OnEnemyCountChanged?.Invoke(EnemiesAlive);
    }

    public void SetWaveIntermission(bool isIntermission)
    {
        if (_levelEnded) return;
        if (IsWaveIntermission == isIntermission) return;

        IsWaveIntermission = isIntermission;
        OnWaveIntermissionStateChanged?.Invoke(IsWaveIntermission);
    }

    public void NotifyAllWavesCleared()
    {
        if (_levelEnded) return;

        SetWaveIntermission(false);
        IsBossPhase = true;
        OnAllNormalWavesCleared?.Invoke();
    }

    public void RegisterBossDefeated()
    {
        TriggerVictory();
    }

    public void TriggerVictory()
    {
        if (_levelEnded || _victorySequenceStarted) return;

        _victorySequenceStarted = true;
        SetWaveIntermission(false);

        if (_victoryRoutine != null)
            StopCoroutine(_victoryRoutine);

        _victoryRoutine = StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        // Chặn timer/spawn logic nhưng KHÔNG pause thời gian ngay,
        // để boss có thời gian chạy animation chết.
        _levelEnded = true;

        Debug.Log($"[LevelManager] Boss defeated. Showing Victory UI after {victoryUiDelay}s.");

        if (victoryUiDelay > 0f)
            yield return new WaitForSecondsRealtime(victoryUiDelay);

        OnLevelVictory?.Invoke();
        _victoryRoutine = null;
    }

    public void TriggerGameOver()
    {
        if (_levelEnded) return;

        SetWaveIntermission(false);
        _levelEnded = true;

        if (_gameOverRoutine != null)
            StopCoroutine(_gameOverRoutine);

        _gameOverRoutine = StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        if (gameOverUiDelay > 0f)
            yield return new WaitForSecondsRealtime(gameOverUiDelay);

        OnLevelFailed?.Invoke();
        _gameOverRoutine = null;
    }

    public void ContinueAfterVictory()
    {
        Time.timeScale = 1f;

        if (CurrentLevel < TOTAL_LEVELS)
            SceneManager.LoadScene("Warzone");
        else
            SceneManager.LoadScene("Victory");
    }

    public void RetryCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // ── UI Debug Helpers ─────────────────────────────────────────────────────
    public static void DebugFireVictory()
    {
        OnLevelVictory?.Invoke();
    }

    public static void DebugFireGameOver()
    {
        OnLevelFailed?.Invoke();
    }

    public static void DebugFireTimerUpdated(float time)
    {
        OnTimerUpdated?.Invoke(time);
    }

    public static void DebugFireEnemyCountChanged(int count)
    {
        OnEnemyCountChanged?.Invoke(count);
    }

    public static void DebugFireIntermissionStateChanged(bool isIntermission)
    {
        OnWaveIntermissionStateChanged?.Invoke(isIntermission);
    }
}
