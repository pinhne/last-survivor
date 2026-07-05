using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Key lưu trong PlayerPrefs
    private const string KEY_LEVEL2_UNLOCKED = "Level2Unlocked";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        LevelManager.OnLevelVictory += HandleLevelVictory;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelVictory -= HandleLevelVictory;
    }

    private void HandleLevelVictory()
    {
        // Nếu thắng màn 1 → mở khóa màn 2
        if (LevelManager.Instance.CurrentLevel == 1)
            UnlockLevel2();
    }

    public void UnlockLevel2()
    {
        PlayerPrefs.SetInt(KEY_LEVEL2_UNLOCKED, 1);
        PlayerPrefs.Save();
    }

    public bool IsLevel2Unlocked()
    {
        return PlayerPrefs.GetInt(KEY_LEVEL2_UNLOCKED, 0) == 1;
    }

    // Dùng khi cần reset (debug hoặc new game)
    public void ResetSave()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}