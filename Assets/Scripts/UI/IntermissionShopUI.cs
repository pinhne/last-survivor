using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// IntermissionShopPanel — RULE §4–§10.
/// Nghe SpawnManager, không dùng Time.timeScale.
/// </summary>
public class IntermissionShopUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _intermissionPanel;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _nextWaveText;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private Image _timerFill;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _errorText;

    [Header("Buttons")]
    [SerializeField] private Button _btnBuyHp;
    [SerializeField] private Button _btnBuyShield;
    [SerializeField] private Button _btnBuyCurrentAmmo;
    [SerializeField] private Button _btnBuyAllAmmo;
    [SerializeField] private Button _btnBuyRifle;
    [SerializeField] private Button _btnBuyShotgun;
    [SerializeField] private Button _btnBuySniper;
    [SerializeField] private Button _btnContinue;

    [Header("Prices")]
    [SerializeField] private int _hpPrice = UIShopConstants.PriceHpPotion;
    [SerializeField] private int _shieldPrice = UIShopConstants.PriceShieldRecharge;
    [SerializeField] private int _currentAmmoPrice = UIShopConstants.PriceCurrentAmmo;
    [SerializeField] private int _allAmmoPrice = UIShopConstants.PriceAllAmmo;
    [SerializeField] private int _riflePrice = UIShopConstants.PriceRifle;
    [SerializeField] private int _shotgunPrice = UIShopConstants.PriceShotgun;
    [SerializeField] private int _sniperPrice = UIShopConstants.PriceSniper;

    [Header("Weapon Data")]
    [SerializeField] private WeaponData _rifleData;
    [SerializeField] private WeaponData _shotgunData;
    [SerializeField] private WeaponData _sniperData;

    private float _intermissionDuration = SpawnManager.INTERMISSION_DURATION;
    private Coroutine _feedbackRoutine;

    private void Awake()
    {
        ResolveWeaponRefs();

        if (_intermissionPanel != null)
        {
            IntermissionShopUILayout.Apply(_intermissionPanel);
            _intermissionPanel.SetActive(false);
        }
        HideFeedback();

        BindButton(_btnBuyHp, OnBuyHp);
        BindButton(_btnBuyShield, OnBuyShield);
        BindButton(_btnBuyCurrentAmmo, OnBuyCurrentAmmo);
        BindButton(_btnBuyAllAmmo, OnBuyAllAmmo);
        BindButton(_btnBuyRifle, () => OnBuyWeapon(_rifleData, _riflePrice));
        BindButton(_btnBuyShotgun, () => OnBuyWeapon(_shotgunData, _shotgunPrice));
        BindButton(_btnBuySniper, () => OnBuyWeapon(_sniperData, _sniperPrice));

        if (_btnContinue != null)
            _btnContinue.onClick.AddListener(OnContinue);
    }

    private void OnEnable()
    {
        SpawnManager.OnWaveIntermissionStarted += ShowIntermissionPanel;
        SpawnManager.OnWaveIntermissionTimerUpdated += UpdateIntermissionTimer;
        SpawnManager.OnWaveIntermissionEnded += HideIntermissionPanel;
        EconomyManager.OnMoneyChanged += UpdateMoneyText;
    }

    private void OnDisable()
    {
        SpawnManager.OnWaveIntermissionStarted -= ShowIntermissionPanel;
        SpawnManager.OnWaveIntermissionTimerUpdated -= UpdateIntermissionTimer;
        SpawnManager.OnWaveIntermissionEnded -= HideIntermissionPanel;
        EconomyManager.OnMoneyChanged -= UpdateMoneyText;
    }

    private void ShowIntermissionPanel(int clearedWave, int nextWave, float duration)
    {
        _intermissionDuration = duration > 0f ? duration : SpawnManager.INTERMISSION_DURATION;

        if (_intermissionPanel != null)
            _intermissionPanel.SetActive(true);

        if (_titleText != null)
            _titleText.text = $"Wave {clearedWave} Cleared";
        if (_nextWaveText != null)
            _nextWaveText.text = $"Next Wave: {nextWave}";

        UpdateIntermissionTimer(_intermissionDuration);
        UpdateMoneyText(EconomyManager.Instance != null ? EconomyManager.Instance.CurrentMoney : 0);
        HideFeedback();
    }

    private void HideIntermissionPanel()
    {
        if (_intermissionPanel != null)
            _intermissionPanel.SetActive(false);
        HideFeedback();
    }

    private void UpdateIntermissionTimer(float remainingSeconds)
    {
        int sec = Mathf.CeilToInt(remainingSeconds);
        if (_timerText != null)
            _timerText.text = $"Time: {sec}";

        if (_timerFill != null && _intermissionDuration > 0f)
            _timerFill.fillAmount = Mathf.Clamp01(remainingSeconds / _intermissionDuration);
    }

    private void UpdateMoneyText(int currentMoney)
    {
        if (_moneyText != null)
            _moneyText.text = $"{currentMoney} xu";
    }

    private void OnContinue()
    {
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SkipIntermission();
    }

    private void OnBuyHp()
    {
        if (!UIShopPurchase.TrySpendMoney(_hpPrice, out var err))
        {
            ShowError(err);
            return;
        }
        UIShopPurchase.FindPlayerHealth()?.Heal(PlayerHealth.HP_POTION_HEAL);
        ShowSuccess("Mua HP Potion thành công!");
    }

    private void OnBuyShield()
    {
        if (!UIShopPurchase.TrySpendMoney(_shieldPrice, out var err))
        {
            ShowError(err);
            return;
        }
        UIShopPurchase.FindPlayerHealth()?.RechargeShield(PlayerHealth.SHIELD_RECHARGE_AMOUNT);
        ShowSuccess("Mua Shield thành công!");
    }

    private void OnBuyCurrentAmmo()
    {
        if (!UIShopPurchase.TrySpendMoney(_currentAmmoPrice, out var err))
        {
            ShowError(err);
            return;
        }
        UIShopPurchase.FindWeaponManager()?.RefillCurrentWeaponAmmo();
        ShowSuccess("Mua đạn thành công!");
    }

    private void OnBuyAllAmmo()
    {
        if (!UIShopPurchase.TrySpendMoney(_allAmmoPrice, out var err))
        {
            ShowError(err);
            return;
        }
        UIShopPurchase.FindWeaponManager()?.RefillAllUnlockedWeaponsAmmo();
        ShowSuccess("Mua đạn tất cả súng thành công!");
    }

    private void OnBuyWeapon(WeaponData data, int price)
    {
        if (UIShopPurchase.PurchaseWeapon(data, price, out var err))
            ShowSuccess($"Mua {data.displayName} thành công!");
        else
            ShowError(err);
    }

    private static void BindButton(Button button, System.Action action)
    {
        if (button == null || action == null) return;
        button.onClick.AddListener(() => action());
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
