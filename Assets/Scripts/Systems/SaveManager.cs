using UnityEngine;
using System;
using System.Collections.Generic;


/// <summary>
/// Quản lý tiến trình mở khóa màn chơi bằng PlayerPrefs.
/// Desert luôn mở, Warzone mở sau khi hoàn thành Desert.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string KEY_HIGHEST_UNLOCKED_LEVEL =
        "LS_HighestUnlockedLevel";

    private const string KEY_MONEY =
    "LS_PlayerMoney";

    private const string KEY_UNLOCKED_WEAPONS =
    "LS_UnlockedWeapons";

    private const char WEAPON_SEPARATOR = '|';

    public static int HighestUnlockedLevel =>
        PlayerPrefs.GetInt(
            KEY_HIGHEST_UNLOCKED_LEVEL,
            1
        );

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

    /// <summary>
    /// Kiểm tra màn có được mở khóa hay chưa.
    /// Màn 1 luôn được mở theo giá trị mặc định.
    /// </summary>
    public static bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= HighestUnlockedLevel;
    }

    /// <summary>
    /// Gọi khi người chơi hoàn thành một màn.
    /// Hoàn thành màn 1 sẽ mở màn 2.
    /// </summary>
    public static void CompleteLevel(int completedLevel)
    {
        int nextLevel = Mathf.Clamp(
            completedLevel + 1,
            1,
            LevelManager.TOTAL_LEVELS
        );

        if (nextLevel <= HighestUnlockedLevel)
            return;

        PlayerPrefs.SetInt(
            KEY_HIGHEST_UNLOCKED_LEVEL,
            nextLevel
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"[SaveManager] Đã mở khóa màn {nextLevel}."
        );
    }

    public static int LoadMoney()
    {
        return Mathf.Max(
            0,
            PlayerPrefs.GetInt(KEY_MONEY, 0)
        );
    }

    public static void SaveMoney(int amount)
    {
        PlayerPrefs.SetInt(
            KEY_MONEY,
            Mathf.Max(0, amount)
        );

        PlayerPrefs.Save();
    }

    public static void ResetMoney()
    {
        PlayerPrefs.DeleteKey(KEY_MONEY);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Chỉ xóa dữ liệu mở khóa màn, không xóa setting
    /// hoặc các dữ liệu PlayerPrefs khác.
    /// </summary>

    public static HashSet<string> LoadUnlockedWeaponNames()
    {
        HashSet<string> result =
            new HashSet<string>(
                StringComparer.Ordinal
            );

        string rawData =
            PlayerPrefs.GetString(
                KEY_UNLOCKED_WEAPONS,
                string.Empty
            );

        if (string.IsNullOrWhiteSpace(rawData))
            return result;

        string[] weaponNames =
            rawData.Split(
                new[] { WEAPON_SEPARATOR },
                StringSplitOptions.RemoveEmptyEntries
            );

        foreach (string weaponName in weaponNames)
        {
            string trimmedName = weaponName.Trim();

            if (!string.IsNullOrWhiteSpace(trimmedName))
                result.Add(trimmedName);
        }

        return result;
    }

    public static void SaveUnlockedWeapon(
        string weaponName
    )
    {
        if (string.IsNullOrWhiteSpace(weaponName))
            return;

        HashSet<string> unlockedWeapons =
            LoadUnlockedWeaponNames();

        // Đã lưu rồi thì không cần ghi lại.
        if (!unlockedWeapons.Add(weaponName))
            return;

        string saveData =
            string.Join(
                WEAPON_SEPARATOR.ToString(),
                unlockedWeapons
            );

        PlayerPrefs.SetString(
            KEY_UNLOCKED_WEAPONS,
            saveData
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"[SaveManager] Đã lưu súng: {weaponName}"
        );
    }

    public static bool IsWeaponPermanentlyUnlocked(
        string weaponName
    )
    {
        if (string.IsNullOrWhiteSpace(weaponName))
            return false;

        return LoadUnlockedWeaponNames()
            .Contains(weaponName);
    }

    /// <summary>
    /// Chỉ dùng cho Reset Data hoặc debug.
    /// Không gọi trong luồng Play thông thường.
    /// </summary>
    public static void ResetWeapons()
    {
        PlayerPrefs.DeleteKey(
            KEY_UNLOCKED_WEAPONS
        );

        PlayerPrefs.Save();

        Debug.Log(
            "[SaveManager] Đã xóa dữ liệu súng."
        );
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(
            KEY_HIGHEST_UNLOCKED_LEVEL
        );

        PlayerPrefs.Save();

        Debug.Log(
            "[SaveManager] Đã reset tiến trình màn chơi."
        );
    }
}