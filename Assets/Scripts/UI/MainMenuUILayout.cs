using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime polish cho Main Menu + Level Select sau khi build.
/// </summary>
public static class MainMenuUILayout
{
    public static void Apply()
    {
        ApplyPanel("MainMenuPanel");
        ApplyPanel("LevelSelectPanel");
    }

    private static void ApplyPanel(string panelName)
    {
        var panel = GameObject.Find(panelName);
        if (panel == null) return;

        PolishTitle(panel.transform, "TitleLine1", Color.white, 82f);
        PolishTitle(panel.transform, "TitleLine2", UITheme.Orange, 82f);
        PolishTitle(panel.transform, "TitleText", UITheme.Orange, 26f);
        PolishTitle(panel.transform, "Tagline", UITheme.TextDim, 13f);

        StyleBtn(panel.transform, "Btn_Play", true);
        StyleBtn(panel.transform, "Btn_Quit", false);
        StyleBtn(panel.transform, "Btn_Back", false);
        StyleBtn(panel.transform, "Btn_Desert", false);
        StyleBtn(panel.transform, "Btn_Warzone", false);
    }

    private static void PolishTitle(Transform parent, string name, Color color, float size)
    {
        var t = parent.Find(name);
        if (t == null) return;
        var tmp = t.GetComponent<TMP_Text>();
        if (tmp == null) return;
        tmp.color = color;
        tmp.fontSize = size;
        tmp.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
    }

    private static void StyleBtn(Transform parent, string name, bool primary)
    {
        var t = parent.Find(name);
        if (t == null) return;

        var img = t.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;

        var tmp = t.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.raycastTarget = false;
            tmp.fontStyle = FontStyles.Bold;
            if (!primary) tmp.color = UITheme.TextMuted;
        }
    }
}
