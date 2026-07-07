using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thanh máu Boss — ẩn mặc định, hiện khi OnBossAppeared.
/// Bản sửa: ép Image Fill đúng dạng Filled để thanh máu giảm theo current/max.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    private const float WARNING_DURATION = 2f;
    private const float DEFEATED_DURATION = 3f;

    [Header("Boss Bar")]
    [SerializeField] private GameObject _bossBarRoot;
    [SerializeField] private Image _bossHpFill;
    [SerializeField] private TMP_Text _bossNameText;

    [Header("Warning / Defeated")]
    [SerializeField] private GameObject _warningRoot;
    [SerializeField] private TMP_Text _warningText;

    private Coroutine _warningCoroutine;
    private Coroutine _defeatedCoroutine;
    private string _lastBossName;

    private void Awake()
    {
        ResolveMissingReferences();
        PrepareFillImage(_bossHpFill);
        HideAll();
    }

    private void Start()
    {
        SyncExistingBoss();
    }

    private void OnEnable()
    {
        BossHealth.OnBossAppeared += OnBossAppeared;
        BossHealth.OnBossHealthChanged += UpdateBossBar;
        BossHealth.OnBossDefeated += OnBossDefeated;
    }

    private void OnDisable()
    {
        BossHealth.OnBossAppeared -= OnBossAppeared;
        BossHealth.OnBossHealthChanged -= UpdateBossBar;
        BossHealth.OnBossDefeated -= OnBossDefeated;

        if (_warningCoroutine != null)
        {
            StopCoroutine(_warningCoroutine);
            _warningCoroutine = null;
        }

        if (_defeatedCoroutine != null)
        {
            StopCoroutine(_defeatedCoroutine);
            _defeatedCoroutine = null;
        }
    }

    private void ResolveMissingReferences()
    {
        if (_bossHpFill == null)
            _bossHpFill = FindImageByName("BossHpFill", "BossHPFill", "BossFill", "Fill");

        if (_bossNameText == null)
            _bossNameText = FindTMP("BossNameText");

        if (_warningText == null)
            _warningText = FindTMP("WarningText");
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

    private void PrepareFillImage(Image image)
    {
        if (image == null)
            return;

        image.raycastTarget = false;

        // Image không có Source Image/Sprite sẽ không hiện Image Type trong Inspector.
        // Khi đó fillAmount không đáng tin, nên SetBar() còn resize RectTransform.
        if (image.sprite != null)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillClockwise = true;
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

    private void SyncExistingBoss()
    {
        BossHealth boss = FindFirstObjectByType<BossHealth>();
        if (boss == null)
            return;

        UpdateBossBar(boss.CurrentHP, boss.MaxHP);
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
        _lastBossName = bossName;

        if (_defeatedCoroutine != null)
        {
            StopCoroutine(_defeatedCoroutine);
            _defeatedCoroutine = null;
        }

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
            {
                _warningText.color = (Mathf.FloorToInt(elapsed * 4f) % 2 == 0)
                    ? Color.red
                    : Color.white;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (_warningRoot != null)
            _warningRoot.SetActive(false);

        if (_bossBarRoot != null)
            _bossBarRoot.SetActive(true);

        if (_bossNameText != null)
            _bossNameText.text = bossName;

        SyncExistingBoss();

        _warningCoroutine = null;
    }

    private void UpdateBossBar(float current, float max)
    {
        SetBar(_bossHpFill, current, max);
    }

    private void OnBossDefeated()
    {
        if (_warningCoroutine != null)
        {
            StopCoroutine(_warningCoroutine);
            _warningCoroutine = null;
        }

        if (_bossBarRoot != null)
            _bossBarRoot.SetActive(false);

        if (_defeatedCoroutine != null)
            StopCoroutine(_defeatedCoroutine);

        _defeatedCoroutine = StartCoroutine(ShowDefeatedBanner());
    }

    private IEnumerator ShowDefeatedBanner()
    {
        if (_warningRoot != null)
            _warningRoot.SetActive(true);

        string bossLabel = string.IsNullOrWhiteSpace(_lastBossName)
            ? "BOSS"
            : _lastBossName.ToUpper();

        if (_warningText != null)
        {
            _warningText.text = $"ĐÃ TIÊU DIỆT\n{bossLabel}";
            _warningText.fontSize = 44f;
            _warningText.alignment = TextAlignmentOptions.Center;
        }

        float elapsed = 0f;
        while (elapsed < DEFEATED_DURATION)
        {
            if (_warningText != null)
            {
                _warningText.color = (Mathf.FloorToInt(elapsed * 3f) % 2 == 0)
                    ? UITheme.VictoryGreen
                    : Color.white;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (_warningRoot != null)
            _warningRoot.SetActive(false);

        if (_warningText != null)
            _warningText.fontSize = 44f;

        _defeatedCoroutine = null;
    }
}
