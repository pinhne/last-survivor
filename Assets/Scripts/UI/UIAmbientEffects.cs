using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiệu ứng ambient: glow pulse, scanline, ember particles (UI).
/// </summary>
public class UIAmbientEffects : MonoBehaviour
{
    [SerializeField] private Color _accentColor = new(1f, 0.55f, 0f, 0.35f);
    [SerializeField] private int _emberCount = 18;

    private Image _glow;
    private RectTransform _scanRoot;
    private readonly List<RectTransform> _embers = new();
    private readonly List<float> _emberSpeeds = new();
    private float _time;

    public void Setup(Color accent, int embers = 18)
    {
        _accentColor = accent;
        _emberCount = embers;
        Build();
    }

    private void Build()
    {
        var root = transform;

        if (transform.Find("FX_GlowPulse") == null)
        {
            var glowGo = new GameObject("FX_GlowPulse", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowGo.transform.SetParent(root, false);
            var rt = glowGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            _glow = glowGo.GetComponent<Image>();
            _glow.sprite = UIProceduralTextures.CreateVignetteRing(512);
            _glow.color = _accentColor;
            _glow.raycastTarget = false;
            glowGo.transform.SetAsFirstSibling();
        }

        if (transform.Find("FX_Scanlines") == null)
        {
            var scanGo = new GameObject("FX_Scanlines", typeof(RectTransform));
            scanGo.transform.SetParent(root, false);
            _scanRoot = scanGo.GetComponent<RectTransform>();
            _scanRoot.anchorMin = Vector2.zero;
            _scanRoot.anchorMax = Vector2.one;
            _scanRoot.offsetMin = _scanRoot.offsetMax = Vector2.zero;
            for (int i = 0; i < 40; i++)
            {
                var line = new GameObject($"Line{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                line.transform.SetParent(_scanRoot, false);
                var lrt = line.GetComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0f, i / 40f);
                lrt.anchorMax = new Vector2(1f, i / 40f);
                lrt.sizeDelta = new Vector2(0f, 1f);
                line.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.07f);
                line.GetComponent<Image>().raycastTarget = false;
            }
            scanGo.transform.SetSiblingIndex(1);
        }

        if (transform.Find("FX_Embers") == null && _emberCount > 0)
        {
            var emberRoot = new GameObject("FX_Embers", typeof(RectTransform));
            emberRoot.transform.SetParent(root, false);
            var ert = emberRoot.GetComponent<RectTransform>();
            ert.anchorMin = Vector2.zero;
            ert.anchorMax = Vector2.one;
            ert.offsetMin = ert.offsetMax = Vector2.zero;

            for (int i = 0; i < _emberCount; i++)
            {
                var e = new GameObject($"Ember{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                e.transform.SetParent(emberRoot.transform, false);
                var img = e.GetComponent<Image>();
                img.color = new Color(_accentColor.r, _accentColor.g, _accentColor.b, Random.Range(0.15f, 0.45f));
                img.raycastTarget = false;
                var rt = e.GetComponent<RectTransform>();
                float s = Random.Range(2f, 6f);
                rt.sizeDelta = new Vector2(s, s);
                rt.anchoredPosition = new Vector2(Random.Range(-900f, 900f), Random.Range(-500f, 500f));
                _embers.Add(rt);
                _emberSpeeds.Add(Random.Range(12f, 40f));
            }
        }
    }

    private void Update()
    {
        _time += Time.unscaledDeltaTime;

        if (_glow != null)
        {
            float pulse = 0.5f + Mathf.Sin(_time * 1.2f) * 0.5f;
            var c = _accentColor;
            c.a = _accentColor.a * (0.6f + pulse * 0.4f);
            _glow.color = c;
        }

        if (_scanRoot != null)
            _scanRoot.anchoredPosition = new Vector2(0f, (_time * 18f) % 24f);

        for (int i = 0; i < _embers.Count; i++)
        {
            var rt = _embers[i];
            var p = rt.anchoredPosition;
            p.y += _emberSpeeds[i] * Time.unscaledDeltaTime;
            p.x += Mathf.Sin(_time + i) * 8f * Time.unscaledDeltaTime;
            if (p.y > 560f) p.y = -560f;
            rt.anchoredPosition = p;
        }
    }
}
