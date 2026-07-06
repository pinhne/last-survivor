using UnityEngine;

/// <summary>
/// Đặt RectTransform theo góc màn hình — tránh mất góc (pivot đúng).
/// </summary>
public static class UIAnchorUtil
{
    public static void SetRect(RectTransform rt, float anchorX, float anchorY,
        float pivotX, float pivotY, float posX, float posY, float width, float height)
    {
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(anchorX, anchorY);
        rt.pivot = new Vector2(pivotX, pivotY);
        rt.anchoredPosition = new Vector2(posX, posY);
        rt.sizeDelta = new Vector2(width, height);
    }

    public static void TopLeft(RectTransform rt, float left, float top, float w, float h)
        => SetRect(rt, 0f, 1f, 0f, 1f, left, -top, w, h);

    public static void TopRight(RectTransform rt, float right, float top, float w, float h)
        => SetRect(rt, 1f, 1f, 1f, 1f, -right, -top, w, h);

    public static void TopCenter(RectTransform rt, float top, float w, float h)
        => SetRect(rt, 0.5f, 1f, 0.5f, 1f, 0f, -top, w, h);

    public static void BottomLeft(RectTransform rt, float left, float bottom, float w, float h)
        => SetRect(rt, 0f, 0f, 0f, 0f, left, bottom, w, h);

    public static void BottomRight(RectTransform rt, float right, float bottom, float w, float h)
        => SetRect(rt, 1f, 0f, 1f, 0f, -right, bottom, w, h);

    public static void Center(RectTransform rt, float w, float h)
        => SetRect(rt, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 0f, w, h);

    public static void StretchWithPadding(RectTransform rt, float pad)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(pad, pad);
        rt.offsetMax = new Vector2(-pad, -pad);
    }
}
