using UnityEngine;

/// <summary>
/// Sửa layout intermission shop — chạy runtime cho scene cũ chưa rebuild.
/// </summary>
public static class IntermissionShopUILayout
{
    public static void Apply(GameObject intermissionPanel)
    {
        if (intermissionPanel == null) return;

        var inner = intermissionPanel.transform.Find("IntermissionInner");
        if (inner == null) return;

        var innerRt = inner.GetComponent<RectTransform>();
        if (innerRt != null)
            innerRt.sizeDelta = new Vector2(520f, 700f);

        SetText(inner, "Txt_IntermissionTitle", 0.94f, 40f);
        SetText(inner, "Txt_NextWave", 0.88f, 32f);
        SetText(inner, "Txt_IntermissionTimer", 0.80f, 56f);
        SetBar(inner, "Img_IntermissionFill", 0.72f);
        SetText(inner, "Txt_Money", 0.67f, 28f);
        SetText(inner, "Txt_Error", 0.62f, 28f);

        SetButton(inner, "Btn_BuyHP", 0.56f);
        SetButton(inner, "Btn_BuyShield", 0.49f);
        SetButton(inner, "Btn_BuyCurrentAmmo", 0.42f);
        SetButton(inner, "Btn_BuyAllAmmo", 0.35f);
        SetButton(inner, "Btn_BuyRifle", 0.28f);
        SetButton(inner, "Btn_BuyShotgun", 0.21f);
        SetButton(inner, "Btn_BuySniper", 0.14f);
        SetButton(inner, "Btn_Continue", 0.05f, 320f, 52f);
    }

    private static void SetText(Transform root, string name, float anchorY, float height)
    {
        var t = root.Find(name);
        if (t == null) return;
        var rt = t.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, anchorY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(480f, height);
    }

    private static void SetBar(Transform root, string name, float anchorY)
    {
        var t = root.Find(name);
        if (t == null) return;
        var rt = t.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, anchorY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(460f, 8f);
    }

    private static void SetButton(Transform root, string name, float anchorY, float width = 400f, float height = 40f)
    {
        var t = root.Find(name);
        if (t == null) return;
        var rt = t.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, anchorY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(width, height);
    }
}
