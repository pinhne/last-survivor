using System;
using UnityEngine;

/// <summary>
/// API contract — BÌNH implement logic. HUD nghe OnAmmoChanged.
/// </summary>
public class Gun : MonoBehaviour
{
    public static event Action<int, int> OnAmmoChanged;

    public static void DebugFireAmmoChanged(int currentAmmo, int reserveAmmo)
        => OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
}
