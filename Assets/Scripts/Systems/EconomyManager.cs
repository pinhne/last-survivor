using System;
using UnityEngine;

/// <summary>
/// API contract — KIỆT implement logic.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }
    public int CurrentMoney { get; private set; }

    internal void SyncDebugMoney(int money) => CurrentMoney = money;

    public static event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CurrentMoney = 500;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (amount < 0 || CurrentMoney < amount)
            return false;
        CurrentMoney -= amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        return true;
    }

    public static void DebugFireMoneyChanged(int money)
    {
        Instance?.SyncDebugMoney(money);
        OnMoneyChanged?.Invoke(money);
    }
}
