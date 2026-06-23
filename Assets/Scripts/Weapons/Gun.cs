using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    // ── Static Events (Thu Hà lắng nghe để update ammo UI) ─────────────────
    public static event Action<int, int> OnAmmoChanged; // (currentAmmo, reserve)

    // ── References ───────────────────────────────────────────────────────────
    [SerializeField] private WeaponData _data;
    [SerializeField] private Transform _gunBarrel;     // điểm spawn MuzzleFlash (Vy gắn vào)
    [SerializeField] private GameObject _muzzleFlashPrefab; // Vy tạo prefab, Bình kéo vào

    private Camera _cam;
    private int _currentAmmo;
    private int _reserveAmmo;
    private bool _isReloading = false;
    private float _nextFireTime = 0f;
    private float _currentRecoil = 0f;  // recoil hiện tại (degrees)

    // ── Properties ───────────────────────────────────────────────────────────
    public WeaponData Data => _data;
    public int CurrentAmmo => _currentAmmo;
    public int ReserveAmmo => _reserveAmmo;
    public bool IsReloading => _isReloading;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        _currentAmmo = _data.maxAmmo;
        _reserveAmmo = _data.maxReserve;
        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Gọi khi WeaponManager SetActive(true) — tự broadcast ammo hiện tại lên UI.
    /// Giải pháp thay thế cho việc WeaponManager gọi Gun.OnAmmoChanged trực tiếp (không hợp lệ trong C#).
    /// </summary>
    private void OnEnable()
    {
        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    private void Update()
    {
        HandleInput();
        HandleRecoilRecover();
    }

    // ── Input Handling ────────────────────────────────────────────────────────
    private void HandleInput()
    {
        if (_isReloading) return;

        bool shootInput = _data.isAutoFire
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (shootInput && Time.time >= _nextFireTime)
        {
            Debug.Log($"[Gun] Shoot! Ammo: {_currentAmmo}"); // ← thêm dòng này
            if (_currentAmmo > 0)
                Shoot();
            else
                PlayEmptySound();
        }

        if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < _data.maxAmmo && _reserveAmmo > 0)
        {
            Debug.Log("[Gun] Reload!"); // ← thêm dòng này
            StartCoroutine(Reload());
        }
    }

    // ── Shoot ─────────────────────────────────────────────────────────────────
    private void Shoot()
    {
        _currentAmmo--;
        _nextFireTime = Time.time + (1f / _data.fireRate);

        // Muzzle Flash (Vy tạo prefab)
        if (_muzzleFlashPrefab != null && _gunBarrel != null)
            Instantiate(_muzzleFlashPrefab, _gunBarrel.position, _gunBarrel.rotation);

        // Âm thanh bắn
        AudioManager.Instance?.PlaySFX(_data.shootSound);

        // Bắn đạn
        if (_data.isShotgun)
            ShootShotgun();
        else
            ShootRaycast(_cam.transform.forward);

        // Recoil
        ApplyRecoil();

        // Cập nhật UI
        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);

        // Tự động reload khi hết đạn trong băng
        if (_currentAmmo <= 0 && _reserveAmmo > 0)
            StartCoroutine(Reload());
    }

    /// <summary>
    /// Bắn 1 viên đạn raycast từ camera center theo hướng chỉ định.
    /// </summary>
    private void ShootRaycast(Vector3 direction)
    {
        Ray ray = new Ray(_cam.transform.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, _data.range))
        {
            Debug.Log($"[Gun] Hit: {hit.collider.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            // Dùng Layer thay vì Tag — an toàn hơn, đúng quy tắc nhóm
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (hit.collider.gameObject.layer == enemyLayer)
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(_data.damage);
            }
        }
    }

    /// <summary>
    /// Shotgun: bắn nhiều viên đạn cùng lúc theo góc tản.
    /// </summary>
    private void ShootShotgun()
    {
        for (int i = 0; i < _data.pelletCount; i++)
        {
            // Tính hướng ngẫu nhiên trong góc tản
            Vector3 spread = _cam.transform.forward;
            spread += new Vector3(
                UnityEngine.Random.Range(-_data.spreadAngle, _data.spreadAngle) * 0.01f,
                UnityEngine.Random.Range(-_data.spreadAngle, _data.spreadAngle) * 0.01f,
                0f
            );
            ShootRaycast(spread.normalized);
        }
    }

    // ── Reload ────────────────────────────────────────────────────────────────
    private IEnumerator Reload()
    {
        _isReloading = true;

        AudioManager.Instance?.PlaySFX(_data.reloadSound);

        yield return new WaitForSeconds(_data.reloadTime);

        // Tính số đạn cần nạp thêm
        int needed = _data.maxAmmo - _currentAmmo;
        int refill = Mathf.Min(needed, _reserveAmmo);

        _currentAmmo += refill;
        _reserveAmmo -= refill;

        _isReloading = false;

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    // ── Recoil ────────────────────────────────────────────────────────────────
    private void ApplyRecoil()
    {
        _currentRecoil += _data.recoilAmount;
        // Chỉ thêm offset, không override toàn bộ rotation
        _cam.transform.localRotation *= Quaternion.Euler(-_data.recoilAmount, 0f, 0f);
    }

    private void HandleRecoilRecover()
    {
        if (_currentRecoil <= 0f) return;

        float prevRecoil = _currentRecoil;
        _currentRecoil = Mathf.Lerp(_currentRecoil, 0f, _data.recoilRecover * Time.deltaTime);
        if (_currentRecoil < 0.01f) _currentRecoil = 0f;

        // Tính phần đã recover và cộng ngược lại cho camera
        float recovered = prevRecoil - _currentRecoil;
        _cam.transform.localRotation *= Quaternion.Euler(recovered, 0f, 0f);
    }

    // ── Sniper Zoom ───────────────────────────────────────────────────────────
    private void ZoomIn()
    {
        _cam.fieldOfView = _data.sniperFOV;
    }

    private void ZoomOut()
    {
        _cam.fieldOfView = 60f; // FOV mặc định
    }

    // ── Refill Ammo (Shop gọi khi mua thêm đạn) ─────────────────────────────
    public void RefillAmmo()
    {
        _currentAmmo = _data.maxAmmo;
        _reserveAmmo = _data.maxReserve;
        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void PlayEmptySound()
    {
        AudioManager.Instance?.PlaySFX(_data.emptySound);
    }
}