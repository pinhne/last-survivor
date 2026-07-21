using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance
    {
        get;
        private set;
    }

    public int CurrentMoney
    {
        get;
        private set;
    }

    public static event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(this);
            return;
        }

        bool sharesObjectWithSceneSystems =
            GetComponent<LevelManager>() != null ||
            GetComponent<SpawnManager>() != null;

        if (sharesObjectWithSceneSystems)
        {
            // Tách EconomyManager khỏi GameManagers của scene,
            // tránh kéo LevelManager và SpawnManager cũ sang scene mới.
            GameObject persistentObject =
                new GameObject(
                    "EconomyManager_Persistent"
                );

            persistentObject.AddComponent<
                EconomyManager
            >();

            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Nạp tiền đã lưu trên máy.
        CurrentMoney =
            SaveManager.LoadMoney();

        OnMoneyChanged?.Invoke(CurrentMoney);

        Debug.Log(
            $"[EconomyManager] Loaded money: " +
            $"{CurrentMoney}"
        );
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        CurrentMoney += amount;

        SaveCurrentMoney();
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0)
            return true;

        if (CurrentMoney < amount)
            return false;

        CurrentMoney -= amount;

        SaveCurrentMoney();
        OnMoneyChanged?.Invoke(CurrentMoney);

        return true;
    }

    public void RestoreMoney(int amount)
    {
        CurrentMoney =
            Mathf.Max(0, amount);

        SaveCurrentMoney();
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public void ResetMoney()
    {
        CurrentMoney = 0;

        SaveCurrentMoney();
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public void NotifyMoneyChanged()
    {
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    private void SaveCurrentMoney()
    {
        SaveManager.SaveMoney(CurrentMoney);
    }

    internal void SyncDebugMoney(int money)
    {
        CurrentMoney =
            Mathf.Max(0, money);
    }

    public static void DebugFireMoneyChanged(
        int money
    )
    {
        Instance?.SyncDebugMoney(money);

        OnMoneyChanged?.Invoke(
            Mathf.Max(0, money)
        );
    }
}