using System;
using UnityEngine;

/// <summary>
/// API contract — KIỆT implement logic.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public const float LEVEL_TIME_LIMIT = 300f;
    public const int   TOTAL_LEVELS     = 2;

    public static LevelManager Instance { get; private set; }

    public int  CurrentLevel  { get; private set; }
    public float TimeRemaining { get; private set; }
    public int  EnemiesAlive  { get; private set; }
    public bool IsBossPhase   { get; private set; }

    public static event Action<float> OnTimerUpdated;
    public static event Action<int>   OnEnemyCountChanged;
    public static event Action<bool>  OnWaveIntermissionStateChanged;
    public static event Action        OnAllNormalWavesCleared;
    public static event Action        OnLevelVictory;
    public static event Action        OnLevelFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterEnemySpawned()  { }
    public void RegisterEnemyKilled()   { }
    public void RegisterBossDefeated()  { }
    public void TriggerVictory()        { }
    public void TriggerGameOver()       { }

    public static void DebugFireVictory()   => OnLevelVictory?.Invoke();
    public static void DebugFireGameOver()  => OnLevelFailed?.Invoke();
    public static void DebugFireTimerUpdated(float time)
        => OnTimerUpdated?.Invoke(time);
    public static void DebugFireEnemyCountChanged(int count)
        => OnEnemyCountChanged?.Invoke(count);

    public static void DebugFireIntermissionStateChanged(bool isIntermission)
        => OnWaveIntermissionStateChanged?.Invoke(isIntermission);
}
