using UnityEngine;

/// <summary>
/// STUB — Vy sẽ implement đầy đủ sau.
/// File này chỉ tồn tại để các script khác (Gun.cs, EnemyAI.cs...) compile được
/// mà không cần chờ AudioManager hoàn chỉnh.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip) { /* Vy implement */ }
    public void PlaySFX(AudioClip clip, float volume) { /* Vy implement */ }
    public void PlayMusic(AudioClip clip, bool loop = true) { /* Vy implement */ }
    public void StopMusic() { /* Vy implement */ }
    public void SetSFXVolume(float volume) { /* Vy implement */ }
    public void SetMusicVolume(float volume) { /* Vy implement */ }
}