using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }
    public int CurrentMoney { get; private set; }

    public static event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (CurrentMoney < amount) return false;
        CurrentMoney -= amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        return true;
    }
}