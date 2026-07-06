using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tắt raycast trên text / nền trang trí để click không bị chặn.
/// Gọi một lần khi scene UI load.
/// </summary>
public static class UIRaycastHelper
{
    public static void FixScene()
    {
        var tmps = Object.FindObjectsOfType<TMP_Text>(true);
        foreach (var tmp in tmps)
            tmp.raycastTarget = false;

        var buttons = Object.FindObjectsOfType<Button>(true);
        foreach (var btn in buttons)
        {
            var rootImg = btn.GetComponent<Image>();
            if (rootImg != null)
                rootImg.raycastTarget = true;

            foreach (var graphic in btn.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.transform == btn.transform)
                    continue;
                graphic.raycastTarget = false;
            }
        }

        var images = Object.FindObjectsOfType<Image>(true);
        foreach (var img in images)
        {
            if (img.GetComponent<Button>() != null)
                continue;
            if (img.GetComponentInParent<Button>() != null)
                continue;

            string n = img.gameObject.name;
            if (n == "ShopPanel" || n == "PausePanel" || n == "VictoryPanel"
                || n == "GameOverPanel" || n == "WarningPanel" || n == "ShopInner")
                continue;

            img.raycastTarget = false;
        }
    }
}
