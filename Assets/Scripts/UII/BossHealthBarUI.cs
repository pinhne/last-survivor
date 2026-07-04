
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thanh máu Boss — ẩn mặc định, hiện khi OnBossAppeared,
/// cảnh báo chớp đỏ 2 giây rồi mới hiện thanh máu.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    private const float WARNING_DURATION = 2f;

    [Header("Boss Bar")]
    [SerializeField] private GameObject _bossBarRoot;
    [SerializeField] private Image _bossHpFill;
    [SerializeField] private TMP_Text _bossNameText;

    [Header("Warning")]
    [SerializeField] private GameObject _warningRoot;
    [SerializeField] private TMP_Text _warningText;

    private Coroutine _warningCoroutine;

    private void Awake()
    {
        HideAll();
    }

    private void OnEnable()
    {
        BossHealth.OnBossAppeared      += OnBossAppeared;
        BossHealth.OnBossHealthChanged += UpdateBossBar;
        BossHealth.OnBossDefeated      += OnBossDefeated;
    }

    private void OnDisable()
    {
        BossHealth.OnBossAppeared      -= OnBossAppeared;
        BossHealth.OnBossHealthChanged -= UpdateBossBar;
        BossHealth.OnBossDefeated      -= OnBossDefeated;

        if (_warningCoroutine != null)
        {
            StopCoroutine(_warningCoroutine);
            _warningCoroutine = null;
        }
    }

    private void HideAll()
    {
        if (_bossBarRoot != null)
            _bossBarRoot.SetActive(false);

        if (_warningRoot != null)
            _warningRoot.SetActive(false);
    }

    private void OnBossAppeared(string bossName)
    {
        if (_warningCoroutine != null)
            StopCoroutine(_warningCoroutine);

        _warningCoroutine = StartCoroutine(ShowWarningThenBar(bossName));
    }

    private IEnumerator ShowWarningThenBar(string bossName)
    {
        if (_bossBarRoot != null)
            _bossBarRoot.SetActive(false);

        if (_warningRoot != null)
            _warningRoot.SetActive(true);

        if (_warningText != null)
            _warningText.text = $"{bossName.ToUpper()} INCOMING";

        float elapsed = 0f;
        while (elapsed < WARNING_DURATION)
        {
            if (_warningText != null)
                _warningText.color = (Mathf.FloorToInt(elapsed * 4f) % 2 == 0)
                    ? Color.red
                    : Color.white;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (_warningRoot != null)
            _warningRoot.SetActive(false);

        if (_bossBarRoot != null)
            _bossBarRoot.SetActive(true);

        if (_bossNameText != null)
            _bossNameText.text = bossName;

        _warningCoroutine = null;
    }

    private void UpdateBossBar(float current, float max)
    {
        if (_bossHpFill != null)
            _bossHpFill.fillAmount = max > 0f ? current / max : 0f;
    }

    private void OnBossDefeated()
    {
        if (_warningCoroutine != null)
        {
            StopCoroutine(_warningCoroutine);
            _warningCoroutine = null;
        }

        HideAll();
    }
}
