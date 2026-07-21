using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int KillsThisLevel { get; private set; }
    public int ScoreThisLevel { get; private set; }

    public float ElapsedTimeThisLevel
    {
        get
        {
            return Mathf.Clamp(
                LEVEL_TIME_LIMIT - TimeRemaining,
                0f,
                LEVEL_TIME_LIMIT
            );
        }
    }

    private bool _summarySaved = false;

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
    [SerializeField] private float victoryUiDelay = 8f;
    [SerializeField] private float gameOverUiDelay = 0f;

    [Header("Score")]
    [SerializeField] private int normalEnemyScore = 100;
    [SerializeField] private int bossScore = 1000;
    private Coroutine _victoryRoutine;
    private Coroutine _gameOverRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (SceneManager.GetActiveScene().name == "Desert")
            GameRunState.ResetRun();
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

        KillsThisLevel = 0;
        ScoreThisLevel = 0;

        CurrentLevel =
            SceneManager.GetActiveScene().name
            == "Desert"
                ? 1
                : 2;

        GameRunState.StartLevel();

        EconomyManager.Instance
            ?.NotifyMoneyChanged();

        OnTimerUpdated?.Invoke(
            TimeRemaining
        );

        OnEnemyCountChanged?.Invoke(
            EnemiesAlive
        );
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
        if (_levelEnded)
            return;

        EnemiesAlive =
            Mathf.Max(0, EnemiesAlive - 1);

        KillsThisLevel++;

        ScoreThisLevel +=
            Mathf.Max(0, normalEnemyScore);

        OnEnemyCountChanged?.Invoke(
            EnemiesAlive
        );
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
        if (_levelEnded ||
            _victorySequenceStarted)
        {
            return;
        }

        KillsThisLevel++;

        ScoreThisLevel +=
            Mathf.Max(0, bossScore);

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

        SaveManager.CompleteLevel(CurrentLevel);
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

        SaveRunSummaryAndWeapons();

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

    private void SaveRunSummaryAndWeapons()
    {
        if (_summarySaved)
            return;

        _summarySaved = true;

        WeaponManager weaponManager =
            FindFirstObjectByType<WeaponManager>();

        if (weaponManager != null)
        {
            weaponManager.SaveRunState();
        }

        int currentMoney =
            EconomyManager.Instance != null
                ? EconomyManager.Instance.CurrentMoney
                : 0;

        GameRunState.SaveMoney(currentMoney);

        GameRunState.FinishLevel(
            ScoreThisLevel,
            ElapsedTimeThisLevel,
            KillsThisLevel
        );

        Debug.Log(
            $"[LevelManager] Saved summary | " +
            $"Level={CurrentLevel}, " +
            $"Score={ScoreThisLevel}, " +
            $"Time={ElapsedTimeThisLevel:F1}, " +
            $"Kills={KillsThisLevel}, " +
            $"Money checkpoint={currentMoney}"
        );
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
