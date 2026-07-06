using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Hiển thị thông báo mua / lỗi trên Txt_Error.
/// </summary>
public static class UIShopFeedback
{
    public static void ShowSuccess(MonoBehaviour host, TMP_Text label, ref Coroutine routine, string message)
    {
        Show(host, label, ref routine, message, UITheme.HpGreen, autoHideSeconds: 2f, useRealtime: true);
    }

    public static void ShowError(MonoBehaviour host, TMP_Text label, ref Coroutine routine, string message)
    {
        Show(host, label, ref routine, message, UITheme.BossRed, autoHideSeconds: 0f, useRealtime: true);
    }

    private static void Show(MonoBehaviour host, TMP_Text label, ref Coroutine routine,
        string message, Color color, float autoHideSeconds, bool useRealtime)
    {
        if (label == null) return;

        label.text = message;
        label.color = color;
        label.gameObject.SetActive(true);

        if (routine != null)
            host.StopCoroutine(routine);

        if (autoHideSeconds > 0f)
            routine = host.StartCoroutine(HideAfter(host, label, autoHideSeconds, useRealtime));
        else
            routine = null;
    }

    public static void Hide(TMP_Text label, MonoBehaviour host, ref Coroutine routine)
    {
        if (routine != null)
        {
            host.StopCoroutine(routine);
            routine = null;
        }
        if (label != null)
            label.gameObject.SetActive(false);
    }

    private static IEnumerator HideAfter(MonoBehaviour host, TMP_Text label, float seconds, bool useRealtime)
    {
        if (useRealtime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
        if (label != null)
            label.gameObject.SetActive(false);
    }
}
