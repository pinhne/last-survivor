using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bố cục HUD khớp HTML (.hud-top-right, .hud-bottom-left, .wave-info, …).
/// </summary>
public static class HUDUILayout
{
    public static void Apply(Transform canvasRoot)
    {
        if (canvasRoot == null) return;

        FixCanvas(canvasRoot);

        // Wave — top-left (48px from top)
        LayoutWave(canvasRoot);
        LayoutLevelBadge(canvasRoot);
        LayoutBossBar(canvasRoot);
        LayoutHudPanel(canvasRoot);
        LayoutShopButton(canvasRoot);
        StyleHudChips(canvasRoot);
        HideDemoHelpOverlap();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == MenuController.SCENE_UI_TEST)
            ApplyUITestHudOffset(canvasRoot);
    }

    /// <summary>Đẩy chip HUD sang trái để không đè panel test bên phải.</summary>
    public static void ApplyUITestHudOffset(Transform canvasRoot)
    {
        var hud = canvasRoot.Find("HUDPanel");
        if (hud == null) return;

        const float extraRight = 300f;
        float right = UITheme.MarginH + extraRight;
        float y = UITheme.MarginTop;
        LayoutChip(hud, "TimerChip", right, ref y, 180f, 48f, 24f);
        y += 8f;
        LayoutChip(hud, "EnemyChip", right, ref y, 200f, 36f, 18f);
        y += 8f;
        LayoutChip(hud, "MoneyChip", right, ref y, 160f, 36f, 18f);
        y += 8f;
        LayoutChip(hud, "AmmoChip", right, ref y, 140f, 36f, 18f);

        // Nút SHOP [B] cũng dời sang trái + nâng lên để không bị panel che / hụt đáy
        var shop = canvasRoot.Find("Btn_Shop");
        if (shop != null)
            UIAnchorUtil.BottomRight(shop.GetComponent<RectTransform>(),
                UITheme.MarginH + extraRight, UITheme.MarginH + 24f, 170f, 44f);
    }

    private static void FixCanvas(Transform root)
    {
        var canvas = root.GetComponent<Canvas>();
        if (canvas == null) canvas = root.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var rt = canvas.GetComponent<RectTransform>();
        if (rt != null && rt.localScale == Vector3.zero)
            rt.localScale = Vector3.one;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UITheme.RefWidth, UITheme.RefHeight);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static void LayoutWave(Transform root)
    {
        var wave = root.Find("WaveInfo");
        if (wave == null) return;
        UIAnchorUtil.TopLeft(wave.GetComponent<RectTransform>(), UITheme.MarginH, 48f, 230f, 88f);
    }

    private static void LayoutLevelBadge(Transform root)
    {
        var badge = root.Find("LevelBadge");
        if (badge == null) return;
        UIAnchorUtil.TopCenter(badge.GetComponent<RectTransform>(), 16f, 480f, 32f);
    }

    private static void LayoutBossBar(Transform root)
    {
        var bossUi = root.Find("BossUI");
        if (bossUi == null) return;
        var bar = bossUi.Find("BossBarPanel");
        if (bar == null) return;
        UIAnchorUtil.TopCenter(bar.GetComponent<RectTransform>(), 60f, 500f, 64f);
    }

    private static void LayoutHudPanel(Transform root)
    {
        var hud = root.Find("HUDPanel");
        if (hud == null) return;

        var hudRt = hud.GetComponent<RectTransform>();
        if (hudRt != null)
        {
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero;
            hudRt.offsetMax = Vector2.zero;
        }

        // Top-right stack — HTML .hud-top-right { top:20, right:24, gap:8 }
        float right = UITheme.MarginH;
        float y = UITheme.MarginTop;
        LayoutChip(hud, "TimerChip", right, ref y, 180f, 48f, 24f);
        y += 8f;
        LayoutChip(hud, "EnemyChip", right, ref y, 200f, 36f, 18f);
        y += 8f;
        LayoutChip(hud, "MoneyChip", right, ref y, 160f, 36f, 18f);

        // Bottom-left — HTML .hud-bottom-left { bottom:24, left:24, width:320 }
        var bottom = hud.Find("BottomLeft");
        if (bottom != null)
            UIAnchorUtil.BottomLeft(bottom.GetComponent<RectTransform>(), UITheme.MarginH, UITheme.MarginH, 340f, 100f);
    }

    private static void LayoutChip(Transform hud, string name, float right, ref float y, float w, float h, float fontSize)
    {
        var t = hud.Find(name);
        if (t == null) return;
        UIAnchorUtil.TopRight(t.GetComponent<RectTransform>(), right, y, w, h);
        y += h;

        var tmp = t.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = false;
            tmp.alignment = TextAlignmentOptions.Right;
        }
    }

    private static void LayoutShopButton(Transform root)
    {
        var shop = root.Find("Btn_Shop");
        if (shop == null) return;
        UIAnchorUtil.BottomRight(shop.GetComponent<RectTransform>(), UITheme.MarginH, UITheme.MarginH, 170f, 44f);

        var tmp = shop.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = "SHOP [B]";
            tmp.color = UITheme.Gold;
            tmp.fontSize = 14f;
        }

        var img = shop.GetComponent<Image>();
        if (img != null)
            img.color = new Color(0f, 0f, 0f, 0.8f);
    }

    private static void StyleHudChips(Transform root)
    {
        var hud = root.Find("HUDPanel");
        if (hud == null) return;

        StyleChip(hud, "TimerChip", UITheme.Orange, UITheme.Orange);
        StyleChip(hud, "EnemyChip", UITheme.Orange, UITheme.Orange);
        StyleChip(hud, "MoneyChip", UITheme.Gold, UITheme.Gold);

        StyleStatRow(hud, "BottomLeft/HPRow", UITheme.HpGreen);
        StyleStatRow(hud, "BottomLeft/ShieldRow", UITheme.ShieldBlue);
    }

    private static void StyleChip(Transform hud, string path, Color accent, Color border)
    {
        var chip = hud.Find(path);
        if (chip == null) return;

        var img = chip.GetComponent<Image>();
        if (img != null) img.color = new Color(0.04f, 0.04f, 0.06f, 0.82f);

        var stripe = chip.Find("AccentStripe");
        if (stripe != null)
        {
            var sImg = stripe.GetComponent<Image>();
            if (sImg != null) sImg.color = border;
        }

        var tmp = chip.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
        }
    }

    private static void StyleStatRow(Transform hud, string path, Color fillColor)
    {
        var row = hud.Find(path);
        if (row == null) return;

        var rowImg = row.GetComponent<Image>();
        if (rowImg != null) rowImg.color = new Color(0f, 0f, 0f, 0.35f);

        var fill = row.Find("Fill");
        if (fill != null)
        {
            var img = fill.GetComponent<Image>();
            if (img != null) img.color = fillColor;
        }

        var label = row.Find("Label");
        if (label != null)
        {
            var tmp = label.GetComponent<TMP_Text>();
            if (tmp != null) tmp.fontStyle = FontStyles.Bold;
        }
    }

    private static void HideDemoHelpOverlap()
    {
        var help = GameObject.Find("DemoHelp");
        if (help != null)
            help.SetActive(false);
    }
}
