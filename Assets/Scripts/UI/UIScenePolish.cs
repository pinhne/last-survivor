using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Polish runtime — nền đẹp + hiệu ứng, hoạt động ngay cả scene cũ chưa rebuild.
/// </summary>
[DefaultExecutionOrder(-200)]
public class UIScenePolish : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAttach()
    {
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            if (canvas.GetComponent<UIScenePolish>() != null) continue;
            canvas.gameObject.AddComponent<UIScenePolish>();
        }
    }

    private bool _done;

    private void Awake()
    {
        if (_done) return;
        _done = true;
        string scene = SceneManager.GetActiveScene().name;
        var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var style = scene switch
        {
            "Victory"   => UIProceduralTextures.Style.Victory,
            "GameOver"  => UIProceduralTextures.Style.GameOver,
            "UITest"    => UIProceduralTextures.Style.Desert,
            _           => UIProceduralTextures.Style.Menu
        };

        bool isHud = IsHudCanvas(canvas.transform);

        if (isHud)
        {
            TameHudBackground(canvas.transform);
            EnsureBackdrop(canvas.transform, style, subtleVignette: true);
            EnsureHudFX(canvas.transform);
        }
        else
        {
            EnsureBackdrop(canvas.transform, style);
            EnsureFX(canvas.transform, style);
            AddCornerBrackets(canvas.transform, style);
        }

        RemoveUglyBoxes(canvas.transform);
        PolishTypography(canvas.transform, scene);
        if (!isHud) PolishLevelCards(canvas.transform);
        AttachIntroAndHover(canvas.transform, isHud);
        UIRaycastHelper.FixScene();

        if (scene == "MainMenu" || canvas.transform.Find("MainMenuPanel") != null)
            MainMenuUILayout.Apply();
        if (scene == "Victory" || canvas.GetComponent<VictoryScreenUI>() != null)
            SceneScreenUILayout.ApplyVictory(canvas.transform);
        if (scene == "GameOver" || canvas.GetComponent<GameOverScreenUI>() != null)
            SceneScreenUILayout.ApplyGameOver(canvas.transform);
        if (isHud)
        {
            HUDUILayout.Apply(canvas.transform);
            FixHudOverlap(canvas.transform);
            BringHudToFront(canvas.transform);
        }
    }

    private static bool IsHudCanvas(Transform canvas)
    {
        return canvas.Find("HUDPanel") != null || canvas.Find("WaveInfo") != null;
    }

    private static void TameHudBackground(Transform canvas)
    {
        var desert = canvas.Find("DesertPlaceholder");
        if (desert != null)
            desert.gameObject.SetActive(false);

        var cine = canvas.Find("CinematicBG");
        if (cine != null)
        {
            foreach (var name in new[] { "GradWarm", "GradCool", "AccentGlow", "Scanlines" })
            {
                var layer = cine.Find(name);
                if (layer != null)
                    layer.gameObject.SetActive(false);
            }
            var baseImg = cine.GetComponent<Image>();
            if (baseImg != null)
                baseImg.color = new Color(0.04f, 0.035f, 0.03f, 1f);
        }

        var corners = canvas.Find("CornerBrackets");
        if (corners != null)
            Object.Destroy(corners.gameObject);
    }

    private static void BringHudToFront(Transform canvas)
    {
        string[] order =
        {
            "WaveInfo", "LevelBadge", "Crosshair", "HUDPanel", "BossUI",
            "Btn_Shop", "ShopPanel", "PausePanel", "VictoryPanel", "GameOverPanel"
        };
        foreach (var name in order)
        {
            var t = canvas.Find(name);
            if (t != null)
                t.SetAsLastSibling();
        }
    }

    private static void EnsureHudFX(Transform canvas)
    {
        if (canvas.GetComponent<UIAmbientEffects>() != null) return;
        var fx = canvas.gameObject.AddComponent<UIAmbientEffects>();
        fx.Setup(new Color(1f, 0.6f, 0.2f, 0.06f), 8);
    }

    private static void FixHudOverlap(Transform canvas)
    {
        var badge = canvas.Find("LevelBadge");
        if (badge != null)
        {
            var rt = badge.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -12f);
                rt.sizeDelta = new Vector2(520f, 28f);
            }
            var img = badge.GetComponent<Image>();
            if (img != null) img.color = new Color(0f, 0f, 0f, 0.55f);
        }
    }

    private static void EnsureBackdrop(Transform canvas, UIProceduralTextures.Style style, bool subtleVignette = false)
    {
        if (canvas.Find("ProceduralBG") != null) return;

        var go = new GameObject("ProceduralBG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(canvas, false);
        go.transform.SetAsFirstSibling();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.sprite = UIProceduralTextures.CreateSprite(style, 1024);
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.raycastTarget = false;

        // Vignette overlay
        var vig = new GameObject("Vignette", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        vig.transform.SetParent(go.transform, false);
        var vrt = vig.GetComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = vrt.offsetMax = Vector2.zero;
        vig.GetComponent<Image>().color = new Color(0f, 0f, 0f, subtleVignette ? 0.18f : 0.42f);
        vig.GetComponent<Image>().raycastTarget = false;

        // Radial vignette ring
        var ring = new GameObject("VignetteRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ring.transform.SetParent(go.transform, false);
        var rrt = ring.GetComponent<RectTransform>();
        rrt.anchorMin = Vector2.zero;
        rrt.anchorMax = Vector2.one;
        rrt.offsetMin = rrt.offsetMax = Vector2.zero;
        var ringImg = ring.GetComponent<Image>();
        ringImg.sprite = UIProceduralTextures.CreateVignetteRing(256);
        ringImg.color = new Color(0f, 0f, 0f, subtleVignette ? 0.25f : 0.55f);
        ringImg.raycastTarget = false;
    }

    private static void EnsureFX(Transform canvas, UIProceduralTextures.Style style)
    {
        if (canvas.GetComponent<UIAmbientEffects>() != null) return;

        var fx = canvas.gameObject.AddComponent<UIAmbientEffects>();
        Color accent = style switch
        {
            UIProceduralTextures.Style.Victory  => new Color(0.2f, 0.95f, 0.45f, 0.2f),
            UIProceduralTextures.Style.GameOver => new Color(1f, 0.15f, 0.1f, 0.22f),
            UIProceduralTextures.Style.Desert  => new Color(1f, 0.6f, 0.2f, 0.15f),
            _ => new Color(1f, 0.55f, 0f, 0.18f)
        };
        fx.Setup(accent, style == UIProceduralTextures.Style.Desert ? 12 : 22);
    }

    private static void RemoveUglyBoxes(Transform canvas)
    {
        foreach (var name in new[] { "MenuCard", "MenuBackground", "DesertPlaceholder", "GradOverlay" })
        {
            var t = canvas.Find(name);
            if (t == null) continue;
            var img = t.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0f, 0f, 0f, 0f);
                img.raycastTarget = false;
            }
        }

        // Glass panel behind title instead of brown box
        var main = canvas.Find("MainMenuPanel");
        if (main != null && main.Find("TitleGlass") == null)
        {
            var glass = new GameObject("TitleGlass", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glass.transform.SetParent(main, false);
            glass.transform.SetAsFirstSibling();
            var rt = glass.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(620f, 420f);
            rt.anchoredPosition = new Vector2(0f, 40f);
            var img = glass.GetComponent<Image>();
            img.color = new Color(0.04f, 0.04f, 0.07f, 0.55f);
            img.raycastTarget = false;
            var ol = glass.AddComponent<Outline>();
            ol.effectColor = new Color(1f, 0.55f, 0f, 0.45f);
            ol.effectDistance = new Vector2(2f, -2f);
        }
    }

    private static void PolishTypography(Transform canvas, string scene)
    {
        foreach (var tmp in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp.font == null && TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;

            if ((tmp.name == "TitleLine1" || tmp.name == "TitleText")
                && (tmp.text.Contains("LAST") || tmp.text == "HOÀN THÀNH" || tmp.text == "GAME OVER"))
            {
                tmp.fontStyle = FontStyles.Bold;
                AddShadow(tmp);
            }

            if (tmp.name == "TitleLine2")
            {
                tmp.color = UITheme.Orange;
                tmp.fontStyle = FontStyles.Bold;
                tmp.characterSpacing = 10f;
                AddShadow(tmp, new Color(0.3f, 0.1f, 0f, 0.8f));
            }

            // Fix broken glyph squares on buttons
            if (tmp.text.StartsWith("\u25A0") || tmp.text.Contains("\u25A0"))
                tmp.text = tmp.text.Replace("\u25A0 ", "").Replace("\u25A0", "");
        }

        StyleAllButtons(canvas);

        var stats = canvas.Find("StatsBox");
        if (stats != null)
        {
            var img = stats.GetComponent<Image>();
            if (img != null) img.color = UITheme.CardBg;
            var ol = stats.GetComponent<Outline>() ?? stats.gameObject.AddComponent<Outline>();
            ol.effectColor = UITheme.VictoryGreen;
            ol.effectDistance = new Vector2(2f, -2f);
        }
    }

    private static void StyleAllButtons(Transform root)
    {
        foreach (var btn in root.GetComponentsInChildren<Button>(true))
        {
            var img = btn.GetComponent<Image>();
            if (img == null) continue;

            var tmp = btn.GetComponentInChildren<TMP_Text>();
            bool isShop = btn.name == "Btn_Shop";
            bool primary = tmp != null && (tmp.text.Contains("CHƠI") || tmp.text.Contains("THỬ") || tmp.text.Contains("MENU CHÍNH"));

            if (isShop)
            {
                img.color = new Color(0f, 0f, 0f, 0.82f);
                if (tmp != null) { tmp.color = UITheme.Gold; tmp.fontStyle = FontStyles.Bold; }
                var ol = btn.GetComponent<Outline>() ?? btn.gameObject.AddComponent<Outline>();
                ol.effectColor = UITheme.Gold;
                ol.effectDistance = new Vector2(2f, -2f);
            }
            else if (primary)
            {
                img.color = UITheme.Orange;
                if (tmp != null) { tmp.color = Color.white; tmp.fontStyle = FontStyles.Bold; }
            }
            else
            {
                img.color = new Color(0.06f, 0.06f, 0.09f, 0.85f);
                var ol = btn.GetComponent<Outline>() ?? btn.gameObject.AddComponent<Outline>();
                ol.effectColor = UITheme.CardBorder;
                ol.effectDistance = new Vector2(2f, -2f);
            }
        }
    }

    private static void AddShadow(TMP_Text tmp, Color? shadow = null)
    {
        var sh = tmp.GetComponent<Shadow>() ?? tmp.gameObject.AddComponent<Shadow>();
        sh.effectColor = shadow ?? new Color(0f, 0f, 0f, 0.75f);
        sh.effectDistance = new Vector2(3f, -3f);
    }

    private static void PolishLevelCards(Transform canvas)
    {
        foreach (var card in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (!card.name.StartsWith("LevelCard")) continue;
            var img = card.GetComponent<Image>();
            if (img == null) continue;
            img.color = new Color(0.06f, 0.06f, 0.09f, 0.88f);
            var ol = card.GetComponent<Outline>() ?? card.gameObject.AddComponent<Outline>();
            ol.effectColor = UITheme.Orange;
            ol.effectDistance = new Vector2(2f, -2f);

            var thumb = card.Find("Thumbnail");
            if (thumb != null)
            {
                var timg = thumb.GetComponent<Image>();
                if (timg != null)
                {
                    float lum = (timg.color.r + timg.color.g + timg.color.b) / 3f;
                    if (lum > 0.3f)
                        timg.color = new Color(0.85f, 0.55f, 0.15f, 1f);
                }
            }
        }
    }

    private static void AddCornerBrackets(Transform canvas, UIProceduralTextures.Style style)
    {
        if (canvas.Find("CornerBrackets") != null) return;

        Color col = style switch
        {
            UIProceduralTextures.Style.Victory => UITheme.VictoryGreen,
            UIProceduralTextures.Style.GameOver => new Color(1f, 0.2f, 0.15f, 0.7f),
            _ => UITheme.Orange
        };

        var root = new GameObject("CornerBrackets", typeof(RectTransform));
        root.transform.SetParent(canvas, false);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        float inset = 48f;
        float len = 72f;
        float thick = 3f;
        foreach (var (anchor, pivot, pos) in new[]
        {
            (new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(inset, -inset)),
            (new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-inset, -inset)),
            (new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(inset, inset)),
            (new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-inset, inset))
        })
        {
            var bracket = new GameObject("Bracket", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bracket.transform.SetParent(root.transform, false);
            var br = bracket.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = anchor;
            br.pivot = pivot;
            br.anchoredPosition = pos;
            br.sizeDelta = new Vector2(len, thick);
            bracket.GetComponent<Image>().color = col;
            bracket.GetComponent<Image>().raycastTarget = false;

            var vert = Object.Instantiate(bracket, root.transform);
            var vr = vert.GetComponent<RectTransform>();
            vr.sizeDelta = new Vector2(thick, len);
        }
    }

    private static void AttachIntroAndHover(Transform canvas, bool isHud)
    {
        if (!isHud)
        {
            var panel = canvas.Find("MainMenuPanel") ?? canvas.Find("VictoryPanel") ?? canvas.Find("GameOverPanel")
                        ?? canvas.Find("LevelSelectPanel");
            if (panel != null && panel.GetComponent<UITitleIntroFX>() == null)
                panel.gameObject.AddComponent<UITitleIntroFX>();
        }

        foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
        {
            if (btn.GetComponent<UIButtonHoverFX>() == null)
                btn.gameObject.AddComponent<UIButtonHoverFX>();
        }
    }
}
