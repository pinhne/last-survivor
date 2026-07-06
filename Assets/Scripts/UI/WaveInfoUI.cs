using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel đợt quái — decorative, cập nhật progress khi số quái thay đổi.
/// </summary>
public class WaveInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _waveDetailText;
    [SerializeField] private Image _waveProgressFill;

    private void OnEnable()
    {
        LevelManager.OnEnemyCountChanged += OnEnemyCountChanged;
    }

    private void OnDisable()
    {
        LevelManager.OnEnemyCountChanged -= OnEnemyCountChanged;
    }

    private void Start()
    {
        if (_waveDetailText != null)
            _waveDetailText.text = "Wave 1/5 · Walker x3";
        SetProgress(0.2f);
    }

    private void OnEnemyCountChanged(int enemiesAlive)
    {
        if (_waveDetailText != null && enemiesAlive > 0)
            _waveDetailText.text = $"Wave · Enemies x{enemiesAlive}";

        float progress = enemiesAlive > 0 ? Mathf.Clamp01(1f - enemiesAlive / 10f) : 0.2f;
        SetProgress(Mathf.Max(0.2f, progress));
    }

    private void SetProgress(float amount)
    {
        if (_waveProgressFill != null)
            _waveProgressFill.fillAmount = amount;
    }
}
