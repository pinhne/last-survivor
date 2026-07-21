using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance
    {
        get;
        private set;
    }

    public int CurrentScore
    {
        get;
        private set;
    }

    public int KillCount
    {
        get;
        private set;
    }

    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnKillCountChanged;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterKill(int scoreValue)
    {
        KillCount++;
        CurrentScore += scoreValue;

        OnKillCountChanged?.Invoke(KillCount);
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void ResetLevelStats()
    {
        CurrentScore = 0;
        KillCount = 0;

        OnScoreChanged?.Invoke(CurrentScore);
        OnKillCountChanged?.Invoke(KillCount);
    }
}