using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop UI — mở bằng phím B hoặc nút Shop.
/// Pause game khi mở (Time.timeScale = 0), gọi EconomyManager để mua.
/// </summary>
public class ShopUI : MonoBehaviour
{
    public const int PRICE_HP_POTION       = 80;
    public const int PRICE_SHIELD_RECHARGE = 120;
    public const int PRICE_RIFLE           = 300;
    public const int PRICE_SHOTGUN         = 450;
    public const int PRICE_SNIPER          = 600;

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

    private bool _isOpen;

    private void Awake()
    {
        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_errorText != null)
            _errorText.gameObject.SetActive(false);

        BindButton(_btnHpPotion,  PRICE_HP_POTION,       OnBuyHpPotion);
        BindButton(_btnShield,    PRICE_SHIELD_RECHARGE, OnBuyShield);
        BindButton(_btnRifle,     PRICE_RIFLE,           OnBuyRifle);
        BindButton(_btnShotgun,   PRICE_SHOTGUN,         OnBuyShotgun);
        BindButton(_btnSniper,    PRICE_SNIPER,          OnBuySniper);

        if (_btnClose != null)
            _btnClose.onClick.AddListener(CloseShop);

        BindShopToggleButton();
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
        if (_isOpen) CloseShop();
        else OpenShop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (_isOpen) CloseShop();
            else OpenShop();
        }
    }

    private void BindButton(Button button, int price, System.Action onSuccess)
    {
        if (button == null) return;
        button.onClick.AddListener(() => TryPurchase(price, onSuccess));
    }

    private void TryPurchase(int price, System.Action onSuccess)
    {
        if (EconomyManager.Instance == null)
        {
            ShowError("Hệ thống kinh tế chưa sẵn sàng.");
            return;
        }

        if (!EconomyManager.Instance.SpendMoney(price))
        {
            ShowError("Không đủ tiền");
            return;
        }

        HideError();
        onSuccess?.Invoke();
    }

    private void OnBuyHpPotion()
    {
        ShopManager.Instance?.PurchaseHpPotion();
    }

    private void OnBuyShield()
    {
        ShopManager.Instance?.PurchaseShieldRecharge();
    }

    private void OnBuyRifle()
    {
        ShopManager.Instance?.PurchaseRifle();
    }

    private void OnBuyShotgun()
    {
        ShopManager.Instance?.PurchaseShotgun();
    }

    private void OnBuySniper()
    {
        ShopManager.Instance?.PurchaseSniper();
    }

    public void OpenShop()
    {
        _isOpen = true;
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(true);
            _shopPanel.transform.SetAsLastSibling();
        }
        Time.timeScale = 0f;
        HideError();
    }

    public void CloseShop()
    {
        _isOpen = false;
        if (_shopPanel != null)
            _shopPanel.SetActive(false);
        Time.timeScale = 1f;
        HideError();
    }

    private void ShowError(string message)
    {
        if (_errorText == null) return;
        _errorText.text = message;
        _errorText.gameObject.SetActive(true);
    }

    private void HideError()
    {
        if (_errorText != null)
            _errorText.gameObject.SetActive(false);
    }
}

