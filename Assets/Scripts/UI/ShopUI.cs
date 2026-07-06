using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop phím B giữa combat (mode phụ) — pause Time.timeScale.
/// Shop chính giữa wave: IntermissionShopUI (RULE §7).
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

    private bool _isOpen;
    private bool _isIntermission;
    private Coroutine _feedbackRoutine;

    private void Awake()
    {
        ResolveWeaponRefs();

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        HideFeedback();

        if (_btnHpPotion != null) _btnHpPotion.onClick.AddListener(OnBuyHp);
        if (_btnShield != null) _btnShield.onClick.AddListener(OnBuyShield);
        if (_btnRifle != null) _btnRifle.onClick.AddListener(OnBuyRifle);
        if (_btnShotgun != null) _btnShotgun.onClick.AddListener(OnBuyShotgun);
        if (_btnSniper != null) _btnSniper.onClick.AddListener(OnBuySniper);

        if (_btnClose != null)
            _btnClose.onClick.AddListener(CloseShop);

        BindShopToggleButton();
    }

    private void OnEnable()
    {
        LevelManager.OnWaveIntermissionStateChanged += OnIntermissionStateChanged;
    }

    private void OnDisable()
    {
        LevelManager.OnWaveIntermissionStateChanged -= OnIntermissionStateChanged;
    }

    private void OnIntermissionStateChanged(bool isIntermission)
    {
        _isIntermission = isIntermission;
        if (isIntermission && _isOpen)
            CloseShop();
    }

    private void BindShopToggleButton()
    {
        if (_btnShopToggle == null)
        {
            var shopGo = transform.Find("Btn_Shop");
            if (shopGo != null)
                _btnShopToggle = shopGo.GetComponent<Button>();
            if (_btnShopToggle == null)
            {
                var found = GameObject.Find("Btn_Shop");
                if (found != null)
                    _btnShopToggle = found.GetComponent<Button>();
            }
        }

        if (_btnShopToggle != null)
            _btnShopToggle.onClick.AddListener(ToggleShop);
    }

    public void ToggleShop()
    {
        if (_isIntermission) return;
        if (_isOpen) CloseShop();
        else OpenShop();
    }

    private void Update()
    {
        if (_isIntermission) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (_isOpen) CloseShop();
            else OpenShop();
        }
    }

    private void OnBuyHp()
    {
        if (UIShopPurchase.PurchaseHpPotion(out var err))
            ShowSuccess("Mua HP Potion thành công!");
        else
            ShowError(err);
    }

    private void OnBuyShield()
    {
        if (UIShopPurchase.PurchaseShieldRecharge(out var err))
            ShowSuccess("Mua Shield thành công!");
        else
            ShowError(err);
    }

    private void OnBuyRifle()
    {
        if (UIShopPurchase.PurchaseWeapon(_rifleData, UIShopConstants.PriceRifle, out var err))
            ShowSuccess("Mua Rifle thành công!");
        else
            ShowError(err);
    }

    private void OnBuyShotgun()
    {
        if (UIShopPurchase.PurchaseWeapon(_shotgunData, UIShopConstants.PriceShotgun, out var err))
            ShowSuccess("Mua Shotgun thành công!");
        else
            ShowError(err);
    }

    private void OnBuySniper()
    {
        if (UIShopPurchase.PurchaseWeapon(_sniperData, UIShopConstants.PriceSniper, out var err))
            ShowSuccess("Mua Sniper thành công!");
        else
            ShowError(err);
    }

    public void OpenShop()
    {
        if (_isIntermission) return;

        _isOpen = true;
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(true);
            _shopPanel.transform.SetAsLastSibling();
        }
        Time.timeScale = 0f;
        HideFeedback();
    }

    public void CloseShop()
    {
        _isOpen = false;
        if (_shopPanel != null)
            _shopPanel.SetActive(false);
        Time.timeScale = 1f;
        HideFeedback();
    }

    private void ShowSuccess(string message)
        => UIShopFeedback.ShowSuccess(this, _errorText, ref _feedbackRoutine, message);

    private void ShowError(string message)
        => UIShopFeedback.ShowError(this, _errorText, ref _feedbackRoutine, message);

    private void HideFeedback()
        => UIShopFeedback.Hide(_errorText, this, ref _feedbackRoutine);

    private void ResolveWeaponRefs()
    {
        _rifleData   ??= Resources.Load<WeaponData>("Weapons/Weapon_Rifle");
        _shotgunData ??= Resources.Load<WeaponData>("Weapons/Weapon_Shotgun");
        _sniperData  ??= Resources.Load<WeaponData>("Weapons/Weapon_Sniper");
    }
}
