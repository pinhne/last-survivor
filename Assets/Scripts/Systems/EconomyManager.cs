using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }
    public int CurrentMoney { get; private set; }

    public static event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Chỉ xóa component EconomyManager trùng, không xóa cả GameManagers của scene mới.
            Destroy(this);
            return;
        }

        // Nếu EconomyManager đang nằm chung object với LevelManager/SpawnManager,
        // không được DontDestroyOnLoad cả object đó, vì sẽ kéo theo hệ thống level/spawn cũ sang scene mới.
        // Tạo một object riêng chỉ giữ EconomyManager để lưu tiền xuyên scene.
        bool sharesObjectWithSceneSystems =
            GetComponent<LevelManager>() != null ||
            GetComponent<SpawnManager>() != null;

        if (sharesObjectWithSceneSystems)
        {
            GameObject persistentObject = new GameObject("EconomyManager_Persistent");
            EconomyManager persistentEconomy = persistentObject.AddComponent<EconomyManager>();
            persistentEconomy.CurrentMoney = CurrentMoney;

            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true;
        if (CurrentMoney < amount) return false;

        CurrentMoney -= amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        return true;
    }

    public void ResetMoney()
    {
        CurrentMoney = 0;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public void NotifyMoneyChanged()
    {
        OnMoneyChanged?.Invoke(CurrentMoney);
    }
}
