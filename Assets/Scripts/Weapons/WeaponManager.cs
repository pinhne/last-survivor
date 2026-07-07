using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Animator _playerAnimator;
    private static readonly int WeaponIndexHash = Animator.StringToHash("WeaponIndex");

    // ── Static Events (Thu Hà lắng nghe để update weapon icon UI) ───────────
    public static event Action<WeaponData> OnWeaponChanged;

    // ── References ────────────────────────────────────────────────────────────
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private List<WeaponData> _starterWeapons;
    [SerializeField] private List<WeaponData> _allWeaponsForRestore;

    // ── Private State ─────────────────────────────────────────────────────────
    private readonly List<WeaponData> _unlockedWeapons = new List<WeaponData>();
    private readonly List<Gun> _gunInstances = new List<Gun>();
    private int _currentIndex = 0;

    // ── Properties ────────────────────────────────────────────────────────────
    public Gun CurrentGun => _gunInstances.Count > 0 ? _gunInstances[_currentIndex] : null;
    public WeaponData CurrentData => _unlockedWeapons.Count > 0 ? _unlockedWeapons[_currentIndex] : null;
    public int UnlockedWeaponCount => _unlockedWeapons.Count;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        // Lấy Animator từ HumanM_Model (con của Player)
        _playerAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        foreach (WeaponData weapon in _starterWeapons)
            AddWeapon(weapon);

        RestoreRunState();

        if (_unlockedWeapons.Count > 0 && CurrentGun == null)
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

        Gun currentGun = CurrentGun;

        // Nếu đang cầm đúng súng đó rồi thì không làm gì thêm
        if (index == _currentIndex && currentGun != null && currentGun.gameObject.activeSelf)
            return;

        // Hủy reload của súng cũ trước khi tắt
        if (currentGun != null)
        {
            currentGun.CancelReload();
            currentGun.gameObject.SetActive(false);
        }

        _currentIndex = index;

        Gun newGun = _gunInstances[_currentIndex];

        if (newGun != null)
            newGun.gameObject.SetActive(true);

        SetWeaponAnimation(_unlockedWeapons[_currentIndex].weaponAnimationIndex);

        OnWeaponChanged?.Invoke(_unlockedWeapons[_currentIndex]);
    }

    private void SetWeaponAnimation(int index)
    {
        _playerAnimator?.SetInteger(WeaponIndexHash, index);
    }

    // ── Unlock Weapon (Shop/UI gọi sau này) ───────────────────────────────────
    public void UnlockWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("[WeaponManager] Cannot unlock null weapon.");
            return;
        }

        if (_unlockedWeapons.Contains(newWeapon))
        {
            Debug.Log($"[WeaponManager] {newWeapon.weaponName} đã unlock rồi.");
            return;
        }

        AddWeapon(newWeapon);
        EquipWeapon(_unlockedWeapons.Count - 1);
        Debug.Log($"[WeaponManager] Đã unlock: {newWeapon.weaponName}");
    }

    public bool IsWeaponUnlocked(WeaponData weaponData)
    {
        return weaponData != null && _unlockedWeapons.Contains(weaponData);
    }

    // ── Ammo Support cho shop giữa wave ───────────────────────────────────────
    // Shop/UI sau này có thể gọi hàm này để mua đạn cho súng đang cầm.
    public void RefillCurrentWeaponAmmo()
    {
        Gun gun = CurrentGun;
        if (gun == null)
        {
            Debug.LogWarning("[WeaponManager] Không có súng hiện tại để refill ammo.");
            return;
        }

        gun.RefillAmmo();
    }

    // Dùng nếu shop muốn mua full ammo cho toàn bộ súng đã sở hữu.
    public void RefillAllUnlockedWeaponsAmmo()
    {
        foreach (Gun gun in _gunInstances)
        {
            if (gun != null)
                gun.RefillAmmo();
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private void AddWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("[WeaponManager] WeaponData is null.");
            return;
        }

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

        gun.Initialize(weaponData);

        weaponObj.SetActive(false);

        _unlockedWeapons.Add(weaponData);
        _gunInstances.Add(gun);
    }
    public void SaveRunState()
    {
        for (int i = 0; i < _unlockedWeapons.Count; i++)
        {
            WeaponData data = _unlockedWeapons[i];
            Gun gun = i < _gunInstances.Count ? _gunInstances[i] : null;

            if (data == null || gun == null)
                continue;

            GameRunState.SaveWeaponAmmo(
                data.weaponName,
                gun.CurrentAmmo,
                gun.ReserveAmmo
            );
        }

        if (CurrentData != null)
            GameRunState.SaveEquippedWeapon(CurrentData.weaponName);
    }

    private void RestoreRunState()
    {
        if (!GameRunState.HasSavedWeapons)
            return;

        foreach (string weaponName in GameRunState.SavedWeaponNames)
        {
            WeaponData data = FindWeaponDataByName(weaponName);

            if (data == null)
            {
                Debug.LogWarning($"[WeaponManager] Không tìm thấy WeaponData để restore: {weaponName}");
                continue;
            }

            if (!IsWeaponUnlocked(data))
                AddWeapon(data);
        }

        for (int i = 0; i < _unlockedWeapons.Count; i++)
        {
            WeaponData data = _unlockedWeapons[i];
            Gun gun = i < _gunInstances.Count ? _gunInstances[i] : null;

            if (data == null || gun == null)
                continue;

            if (GameRunState.TryGetWeaponAmmo(data.weaponName, out GameRunState.WeaponAmmoState state))
                gun.SetAmmo(state.currentAmmo, state.reserveAmmo);
        }

        int equippedIndex = FindUnlockedWeaponIndex(GameRunState.EquippedWeaponName);

        if (equippedIndex >= 0)
            EquipWeapon(equippedIndex);
        else if (_unlockedWeapons.Count > 0)
            EquipWeapon(0);
    }

    private WeaponData FindWeaponDataByName(string weaponName)
    {
        if (string.IsNullOrWhiteSpace(weaponName))
            return null;

        if (_allWeaponsForRestore != null)
        {
            foreach (WeaponData data in _allWeaponsForRestore)
            {
                if (data != null && data.weaponName == weaponName)
                    return data;
            }
        }

        if (_starterWeapons != null)
        {
            foreach (WeaponData data in _starterWeapons)
            {
                if (data != null && data.weaponName == weaponName)
                    return data;
            }
        }

        return null;
    }

    private int FindUnlockedWeaponIndex(string weaponName)
    {
        if (string.IsNullOrWhiteSpace(weaponName))
            return -1;

        for (int i = 0; i < _unlockedWeapons.Count; i++)
        {
            WeaponData data = _unlockedWeapons[i];

            if (data != null && data.weaponName == weaponName)
                return i;
        }

        return -1;
    }
}
