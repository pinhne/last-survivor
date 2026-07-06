using System;
using UnityEngine;

/// <summary>
/// API contract — KIỆT implement logic. Thu Hà nghe event intermission.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public const float INTERMISSION_DURATION = 15f;

    public static SpawnManager Instance { get; private set; }

    public static event Action<int, int, float> OnWaveIntermissionStarted;
    public static event Action<float> OnWaveIntermissionTimerUpdated;
    public static event Action OnWaveIntermissionEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SkipIntermission() { }

    public static void DebugFireIntermissionStarted(int clearedWave, int nextWave, float duration)
        => OnWaveIntermissionStarted?.Invoke(clearedWave, nextWave, duration);

    public static void DebugFireIntermissionTimerUpdated(float remaining)
        => OnWaveIntermissionTimerUpdated?.Invoke(remaining);

    public static void DebugFireIntermissionEnded()
        => OnWaveIntermissionEnded?.Invoke();
}
