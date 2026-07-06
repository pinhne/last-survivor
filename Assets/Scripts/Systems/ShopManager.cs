using UnityEngine;

/// <summary>
/// API contract — KIỆT implement logic. ShopUI gọi sau khi SpendMoney thành công.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PurchaseHpPotion()       { }
    public void PurchaseShieldRecharge() { }
    public void PurchaseRifle()          { }
    public void PurchaseShotgun()        { }
    public void PurchaseSniper()         { }
}
