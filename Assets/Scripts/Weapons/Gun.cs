using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private Vector3 _visualKickPositionOffset = Vector3.zero;
    private Vector3 _visualKickEulerOffset = Vector3.zero;

    private Coroutine reloadCoroutine;


    // ── Static Events ───────────────────────────────────────────────────────
    public static event Action<int, int> OnAmmoChanged;

    public static void DebugFireAmmoChanged(int currentAmmo, int reserveAmmo)
    {
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }

    // ── References ─────────────────────────────────────────────────────────
    [Header("Data")]
    [SerializeField] private WeaponData _data;

    [Header("References")]
    [SerializeField] private Transform _gunBarrel;
    [SerializeField] private GameObject _muzzleFlashPrefab;
    [SerializeField] private Transform _weaponVisualRoot;
    [SerializeField] private Animator _weaponAnimator;

    [Header("Debug Logs")]
    [SerializeField] private bool _logInit = true;
    [SerializeField] private bool _logAmmoOnShot = true;
    [SerializeField] private bool _logReload = true;
    [SerializeField] private bool _logEmptyAmmo = true;
    [SerializeField] private bool _logHitDebug = false;
    [SerializeField] private bool _logBlockedReload = false;

    [Header("Muzzle Flash")]
    [SerializeField] private float _muzzleFlashScale = 0.25f;
    [SerializeField] private float _muzzleFlashDestroyDelay = 0.08f;
    private Camera _cam;
    private PlayerCamera _playerCamera;

    // ── Runtime State ──────────────────────────────────────────────────────
    private int _currentAmmo;
    private int _reserveAmmo;
    private bool _isReloading = false;
    private bool _isAiming = false;
    private float _nextFireTime = 0f;

    private Vector3 _visualDefaultLocalPosition;
    private Quaternion _visualDefaultLocalRotation;
    private bool _hasVisualPose = false;

    // ── Properties ─────────────────────────────────────────────────────────
    public WeaponData Data => _data;
    public int CurrentAmmo => _currentAmmo;
    public int ReserveAmmo => _reserveAmmo;
    public bool IsReloading => _isReloading;
    public bool IsAiming => _isAiming;

    // ── Unity Lifecycle ────────────────────────────────────────────────────
    private void Awake()
    {
        ResolveReferences();
        CacheWeaponVisualPose();
    }

    private void Start()
    {
        if (_data != null && _currentAmmo == 0 && _reserveAmmo == 0)
            Initialize(_data);
    }

    private void OnEnable()
    {
        if (_data != null)
            OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    private void OnDisable()
    {
        if (_isAiming)
            SetAiming(false);
        CancelReload();
    }

    private void Update()
    {
        if (_data == null) return;

        HandleInput();
        RecoverWeaponVisual();
    }

    // ── Init ───────────────────────────────────────────────────────────────
    public void Initialize(WeaponData data)
    {
        _data = data;

        if (_data == null)
        {
            Debug.LogError("[Gun] Initialize failed: WeaponData is null.");
            return;
        }

        ResolveReferences();
        CacheWeaponVisualPose();

        _visualKickPositionOffset = Vector3.zero;
        _visualKickEulerOffset = Vector3.zero;

        _currentAmmo = _data.maxAmmo;
        _reserveAmmo = _data.maxReserve;
        _isReloading = false;
        _isAiming = false;
        _nextFireTime = 0f;

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);

        if (_logInit)
            Debug.Log($"[Gun][{GetWeaponName()}] Init | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve}");
    }

    private void ResolveReferences()
    {
        if (_cam == null)
            _cam = Camera.main;

        if (_playerCamera == null && _cam != null)
            _playerCamera = _cam.GetComponent<PlayerCamera>();

        if (_weaponVisualRoot == null)
            _weaponVisualRoot = transform;

        if (_weaponAnimator == null)
            _weaponAnimator = GetComponentInChildren<Animator>();

        if (_gunBarrel == null)
        {
            Transform foundBarrel = transform.Find("GunBarrel");

            if (foundBarrel != null)
                _gunBarrel = foundBarrel;
        }
    }

    private void CacheWeaponVisualPose()
    {
        if (_weaponVisualRoot == null) return;

        _visualDefaultLocalPosition = _weaponVisualRoot.localPosition;
        _visualDefaultLocalRotation = _weaponVisualRoot.localRotation;
        _hasVisualPose = true;
    }

    // ── Input Handling ─────────────────────────────────────────────────────
    private void HandleInput()
    {
        HandleAimInput();

        if (_isReloading) return;

        bool shootInput = _data.isAutoFire
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (shootInput && Time.time >= _nextFireTime)
        {
            if (_currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                PlayEmptySound();
                _nextFireTime = Time.time + GetFireDelay();

                if (_logEmptyAmmo)
                    Debug.Log($"[Gun][{GetWeaponName()}] Empty | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve}");
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
            TryReload();
    }

    private void HandleAimInput()
    {
        bool aimInput = Input.GetMouseButton(1);

        if (aimInput != _isAiming)
            SetAiming(aimInput);
    }

    private void SetAiming(bool aiming)
    {
        _isAiming = aiming;

        float targetFOV = _data.isSniper ? _data.sniperFOV : _data.aimFOV;

        if (_playerCamera != null)
            _playerCamera.SetZoom(_isAiming, targetFOV);

        TrySetAnimatorBool("IsAiming", _isAiming);
    }

    // ── Shoot ──────────────────────────────────────────────────────────────
    private void Shoot()
    {
        if (_cam == null)
        {
            ResolveReferences();

            if (_cam == null)
            {
                Debug.LogError($"[Gun][{GetWeaponName()}] Shoot failed: Camera.main not found.");
                return;
            }
        }

        _currentAmmo--;
        _nextFireTime = Time.time + GetFireDelay();

        SpawnMuzzleFlash();
        PlayShootSound();

        if (_data.isShotgun)
            ShootShotgun();
        else
            ShootRaycast(_cam.transform.forward);

        ApplyWeaponFeedback();
        TrySetAnimatorTrigger("Fire");

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);

        if (_logAmmoOnShot)
            LogShotAmmo();

        if (_currentAmmo <= 0 && _reserveAmmo > 0)
        {
            TryReload();
        }
    }

    private void SpawnMuzzleFlash()
    {
        if (_muzzleFlashPrefab == null || _gunBarrel == null) return;

        GameObject flash = Instantiate(
            _muzzleFlashPrefab,
            _gunBarrel.position,
            _gunBarrel.rotation
        );

        flash.transform.localScale = Vector3.one * _muzzleFlashScale;

        Destroy(flash, _muzzleFlashDestroyDelay);
    }

    private void PlayShootSound()
    {
        AudioManager.Instance?.PlaySFX(_data.shootSound);
    }

    private void ShootRaycast(Vector3 direction)
    {
        Ray ray = new Ray(_cam.transform.position, direction);

        if (!Physics.Raycast(ray, out RaycastHit hit, _data.range))
            return;

        if (_logHitDebug)
        {
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            Debug.Log($"[Gun][{GetWeaponName()}][HitDebug] Hit: {hit.collider.name} | Layer: {layerName}");
        }

        EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(_data.damage);

            if (_logHitDebug)
                Debug.Log($"[Gun][{GetWeaponName()}][HitDebug] Enemy damaged: {enemyHealth.name} | Damage: {_data.damage}");
        }
    }

    private void ShootShotgun()
    {
        float spreadRadius = Mathf.Tan(_data.spreadAngle * Mathf.Deg2Rad);

        for (int i = 0; i < _data.pelletCount; i++)
        {
            Vector3 spreadDirection = _cam.transform.forward;

            spreadDirection += _cam.transform.right * UnityEngine.Random.Range(-spreadRadius, spreadRadius);
            spreadDirection += _cam.transform.up * UnityEngine.Random.Range(-spreadRadius, spreadRadius);

            ShootRaycast(spreadDirection.normalized);
        }
    }

    // ── Reload ─────────────────────────────────────────────────────────────
    private void TryReload()
    {
        if (_data == null)
        {
            Debug.LogError("[Gun] Reload blocked: WeaponData is null.");
            return;
        }

        if (_isReloading)
        {
            if (_logBlockedReload)
                Debug.Log($"[Gun][{GetWeaponName()}] Reload blocked: already reloading.");

            return;
        }

        if (_currentAmmo >= _data.maxAmmo)
        {
            if (_logBlockedReload)
                Debug.Log($"[Gun][{GetWeaponName()}] Reload blocked: magazine already full | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve}");

            return;
        }

        if (_reserveAmmo <= 0)
        {
            if (_logBlockedReload)
                Debug.Log($"[Gun][{GetWeaponName()}] Reload blocked: no reserve ammo | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve}");

            return;
        }

        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }

        reloadCoroutine = StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        _isReloading = true;

        if (_isAiming)
            SetAiming(false);

        TrySetAnimatorTrigger("Reload");
        AudioManager.Instance?.PlaySFX(_data.reloadSound);

        if (_logReload)
            Debug.Log($"[Gun][{GetWeaponName()}] Reload start | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve} | Wait: {_data.reloadTime}s");

        yield return new WaitForSeconds(_data.reloadTime);

        int neededAmmo = _data.maxAmmo - _currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, _reserveAmmo);

        _currentAmmo += ammoToLoad;
        _reserveAmmo -= ammoToLoad;

        _isReloading = false;
        reloadCoroutine = null;

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);

        if (_logReload)
            Debug.Log($"[Gun][{GetWeaponName()}] Reload done | Loaded: {ammoToLoad} | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve}");
    }

    // ── Weapon Feedback ────────────────────────────────────────────────────
    private void ApplyWeaponFeedback()
    {
        if (_playerCamera != null)
        {
            _playerCamera.AddRecoil(
                _data.verticalRecoil,
                _data.horizontalRecoil,
                _data.recoilRecoverSpeed
            );
        }

        ApplyWeaponVisualKick();
    }

    private void ApplyWeaponVisualKick()
    {
        if (!_hasVisualPose || _weaponVisualRoot == null || _data == null) return;

        float sideKick = UnityEngine.Random.Range(
            -_data.weaponKickSide,
            _data.weaponKickSide
        );

        _visualKickPositionOffset += Vector3.back * _data.weaponKickback;

        _visualKickEulerOffset += new Vector3(
            -_data.weaponKickUp,
            sideKick,
            0f
        );
    }

    private void RecoverWeaponVisual()
    {
        if (!_hasVisualPose || _weaponVisualRoot == null || _data == null) return;

        Vector3 basePosition = _visualDefaultLocalPosition;
        Quaternion baseRotation = _visualDefaultLocalRotation;

        float kickT = Mathf.Clamp01(_data.weaponReturnSpeed * Time.deltaTime);

        _visualKickPositionOffset = Vector3.Lerp(
            _visualKickPositionOffset,
            Vector3.zero,
            kickT
        );

        _visualKickEulerOffset = Vector3.Lerp(
            _visualKickEulerOffset,
            Vector3.zero,
            kickT
        );

        Vector3 finalPosition = basePosition + _visualKickPositionOffset;
        Quaternion finalRotation =
            baseRotation * Quaternion.Euler(_visualKickEulerOffset);

        float moveT = Mathf.Clamp01(_data.aimMoveSpeed * Time.deltaTime);

        _weaponVisualRoot.localPosition = Vector3.Lerp(
            _weaponVisualRoot.localPosition,
            finalPosition,
            moveT
        );

        _weaponVisualRoot.localRotation = Quaternion.Slerp(
            _weaponVisualRoot.localRotation,
            finalRotation,
            moveT
        );
    }

    // ── Refill Ammo ────────────────────────────────────────────────────────
    public void RefillAmmo()
    {
        if (_data == null) return;

        _currentAmmo = _data.maxAmmo;
        _reserveAmmo = _data.maxReserve;

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);

        if (_logReload)
            Debug.Log($"[Gun][{GetWeaponName()}] Ammo refilled | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve}");
    }

    // ── Debug Helpers ──────────────────────────────────────────────────────
    private void LogShotAmmo()
    {
        if (_data.isShotgun)
        {
            Debug.Log($"[Gun][{GetWeaponName()}] Shot | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve} | Pellets: {_data.pelletCount}");
            return;
        }

        Debug.Log($"[Gun][{GetWeaponName()}] Shot | Ammo: {_currentAmmo}/{_data.maxAmmo} | Reserve: {_reserveAmmo}/{_data.maxReserve}");
    }

    private string GetWeaponName()
    {
        if (_data == null || string.IsNullOrWhiteSpace(_data.weaponName))
            return gameObject.name;

        return _data.weaponName;
    }

    private float GetFireDelay()
    {
        if (_data == null || _data.fireRate <= 0f)
            return 0.1f;

        return 1f / _data.fireRate;
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private void PlayEmptySound()
    {
        AudioManager.Instance?.PlaySFX(_data.emptySound);
    }

    private void TrySetAnimatorTrigger(string parameterName)
    {
        if (!HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
            return;

        _weaponAnimator.SetTrigger(parameterName);
    }

    private void TrySetAnimatorBool(string parameterName, bool value)
    {
        if (!HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
            return;

        _weaponAnimator.SetBool(parameterName, value);
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (_weaponAnimator == null) return false;
        if (_weaponAnimator.runtimeAnimatorController == null) return false;

        foreach (AnimatorControllerParameter parameter in _weaponAnimator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
                return true;
        }

        return false;
    }
    public void CancelReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        _isReloading = false;
    }
}
