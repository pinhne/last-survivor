using TMPro;
using UnityEngine;

/// <summary>
/// Fade-in + scale title khi vào scene.
/// </summary>
public class UITitleIntroFX : MonoBehaviour
{
    [SerializeField] private float _duration = 0.9f;
    [SerializeField] private float _startScale = 0.88f;

    private TMP_Text[] _texts;
    private float _t;

    private void Awake()
    {
        _texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (var tmp in _texts)
        {
            if (tmp.name.Contains("Title") || tmp.text.Contains("LAST") || tmp.text.Contains("SURVIVOR")
                || tmp.text == "GAME OVER" || tmp.text == "HOÀN THÀNH" || tmp.text.Contains("CHỌN MÀN"))
            {
                var c = tmp.color;
                c.a = 0f;
                tmp.color = c;
                tmp.transform.localScale = Vector3.one * _startScale;
            }
        }
    }

    private void Update()
    {
        _t += Time.unscaledDeltaTime;
        float p = Mathf.Clamp01(_t / _duration);
        float ease = 1f - Mathf.Pow(1f - p, 3f);

        foreach (var tmp in _texts)
        {
            if (tmp.name.Contains("Title") || tmp.text.Contains("LAST") || tmp.text.Contains("SURVIVOR")
                || tmp.text == "GAME OVER" || tmp.text == "HOÀN THÀNH" || tmp.text.Contains("CHỌN MÀN"))
            {
                var c = tmp.color;
                c.a = ease;
                tmp.color = c;
                tmp.transform.localScale = Vector3.Lerp(Vector3.one * _startScale, Vector3.one, ease);
            }
        }

        if (p >= 1f) enabled = false;
    }
}
