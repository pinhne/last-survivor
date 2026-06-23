using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // ── Static Events (Thu Hà lắng nghe để update weapon icon UI) ───────────
    public static event Action<WeaponData> OnWeaponChanged;

    // ── References ────────────────────────────────────────────────────────────
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private List<WeaponData> _starterWeapons;

    // ── Private State ─────────────────────────────────────────────────────────
    private List<WeaponData> _unlockedWeapons = new List<WeaponData>();
    private List<Gun> _gunInstances = new List<Gun>();
    private int _currentIndex = 0;

    // ── Properties ────────────────────────────────────────────────────────────
    public Gun CurrentGun => _gunInstances.Count > 0 ? _gunInstances[_currentIndex] : null;
    public WeaponData CurrentData => _unlockedWeapons.Count > 0 ? _unlockedWeapons[_currentIndex] : null;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Start()
    {
        foreach (WeaponData weapon in _starterWeapons)
            AddWeapon(weapon);

        if (_unlockedWeapons.Count > 0)
            EquipWeapon(0);
    }

    private void Update()
    {
        HandleWeaponSwitch();
    }

    // ── Weapon Switch ─────────────────────────────────────────────────────────
    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon(3);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SwitchWeapon(1);
        if (scroll < 0f) SwitchWeapon(-1);
    }

    private void SwitchWeapon(int direction)
    {
        if (_unlockedWeapons.Count <= 1) return;

        int newIndex = (_currentIndex + direction + _unlockedWeapons.Count) % _unlockedWeapons.Count;
        EquipWeapon(newIndex);
    }

    private void EquipWeapon(int index)
    {
        if (index < 0 || index >= _unlockedWeapons.Count) return;

        foreach (Gun gun in _gunInstances)
            gun.gameObject.SetActive(false);

        _currentIndex = index;
        _gunInstances[_currentIndex].gameObject.SetActive(true);
        // Gun.OnEnable() tự broadcast OnAmmoChanged khi SetActive(true)
        // Không gọi Gun.OnAmmoChanged từ đây — C# không cho invoke event từ class ngoài

        OnWeaponChanged?.Invoke(_unlockedWeapons[_currentIndex]);
    }

    // ── Unlock Weapon (Shop gọi) ──────────────────────────────────────────────
    public void UnlockWeapon(WeaponData newWeapon)
    {
        if (_unlockedWeapons.Contains(newWeapon))
        {
            Debug.Log($"[WeaponManager] {newWeapon.weaponName} đã unlock rồi.");
            return;
        }

        AddWeapon(newWeapon);
        EquipWeapon(_unlockedWeapons.Count - 1);
        Debug.Log($"[WeaponManager] Đã unlock: {newWeapon.weaponName}");
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private void AddWeapon(WeaponData weaponData)
    {
        if (weaponData.weaponPrefab == null)
        {
            Debug.LogWarning($"[WeaponManager] {weaponData.weaponName} chưa có weaponPrefab!");
            return;
        }

        GameObject weaponObj = Instantiate(
            weaponData.weaponPrefab,
            _weaponHolder.position,
            _weaponHolder.rotation,
            _weaponHolder
        );

        Gun gun = weaponObj.GetComponent<Gun>();
        if (gun == null)
            gun = weaponObj.AddComponent<Gun>();

        weaponObj.SetActive(false);

        _unlockedWeapons.Add(weaponData);
        _gunInstances.Add(gun);
    }
}