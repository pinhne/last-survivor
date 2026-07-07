using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD trong màn chơi.
/// Bản sửa: update text + bar fill ổn định, tự tạo AmmoText nếu prefab chưa có.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image _hpFill;
    [SerializeField] private TMP_Text _hpText;

    [Header("Shield")]
    [SerializeField] private Image _shieldFill;
    [SerializeField] private TMP_Text _shieldText;

    [Header("Level Info")]
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _enemyCountText;

    [Header("Economy")]
    [SerializeField] private TMP_Text _moneyText;

    [Header("Ammo")]
    [SerializeField] private TMP_Text _ammoText;

    private bool _isIntermission;

    private void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            HUDUILayout.Apply(canvas.transform);

        ResolveMissingReferences();
        PrepareFillImage(_hpFill);
        PrepareFillImage(_shieldFill);
    }

    private void Start()
    {
        SyncInitialValues();
    }

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHPBar;
        PlayerHealth.OnShieldChanged += UpdateShieldBar;
        LevelManager.OnTimerUpdated += UpdateTimer;
        LevelManager.OnEnemyCountChanged += UpdateEnemyText;
        EconomyManager.OnMoneyChanged += UpdateMoneyText;
        Gun.OnAmmoChanged += UpdateAmmoText;
        LevelManager.OnWaveIntermissionStateChanged += OnIntermissionStateChanged;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHPBar;
        PlayerHealth.OnShieldChanged -= UpdateShieldBar;
        LevelManager.OnTimerUpdated -= UpdateTimer;
        LevelManager.OnEnemyCountChanged -= UpdateEnemyText;
        EconomyManager.OnMoneyChanged -= UpdateMoneyText;
        Gun.OnAmmoChanged -= UpdateAmmoText;
        LevelManager.OnWaveIntermissionStateChanged -= OnIntermissionStateChanged;
    }

    private void ResolveMissingReferences()
    {
        if (_ammoText == null)
            _ammoText = FindTMP("AmmoText") ?? CreateAmmoText();

        if (_timerText == null)
            _timerText = FindTMP("TimerText") ?? FindTMP("Text");

        if (_enemyCountText == null)
            _enemyCountText = FindTMP("EnemyCountText");

        if (_moneyText == null)
            _moneyText = FindTMP("MoneyText");

        if (_hpFill == null)
            _hpFill = FindImageByName("HpFill", "HPFill", "HealthFill", "Fill");

        if (_shieldFill == null)
            _shieldFill = FindImageByName("ShieldFill", "SHFill");
    }

    private TMP_Text FindTMP(string objectName)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text != null && text.name == objectName)
                return text;
        }

        return null;
    }

    private Image FindImageByName(params string[] names)
    {
        Image[] images = GetComponentsInChildren<Image>(true);

        foreach (string wantedName in names)
        {
            foreach (Image image in images)
            {
                if (image != null && image.name == wantedName)
                    return image;
            }
        }

        return null;
    }

    private TMP_Text CreateAmmoText()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform;

        GameObject go = new GameObject("AmmoText_Auto", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-42f, 42f);
        rt.sizeDelta = new Vector2(300f, 42f);

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = "Ammo: -- / --";
        text.fontSize = 24f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Right;
        text.color = Color.white;
        text.raycastTarget = false;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }

    private void PrepareFillImage(Image image)
    {
        if (image == null)
            return;

        image.raycastTarget = false;

        // Unity chỉ hiện Image Type khi Image có Source Image/Sprite.
        // Vì prefab của Hà dùng Image không có sprite, fillAmount có thể không render đúng.
        // Vì vậy bar sẽ được giảm bằng RectTransform trong SetBar().
        if (image.sprite != null)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillClockwise = true;
            image.fillAmount = 1f;
        }
    }

    private void SetBar(Image image, float current, float max)
    {
        if (image == null)
            return;

        float amount = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        PrepareFillImage(image);

        if (image.sprite != null)
            image.fillAmount = amount;

        // Cách chắc chắn cho UI Image không có Source Image: thu ngắn RectTransform.
        RectTransform rt = image.rectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(amount, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }
    }

    private void SyncInitialValues()
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            UpdateHPBar(playerHealth.CurrentHP, PlayerHealth.MAX_HP);
            UpdateShieldBar(playerHealth.CurrentShield, PlayerHealth.MAX_SHIELD);
        }

        if (LevelManager.Instance != null)
        {
            UpdateTimer(LevelManager.Instance.TimeRemaining);
            UpdateEnemyText(LevelManager.Instance.EnemiesAlive);
        }

        if (EconomyManager.Instance != null)
            UpdateMoneyText(EconomyManager.Instance.CurrentMoney);

        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null && weaponManager.CurrentGun != null)
            UpdateAmmoText(weaponManager.CurrentGun.CurrentAmmo, weaponManager.CurrentGun.ReserveAmmo);
    }

    private void OnIntermissionStateChanged(bool isIntermission)
    {
        _isIntermission = isIntermission;
    }

    private void UpdateHPBar(float currentHP, float maxHP)
    {
        SetBar(_hpFill, currentHP, maxHP);

        if (_hpText != null)
            _hpText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
    }

    private void UpdateShieldBar(float currentShield, float maxShield)
    {
        SetBar(_shieldFill, currentShield, maxShield);

        if (_shieldText != null)
            _shieldText.text = $"{Mathf.CeilToInt(currentShield)} / {Mathf.CeilToInt(maxShield)}";
    }

    private void UpdateTimer(float timeRemaining)
    {
        if (_timerText == null)
            return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        _timerText.text = $"{minutes:00}:{seconds:00}";

        _timerText.color = (!_isIntermission && timeRemaining <= 30f)
            ? Color.red
            : Color.white;
    }

    private void UpdateEnemyText(int enemiesAlive)
    {
        if (_enemyCountText != null)
            _enemyCountText.text = $"Enemies: {enemiesAlive}";
    }

    private void UpdateMoneyText(int currentMoney)
    {
        if (_moneyText != null)
            _moneyText.text = $"{currentMoney} xu";
    }

    private void UpdateAmmoText(int currentAmmo, int reserveAmmo)
    {
        if (_ammoText != null)
            _ammoText.text = $"Ammo: {currentAmmo} / {reserveAmmo}";
    }
}
