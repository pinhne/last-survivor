using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// API contract — BÌNH implement logic. Shop UI gọi unlock/refill.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    private readonly HashSet<WeaponData> _unlocked = new();

    public void UnlockWeapon(WeaponData weaponData)
    {
        if (weaponData != null)
            _unlocked.Add(weaponData);
    }

    public bool IsWeaponUnlocked(WeaponData weaponData)
        => weaponData != null && _unlocked.Contains(weaponData);

    public void RefillCurrentWeaponAmmo() { }
    public void RefillAllUnlockedWeaponsAmmo() { }
}
