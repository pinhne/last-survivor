using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop UI chính cho gameplay.
/// - Không mở tự do bằng phím B trong lúc combat.
/// - Tự hiện khi LevelManager báo vào intermission sau khi clear wave.
/// - Nút đóng trong intermission sẽ thử gọi SpawnManager.SkipIntermission().
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private TMP_Text _errorText;

    [Header("Buttons")]
    [SerializeField] private Button _btnShopToggle;
    [SerializeField] private Button _btnHpPotion;
    [SerializeField] private Button _btnShield;
    [SerializeField] private Button _btnRifle;
    [SerializeField] private Button _btnShotgun;
    [SerializeField] private Button _btnSniper;
    [SerializeField] private Button _btnClose;

    [Header("Weapon Data")]
    [SerializeField] private WeaponData _rifleData;
    [SerializeField] private WeaponData _shotgunData;
    [SerializeField] private WeaponData _sniperData;

    [Header("Behaviour")]
    [Tooltip("Tắt mặc định: shop chỉ mở khi nghỉ giữa wave. Bật nếu muốn mở shop tự do bằng nút/phím trong lúc combat.")]
    [SerializeField] private bool _allowCombatShop = false;

    [Tooltip("Chỉ dùng khi Allow Combat Shop bật.")]
    [SerializeField] private KeyCode _combatShopKey = KeyCode.B;

    [Tooltip("Trong intermission, bấm nút Đóng/Tiếp tục sẽ gọi SkipIntermission nếu SpawnManager có hàm này.")]
    [SerializeField] private bool _closeButtonSkipsIntermission = true;

    [Header("Readable UI")]
    [SerializeField] private float _shopPanelScale = 1.25f;
    [SerializeField] private float _minimumTextSize = 18f;
    [SerializeField] private float _minimumTitleTextSize = 30f;
    [SerializeField] private float _minimumButtonHeight = 42f;

    private bool _isOpen;
    private bool _isIntermission;
    private Coroutine _feedbackRoutine;

    private void Awake()
    {
        ResolveWeaponRefs();
        ApplyReadableSizing();

        if (_shopPanel != null)
        {
            _shopPanel.SetActive(false);
        }

        HideFeedback();

        if (_btnHpPotion != null) _btnHpPotion.onClick.AddListener(OnBuyHp);
        if (_btnShield != null) _btnShield.onClick.AddListener(OnBuyShield);
        if (_btnRifle != null) _btnRifle.onClick.AddListener(OnBuyRifle);
        if (_btnShotgun != null) _btnShotgun.onClick.AddListener(OnBuyShotgun);
        if (_btnSniper != null) _btnSniper.onClick.AddListener(OnBuySniper);

        if (_btnClose != null)
        {
            _btnClose.onClick.AddListener(CloseShop);
        }

        BindShopToggleButton();
        SetShopToggleVisible(_allowCombatShop);
    }

    private void OnEnable()
    {
        LevelManager.OnWaveIntermissionStateChanged += OnIntermissionStateChanged;
    }

    private void OnDisable()
    {
        LevelManager.OnWaveIntermissionStateChanged -= OnIntermissionStateChanged;
    }

    private void Update()
    {
        // Không cho mở shop bằng B khi đang combat, trừ khi bật option phụ trong Inspector.
        if (!_allowCombatShop || _isIntermission)
        {
            return;
        }

        if (Input.GetKeyDown(_combatShopKey))
        {
            ToggleShop();
        }
    }

    private void OnIntermissionStateChanged(bool isIntermission)
    {
        _isIntermission = isIntermission;

        if (_isIntermission)
        {
            OpenIntermissionShop();
        }
        else
        {
            CloseShopInternal(restoreTimeScale: false, lockCursor: true);
        }
    }

    private void BindShopToggleButton()
    {
        if (_btnShopToggle == null)
        {
            Transform shopGo = transform.Find("Btn_Shop");
            if (shopGo != null)
            {
                _btnShopToggle = shopGo.GetComponent<Button>();
            }

            if (_btnShopToggle == null)
            {
                GameObject found = GameObject.Find("Btn_Shop");
                if (found != null)
                {
                    _btnShopToggle = found.GetComponent<Button>();
                }
            }
        }

        if (_btnShopToggle != null)
        {
            _btnShopToggle.onClick.AddListener(ToggleShop);
        }
    }

    private void SetShopToggleVisible(bool visible)
    {
        if (_btnShopToggle != null)
        {
            _btnShopToggle.gameObject.SetActive(visible);
        }
    }

    public void ToggleShop()
    {
        if (_isIntermission)
        {
            // Trong intermission shop tự hiện, không dùng toggle.
            return;
        }

        if (!_allowCombatShop)
        {
            return;
        }

        if (_isOpen)
        {
            CloseShopInternal(restoreTimeScale: true, lockCursor: true);
        }
        else
        {
            OpenCombatShop();
        }
    }

    public void OpenShop()
    {
        // Giữ hàm public để không vỡ reference cũ trong UnityEvent.
        if (_isIntermission)
        {
            OpenIntermissionShop();
            return;
        }

        if (_allowCombatShop)
        {
            OpenCombatShop();
        }
    }

    private void OpenCombatShop()
    {
        _isOpen = true;

        if (_shopPanel != null)
        {
            _shopPanel.SetActive(true);
            _shopPanel.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
        UnlockCursor();
        HideFeedback();
    }

    private void OpenIntermissionShop()
    {
        _isOpen = true;
        SetShopToggleVisible(false);

        if (_shopPanel != null)
        {
            _shopPanel.SetActive(true);
            _shopPanel.transform.SetAsLastSibling();
        }

        // Không pause Time.timeScale ở intermission để countdown/wave system không bị kẹt.
        UnlockCursor();
        HideFeedback();

        Debug.Log("[ShopUI] Intermission started -> Shop opened.");
    }

    public void CloseShop()
    {
        if (_isIntermission)
        {
            if (_closeButtonSkipsIntermission)
            {
                TrySkipIntermission();
            }

            // Đóng panel ngay để người chơi thấy phản hồi. LevelManager/SpawnManager sẽ bắn event false sau đó nếu có.
            CloseShopInternal(restoreTimeScale: false, lockCursor: true);
            return;
        }

        CloseShopInternal(restoreTimeScale: true, lockCursor: true);
    }

    private void CloseShopInternal(bool restoreTimeScale, bool lockCursor)
    {
        _isOpen = false;

        if (_shopPanel != null)
        {
            _shopPanel.SetActive(false);
        }

        if (restoreTimeScale)
        {
            Time.timeScale = 1f;
        }

        if (lockCursor)
        {
            LockCursor();
        }

        HideFeedback();
    }

    private void OnBuyHp()
    {
        if (UIShopPurchase.PurchaseHpPotion(out string err))
        {
            ShowSuccess("Mua HP Potion thành công!");
        }
        else
        {
            ShowError(err);
        }
    }

    private void OnBuyShield()
    {
        if (UIShopPurchase.PurchaseShieldRecharge(out string err))
        {
            ShowSuccess("Mua Shield thành công!");
        }
        else
        {
            ShowError(err);
        }
    }

    private void OnBuyRifle()
    {
        if (UIShopPurchase.PurchaseWeapon(_rifleData, UIShopConstants.PriceRifle, out string err))
        {
            ShowSuccess("Mua Rifle thành công!");
        }
        else
        {
            ShowError(err);
        }
    }

    private void OnBuyShotgun()
    {
        if (UIShopPurchase.PurchaseWeapon(_shotgunData, UIShopConstants.PriceShotgun, out string err))
        {
            ShowSuccess("Mua Shotgun thành công!");
        }
        else
        {
            ShowError(err);
        }
    }

    private void OnBuySniper()
    {
        if (UIShopPurchase.PurchaseWeapon(_sniperData, UIShopConstants.PriceSniper, out string err))
        {
            ShowSuccess("Mua Sniper thành công!");
        }
        else
        {
            ShowError(err);
        }
    }

    private void ShowSuccess(string message)
    {
        UIShopFeedback.ShowSuccess(this, _errorText, ref _feedbackRoutine, message);
    }

    private void ShowError(string message)
    {
        UIShopFeedback.ShowError(this, _errorText, ref _feedbackRoutine, message);
    }

    private void HideFeedback()
    {
        UIShopFeedback.Hide(_errorText, this, ref _feedbackRoutine);
    }

    private void ResolveWeaponRefs()
    {
        // Ưu tiên asset đã gán trong Inspector.
        // Resources chỉ là fallback, không nên phụ thuộc nếu nhóm đã có WeaponData ở ScriptableObjects/Weapons.
        _rifleData ??= Resources.Load<WeaponData>("Weapons/Weapon_Rifle");
        _shotgunData ??= Resources.Load<WeaponData>("Weapons/Weapon_Shotgun");
        _sniperData ??= Resources.Load<WeaponData>("Weapons/Weapon_Sniper");
    }

    private void ApplyReadableSizing()
    {
        if (_shopPanel == null)
        {
            return;
        }

        if (_shopPanelScale > 0f)
        {
            _shopPanel.transform.localScale = Vector3.one * _shopPanelScale;
        }

        foreach (TMP_Text text in _shopPanel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text == null)
            {
                continue;
            }

            bool looksLikeTitle = text.name.ToLowerInvariant().Contains("title")
                                  || text.text.Contains("CỬA HÀNG")
                                  || text.text.Contains("SHOP");

            float minSize = looksLikeTitle ? _minimumTitleTextSize : _minimumTextSize;
            if (text.fontSize < minSize)
            {
                text.fontSize = minSize;
            }

            text.enableWordWrapping = false;
        }

        foreach (Button button in _shopPanel.GetComponentsInChildren<Button>(true))
        {
            RectTransform rt = button.GetComponent<RectTransform>();
            if (rt != null && rt.sizeDelta.y < _minimumButtonHeight)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, _minimumButtonHeight);
            }
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool TrySkipIntermission()
    {
        try
        {
            Type spawnManagerType = Type.GetType("SpawnManager")
                                    ?? typeof(MonoBehaviour).Assembly.GetType("SpawnManager");

            if (spawnManagerType == null)
            {
                Debug.LogWarning("[ShopUI] Không tìm thấy SpawnManager type để SkipIntermission.");
                return false;
            }

            object instance = null;

            PropertyInfo instanceProperty = spawnManagerType.GetProperty(
                "Instance",
                BindingFlags.Public | BindingFlags.Static
            );

            if (instanceProperty != null)
            {
                instance = instanceProperty.GetValue(null);
            }

            if (instance == null)
            {
                FieldInfo instanceField = spawnManagerType.GetField(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static
                );

                if (instanceField != null)
                {
                    instance = instanceField.GetValue(null);
                }
            }

            if (instance == null)
            {
                Debug.LogWarning("[ShopUI] SpawnManager.Instance chưa sẵn sàng, không gọi SkipIntermission được.");
                return false;
            }

            MethodInfo skipMethod = spawnManagerType.GetMethod(
                "SkipIntermission",
                BindingFlags.Public | BindingFlags.Instance
            );

            if (skipMethod == null)
            {
                Debug.LogWarning("[ShopUI] SpawnManager không có hàm public SkipIntermission().");
                return false;
            }

            skipMethod.Invoke(instance, null);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ShopUI] Gọi SkipIntermission thất bại: " + ex.Message);
            return false;
        }
    }
}
