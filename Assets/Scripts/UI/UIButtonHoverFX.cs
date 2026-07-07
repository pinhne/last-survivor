using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hover scale + glow pulse cho nút UI.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonHoverFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float _hoverScale = 1.06f;
    [SerializeField] private float _speed = 12f;

    private RectTransform _rt;
    private Vector3 _baseScale;
    private float _target = 1f;
    private float _current = 1f;
    private Outline _outline;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _baseScale = _rt.localScale;
        _outline = GetComponent<Outline>();
    }

    public void OnPointerEnter(PointerEventData eventData) => _target = _hoverScale;
    public void OnPointerExit(PointerEventData eventData) => _target = 1f;

    private void Update()
    {
        _current = Mathf.Lerp(_current, _target, Time.unscaledDeltaTime * _speed);
        _rt.localScale = _baseScale * _current;

        if (_outline != null)
        {
            float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 4f) * 0.5f;
            var c = _outline.effectColor;
            c.a = Mathf.Lerp(0.35f, 0.85f, pulse) * (_target > 1f ? 1f : 0.5f);
            _outline.effectColor = c;
        }
    }
}
