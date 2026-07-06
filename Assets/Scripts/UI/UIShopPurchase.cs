using UnityEngine;

/// <summary>
/// Logic mua đồ — Hướng B (RULE §8): gọi trực tiếp hệ thống, không qua ShopManager.
/// </summary>
public static class UIShopPurchase
{
    public static PlayerHealth FindPlayerHealth()
        => Object.FindFirstObjectByType<PlayerHealth>();

    public static WeaponManager FindWeaponManager()
        => Object.FindFirstObjectByType<WeaponManager>();

    public static bool TrySpendMoney(int price, out string errorMessage)
    {
        if (EconomyManager.Instance == null)
        {
            errorMessage = "Hệ thống kinh tế chưa sẵn sàng";
            return false;
        }

        if (!EconomyManager.Instance.SpendMoney(price))
        {
            errorMessage = UIShopConstants.ErrorNotEnoughMoney;
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>§10 — Mua HP: trừ 80 xu, Heal(50).</summary>
    public static bool PurchaseHpPotion(out string errorMessage)
    {
        if (!TrySpendMoney(UIShopConstants.PriceHpPotion, out errorMessage))
            return false;

        FindPlayerHealth()?.Heal(PlayerHealth.HP_POTION_HEAL);
        return true;
    }

    /// <summary>§10 — Mua Shield: trừ 120 xu, RechargeShield(100).</summary>
    public static bool PurchaseShieldRecharge(out string errorMessage)
    {
        if (!TrySpendMoney(UIShopConstants.PriceShieldRecharge, out errorMessage))
            return false;

        FindPlayerHealth()?.RechargeShield(PlayerHealth.SHIELD_RECHARGE_AMOUNT);
        return true;
    }

    /// <summary>§10 — Đạn súng hiện tại: 60 xu, RefillCurrentWeaponAmmo().</summary>
    public static bool PurchaseCurrentWeaponAmmo(out string errorMessage)
    {
        if (!TrySpendMoney(UIShopConstants.PriceCurrentAmmo, out errorMessage))
            return false;

        FindWeaponManager()?.RefillCurrentWeaponAmmo();
        return true;
    }

    /// <summary>§10 — Đạn tất cả súng đã sở hữu: 150 xu.</summary>
    public static bool PurchaseAllWeaponAmmo(out string errorMessage)
    {
        if (!TrySpendMoney(UIShopConstants.PriceAllAmmo, out errorMessage))
            return false;

        FindWeaponManager()?.RefillAllUnlockedWeaponsAmmo();
        return true;
    }

    /// <summary>§10 — Mua súng: đủ tiền + chưa sở hữu → UnlockWeapon.</summary>
    public static bool PurchaseWeapon(WeaponData weaponData, int price, out string errorMessage)
    {
        if (weaponData == null)
        {
            errorMessage = "Chưa gán WeaponData trong Inspector";
            return false;
        }

        var weaponManager = FindWeaponManager();
        if (weaponManager != null && weaponManager.IsWeaponUnlocked(weaponData))
        {
            errorMessage = UIShopConstants.ErrorWeaponOwned;
            return false;
        }

        if (!TrySpendMoney(price, out errorMessage))
            return false;

        weaponManager?.UnlockWeapon(weaponData);
        return true;
    }
}
