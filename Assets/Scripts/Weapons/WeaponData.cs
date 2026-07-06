using UnityEngine;

/// <summary>
/// ScriptableObject / data stub — BÌNH gán asset thật trong Inspector.
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Last Survivor/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string displayName = "Weapon";
}
