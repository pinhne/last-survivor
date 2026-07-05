using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public static event Action<int, int> OnAmmoChanged;

    [SerializeField] private WeaponData _data;
    [SerializeField] private Transform _gunBarrel;
    [SerializeField] private GameObject _muzzleFlashPrefab;

    private Camera _cam;
    private int _currentAmmo;
    private int _reserveAmmo;
    private bool _isReloading = false;
    private float _nextFireTime = 0f;
    private float _currentRecoil = 0f;

    public WeaponData Data => _data;
    public int CurrentAmmo => _currentAmmo;
    public int ReserveAmmo => _reserveAmmo;
    public bool IsReloading => _isReloading;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        if (_data == null) return;

        _currentAmmo = _data.maxAmmo;
        _reserveAmmo = _data.maxReserve;
        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    public void Initialize(WeaponData data)
    {
        _data = data;

        if (_data == null)
        {
            Debug.LogError("[Gun] Initialize failed: WeaponData is null");
            return;
        }

        _currentAmmo = _data.maxAmmo;
        _reserveAmmo = _data.maxReserve;
        _isReloading = false;

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
        Debug.Log($"[Gun] Init {_data.weaponName} | Ammo: {_currentAmmo}/{_reserveAmmo}");
    }

    private void OnEnable()
    {
        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    private void Update()
    {
        if (_data == null || _cam == null) return;

        HandleInput();
        HandleRecoilRecover();

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[Gun] Left click received");
        }
    }

    private void HandleInput()
    {
        if (_isReloading) return;

        bool shootInput = _data.isAutoFire
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (shootInput && Time.time >= _nextFireTime)
        {
            Debug.Log($"[Gun] Shoot! Ammo: {_currentAmmo}");

            if (_currentAmmo > 0)
                Shoot();
            else
                PlayEmptySound();
        }

        if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < _data.maxAmmo && _reserveAmmo > 0)
        {
            Debug.Log("[Gun] Reload!");
            StartCoroutine(Reload());
        }
    }

    private void Shoot()
    {
        _currentAmmo--;
        _nextFireTime = Time.time + (1f / _data.fireRate);

        if (_muzzleFlashPrefab != null && _gunBarrel != null)
        {
            GameObject fx = Instantiate(_muzzleFlashPrefab, _gunBarrel.position, _gunBarrel.rotation);
            Destroy(fx, 0.2f);
        }

        AudioManager.Instance?.PlaySFX(_data.shootSound);

        if (_data.isShotgun)
            ShootShotgun();
        else
            ShootRaycast(_cam.transform.forward);

        ApplyRecoil();

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);

        if (_currentAmmo <= 0 && _reserveAmmo > 0)
            StartCoroutine(Reload());
    }

    private void ShootRaycast(Vector3 direction)
    {
        Ray ray = new Ray(_cam.transform.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, _data.range))
        {
            Debug.Log($"[Gun] Hit: {hit.collider.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            int enemyLayer = LayerMask.NameToLayer("Enemy");

            if (hit.collider.gameObject.layer == enemyLayer)
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();

                if (enemyHealth != null)
                    enemyHealth.TakeDamage(_data.damage);
            }
        }
    }

    private void ShootShotgun()
    {
        for (int i = 0; i < _data.pelletCount; i++)
        {
            Vector3 spread = _cam.transform.forward;

            spread += new Vector3(
                UnityEngine.Random.Range(-_data.spreadAngle, _data.spreadAngle) * 0.01f,
                UnityEngine.Random.Range(-_data.spreadAngle, _data.spreadAngle) * 0.01f,
                0f
            );

            ShootRaycast(spread.normalized);
        }
    }

    private IEnumerator Reload()
    {
        _isReloading = true;

        Debug.Log($"[Gun] Reload start: {_data.weaponName}, wait {_data.reloadTime}s");

        yield return new WaitForSeconds(_data.reloadTime);

        int neededAmmo = _data.maxAmmo - _currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, _reserveAmmo);

        _currentAmmo += ammoToLoad;
        _reserveAmmo -= ammoToLoad;

        _isReloading = false;

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);

        Debug.Log($"[Gun] Reload done: {_data.weaponName} | Ammo: {_currentAmmo}/{_reserveAmmo}");
    }

    private void ApplyRecoil()
    {
        _currentRecoil += _data.recoilAmount;
        _cam.transform.localRotation *= Quaternion.Euler(-_data.recoilAmount, 0f, 0f);
    }

    private void HandleRecoilRecover()
    {
        if (_currentRecoil <= 0f) return;

        float prevRecoil = _currentRecoil;
        _currentRecoil = Mathf.Lerp(_currentRecoil, 0f, _data.recoilRecover * Time.deltaTime);

        if (_currentRecoil < 0.01f)
            _currentRecoil = 0f;

        float recovered = prevRecoil - _currentRecoil;
        _cam.transform.localRotation *= Quaternion.Euler(recovered, 0f, 0f);
    }

    private void ZoomIn()
    {
        _cam.fieldOfView = _data.sniperFOV;
    }

    private void ZoomOut()
    {
        _cam.fieldOfView = 60f;
    }

    public void RefillAmmo()
    {
        _currentAmmo = _data.maxAmmo;
        _reserveAmmo = _data.maxReserve;
        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    private void PlayEmptySound()
    {
        AudioManager.Instance?.PlaySFX(_data.emptySound);
    }
}