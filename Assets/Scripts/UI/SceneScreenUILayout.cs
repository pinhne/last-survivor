using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime polish cho Victory / GameOver scenes.
/// </summary>
public static class SceneScreenUILayout
{
    public static void ApplyVictory(Transform canvasRoot)
    {
        PolishTitle(canvasRoot, "TitleText", UITheme.VictoryGreen, 64f);
        PolishTitle(canvasRoot, "Tagline", UITheme.VictoryGreen, 14f);

        var stats = canvasRoot.Find("StatsBox");
        if (stats != null)
        {
            var img = stats.GetComponent<Image>();
            if (img != null) img.color = UITheme.CardBg;
        }

        foreach (var name in new[] { "TotalScoreText", "TotalTimeText", "TotalKillText" })
            PolishStat(canvasRoot, name);
    }

    public static void ApplyGameOver(Transform canvasRoot)
    {
        PolishTitle(canvasRoot, "TitleText", UITheme.BossRed, 64f);
        PolishTitle(canvasRoot, "Tagline", UITheme.BossRed, 14f);

        var sub = FindTmp(canvasRoot, "SubtitleText");
        if (sub != null)
        {
            sub.fontSize = 20f;
            sub.color = UITheme.TextMuted;
        }
    }

    private static void PolishTitle(Transform root, string name, Color color, float size)
    {
        var tmp = FindTmp(root, name);
        if (tmp == null) return;
        tmp.color = color;
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private static void PolishStat(Transform root, string name)
    {
        var tmp = FindTmp(root, name);
        if (tmp == null) return;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Right;
    }

    private static TMP_Text FindTmp(Transform root, string name)
    {
        var t = root.Find(name);
        if (t != null) return t.GetComponent<TMP_Text>();
        foreach (Transform child in root)
        {
            var found = child.Find(name);
            if (found != null) return found.GetComponent<TMP_Text>();
        }
        return null;
    }
}
