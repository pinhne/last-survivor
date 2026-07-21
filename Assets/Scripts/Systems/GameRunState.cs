using System.Collections.Generic;
using UnityEngine;

public static class GameRunState
{
    public struct WeaponAmmoState
    {
        public int currentAmmo;
        public int reserveAmmo;

        public WeaponAmmoState(int currentAmmo, int reserveAmmo)
        {
            this.currentAmmo = currentAmmo;
            this.reserveAmmo = reserveAmmo;
        }
    }

    private static readonly Dictionary<string, WeaponAmmoState> _weaponAmmoStates = new();

    private static bool _runStarted;

    public static int TotalScore { get; private set; }
    public static float TotalTime { get; private set; }
    public static int TotalKills { get; private set; }
    public static int SavedMoney { get; private set; }
    public static string EquippedWeaponName { get; private set; }

    public static IEnumerable<string> SavedWeaponNames => _weaponAmmoStates.Keys;
    public static bool HasSavedWeapons => _weaponAmmoStates.Count > 0;

    public static void ResetRun()
    {
        _weaponAmmoStates.Clear();

        TotalScore = 0;
        TotalTime = 0f;
        TotalKills = 0;
        SavedMoney = 0;
        EquippedWeaponName = null;

        _runStarted = true;

        WriteSummaryToPlayerPrefs();
    }

    public static void StartLevel()
    {
        _runStarted = true;
    }

    public static void FinishLevel(
    int scoreThisLevel,
    float elapsedTime,
    int killsThisLevel
)
    {
        TotalScore += Mathf.Max(
            0,
            scoreThisLevel
        );

        TotalTime += Mathf.Max(
            0f,
            elapsedTime
        );

        TotalKills += Mathf.Max(
            0,
            killsThisLevel
        );

        WriteSummaryToPlayerPrefs();

        Debug.Log(
            $"[GameRunState] Level finished | " +
            $"Added Score={scoreThisLevel}, " +
            $"Time={elapsedTime:F1}, " +
            $"Kills={killsThisLevel} | " +
            $"Total Score={TotalScore}, " +
            $"Total Time={TotalTime:F1}, " +
            $"Total Kills={TotalKills}"
        );
    }

    public static void SaveWeaponAmmo(string weaponName, int currentAmmo, int reserveAmmo)
    {
        if (string.IsNullOrWhiteSpace(weaponName))
            return;

        _weaponAmmoStates[weaponName] = new WeaponAmmoState(currentAmmo, reserveAmmo);
    }

    public static void SaveMoney(int currentMoney)
    {
        SavedMoney = Mathf.Max(0, currentMoney);
    }

    public static int GetSavedMoney()
    {
        return Mathf.Max(0, SavedMoney);
    }

    public static bool TryGetWeaponAmmo(string weaponName, out WeaponAmmoState state)
    {
        if (string.IsNullOrWhiteSpace(weaponName))
        {
            state = default;
            return false;
        }

        return _weaponAmmoStates.TryGetValue(weaponName, out state);
    }

    public static void SaveEquippedWeapon(string weaponName)
    {
        EquippedWeaponName = weaponName;
    }

    public static bool IsRunStarted()
    {
        return _runStarted;
    }

    private static void WriteSummaryToPlayerPrefs()
    {
        PlayerPrefs.SetInt("LS_TotalScore", TotalScore);
        PlayerPrefs.SetFloat("LS_TotalTime", TotalTime);
        PlayerPrefs.SetInt("LS_TotalKills", TotalKills);
        PlayerPrefs.Save();
    }
}