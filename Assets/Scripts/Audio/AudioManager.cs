using UnityEngine;

/// <summary>
/// AudioManager dùng chung toàn game.
/// - Tự tạo instance nếu scene chưa có AudioManager.
/// - Có SFX source và Music source riêng.
/// - Giữ đúng API đã thống nhất với team.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            _instance = FindFirstObjectByType<AudioManager>();

            if (_instance != null)
                return _instance;

            GameObject audioObject = new GameObject("AudioManager");
            _instance = audioObject.AddComponent<AudioManager>();
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
    }

    private void EnsureAudioSources()
    {
        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFX_Source");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("Music_Source");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        sfxSource.playOnAwake = false;
        musicSource.playOnAwake = false;

        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, sfxVolume);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        EnsureAudioSources();

        float finalVolume = Mathf.Clamp01(volume);
        sfxSource.PlayOneShot(clip, finalVolume);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null)
            return;

        EnsureAudioSources();

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
}
