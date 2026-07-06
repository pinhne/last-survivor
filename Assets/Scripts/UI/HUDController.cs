using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD trong màn chơi — RULE §6.1, §6.4, §6.5.
/// Chỉ lắng nghe event, không poll Update().
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
    }

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged              += UpdateHPBar;
        PlayerHealth.OnShieldChanged              += UpdateShieldBar;
        LevelManager.OnTimerUpdated               += UpdateTimer;
        LevelManager.OnEnemyCountChanged          += UpdateEnemyText;
        EconomyManager.OnMoneyChanged             += UpdateMoneyText;
        Gun.OnAmmoChanged                         += UpdateAmmoText;
        LevelManager.OnWaveIntermissionStateChanged += OnIntermissionStateChanged;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged              -= UpdateHPBar;
        PlayerHealth.OnShieldChanged              -= UpdateShieldBar;
        LevelManager.OnTimerUpdated               -= UpdateTimer;
        LevelManager.OnEnemyCountChanged          -= UpdateEnemyText;
        EconomyManager.OnMoneyChanged             -= UpdateMoneyText;
        Gun.OnAmmoChanged                         -= UpdateAmmoText;
        LevelManager.OnWaveIntermissionStateChanged -= OnIntermissionStateChanged;
    }

    private void OnIntermissionStateChanged(bool isIntermission)
    {
        _isIntermission = isIntermission;
    }

    private void UpdateHPBar(float currentHP, float maxHP)
    {
        if (_hpFill != null)
            _hpFill.fillAmount = maxHP > 0f ? currentHP / maxHP : 0f;

        if (_hpText != null)
            _hpText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
    }

    private void UpdateShieldBar(float currentShield, float maxShield)
    {
        if (_shieldFill != null)
            _shieldFill.fillAmount = maxShield > 0f ? currentShield / maxShield : 0f;

        if (_shieldText != null)
            _shieldText.text = $"{Mathf.CeilToInt(currentShield)} / {Mathf.CeilToInt(maxShield)}";
    }

    private void UpdateTimer(float timeRemaining)
    {
        if (_timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        _timerText.text = $"{minutes:00}:{seconds:00}";

        if (!_isIntermission && timeRemaining <= 30f)
            _timerText.color = Color.red;
        else
            _timerText.color = Color.white;
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
