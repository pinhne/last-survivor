using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD trong màn chơi: HP, Shield, Timer, số quái còn sống, tiền.
/// Chỉ lắng nghe event — không poll trong Update().
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

    private void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            HUDUILayout.Apply(canvas.transform);
    }

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged     += UpdateHPBar;
        PlayerHealth.OnShieldChanged     += UpdateShieldBar;
        LevelManager.OnTimerUpdated      += UpdateTimer;
        LevelManager.OnEnemyCountChanged += UpdateEnemyCount;
        EconomyManager.OnMoneyChanged    += UpdateMoneyDisplay;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged     -= UpdateHPBar;
        PlayerHealth.OnShieldChanged     -= UpdateShieldBar;
        LevelManager.OnTimerUpdated      -= UpdateTimer;
        LevelManager.OnEnemyCountChanged -= UpdateEnemyCount;
        EconomyManager.OnMoneyChanged    -= UpdateMoneyDisplay;
    }

    private void UpdateHPBar(float current, float max)
    {
        if (_hpFill != null)
            _hpFill.fillAmount = max > 0f ? current / max : 0f;

        if (_hpText != null)
            _hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void UpdateShieldBar(float current, float max)
    {
        if (_shieldFill != null)
            _shieldFill.fillAmount = max > 0f ? current / max : 0f;

        if (_shieldText != null)
            _shieldText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void UpdateTimer(float timeRemaining)
    {
        if (_timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        _timerText.text = $"{minutes:00}:{seconds:00}";

        if (timeRemaining <= 30f)
            _timerText.color = Color.red;
        else
            _timerText.color = Color.white;
    }

    private void UpdateEnemyCount(int enemiesAlive)
    {
        if (_enemyCountText != null)
            _enemyCountText.text = $"Enemies: {enemiesAlive}";
    }

    private void UpdateMoneyDisplay(int currentMoney)
    {
        if (_moneyText != null)
            _moneyText.text = $"{currentMoney} xu";
    }
}

