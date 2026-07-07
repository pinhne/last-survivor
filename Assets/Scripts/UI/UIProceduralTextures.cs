using UnityEngine;

/// <summary>
/// Tạo texture nền cinematic procedural — không cần file ảnh ngoài.
/// </summary>
public static class UIProceduralTextures
{
    public enum Style { Menu, Victory, GameOver, Desert }

    public static Sprite CreateSprite(Style style, int size = 1024)
    {
        var tex = Create(style, size);
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    public static Texture2D Create(Style style, int size = 1024)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        for (int y = 0; y < size; y++)
        {
            float ny = y / (float)(size - 1);
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)(size - 1);
                tex.SetPixel(x, y, Sample(style, nx, ny, size));
            }
        }

        tex.Apply();
        return tex;
    }

    private static float Perlin(float nx, float ny, float scale, float seed)
    {
        return Mathf.PerlinNoise(nx * scale + seed, ny * scale + seed * 1.37f);
    }

    private static float Vignette(float nx, float ny, float cx = 0.5f, float cy = 0.45f, float strength = 1.35f)
    {
        float d = Mathf.Sqrt((nx - cx) * (nx - cx) + (ny - cy) * (ny - cy));
        return Mathf.Clamp01(Mathf.Pow(1f - d * strength, 2.1f));
    }

    private static Color Sample(Style style, float nx, float ny, int size)
    {
        float vig = Vignette(nx, ny);
        float grain = Perlin(nx, ny, 180f, 3.7f) * 0.06f;

        return style switch
        {
            Style.Menu => MenuPixel(nx, ny, vig, grain, size),
            Style.Victory => VictoryPixel(nx, ny, vig, grain),
            Style.GameOver => GameOverPixel(nx, ny, vig, grain),
            _ => DesertPixel(nx, ny, vig, grain)
        };
    }

    private static Color MenuPixel(float nx, float ny, float vig, float grain, int size)
    {
        var baseCol = new Color(0.015f, 0.02f, 0.045f);

        // Horizon glow (apocalyptic sunset)
        float horizon = Mathf.Exp(-Mathf.Pow((ny - 0.18f) * 5f, 2f));
        var sunset = new Color(0.75f, 0.22f, 0.04f) * horizon * 0.7f;
        sunset += new Color(0.35f, 0.08f, 0.02f) * horizon * 0.4f;

        // God-ray from center-bottom
        float beam = Mathf.Exp(-Mathf.Pow((nx - 0.5f) * 2.8f, 2f)) * Mathf.Exp(-Mathf.Pow((ny - 0.68f) * 1.8f, 2f));
        var accent = new Color(1f, 0.5f, 0.08f) * beam * 0.45f;

        // Subtle grid (tactical HUD feel)
        float gridX = Mathf.Abs(Mathf.Sin(nx * Mathf.PI * 28f)) < 0.04f ? 0.025f : 0f;
        float gridY = Mathf.Abs(Mathf.Sin(ny * Mathf.PI * 16f)) < 0.04f ? 0.02f : 0f;

        // Stars (top region)
        float stars = 0f;
        if (ny > 0.55f)
        {
            float s = Perlin(nx, ny, 420f, 12f);
            if (s > 0.93f) stars = (s - 0.93f) * 12f;
        }

        // City silhouette at bottom
        float skyline = 0f;
        if (ny < 0.28f)
        {
            float buildings = 0f;
            for (int i = 0; i < 8; i++)
            {
                float bx = (i + 0.3f) / 8f;
                float bw = 0.06f + Perlin(bx, 0f, 4f, i) * 0.05f;
                float bh = 0.08f + Perlin(bx, 1f, 3f, i + 5) * 0.14f;
                if (nx > bx - bw && nx < bx + bw && ny < bh)
                    buildings = Mathf.Max(buildings, 1f - ny / bh);
            }
            skyline = buildings * 0.35f;
        }

        var c = baseCol + sunset + accent;
        c += new Color(gridX + gridY, gridX * 0.5f, 0f);
        c += new Color(stars, stars * 0.9f, stars * 0.7f);
        c += new Color(0f, 0f, 0f, 0f) * skyline + new Color(0.02f, 0.015f, 0.03f) * skyline;
        c += new Color(grain, grain * 0.85f, grain * 0.5f);
        c *= 0.28f + vig * 0.82f;
        c.a = 1f;
        return c;
    }

    private static Color VictoryPixel(float nx, float ny, float vig, float grain)
    {
        var baseCol = new Color(0.01f, 0.035f, 0.018f);

        float aurora = Mathf.Sin(nx * 10f + ny * 4f) * 0.5f + 0.5f;
        aurora *= Mathf.Exp(-Mathf.Pow((ny - 0.62f) * 2.2f, 2f));
        var green = new Color(0.08f, 0.8f, 0.28f) * aurora * 0.55f;

        float rays = Mathf.Exp(-Mathf.Pow((nx - 0.5f) * 1.8f, 2f)) * Mathf.Exp(-Mathf.Pow((ny - 0.48f) * 1.4f, 2f));
        var glow = new Color(0.25f, 1f, 0.5f) * rays * 0.35f;

        float spark = ny > 0.5f && Perlin(nx, ny, 350f, 8f) > 0.94f ? 0.15f : 0f;

        var c = baseCol + green + glow + new Color(spark * 0.5f, spark, spark * 0.6f);
        c += new Color(grain * 0.4f, grain, grain * 0.5f);
        c *= 0.25f + vig * 0.85f;
        c.a = 1f;
        return c;
    }

    private static Color GameOverPixel(float nx, float ny, float vig, float grain)
    {
        var baseCol = new Color(0.04f, 0.008f, 0.012f);

        float pulse = Mathf.Exp(-Mathf.Pow((ny - 0.52f) * 1.9f, 2f));
        var red = new Color(0.7f, 0.04f, 0.02f) * pulse * 0.65f;

        float crack = Perlin(nx, ny, 14f, 2f);
        red += new Color(crack * 0.12f, crack * 0.02f, 0f);

        // Blood drip streaks
        float drip = 0f;
        if (nx > 0.35f && nx < 0.65f)
        {
            float streak = Perlin(nx * 3f, 0f, 6f, 1f);
            if (ny < streak * 0.35f) drip = (streak * 0.35f - ny) * 0.4f;
        }

        var c = baseCol + red + new Color(drip * 0.5f, 0f, 0f);
        c += new Color(grain, grain * 0.25f, grain * 0.25f);
        c *= 0.22f + vig * 0.88f;
        c.a = 1f;
        return c;
    }

    private static Color DesertPixel(float nx, float ny, float vig, float grain)
    {
        var sandLow = new Color(0.32f, 0.24f, 0.12f);
        var sandHigh = new Color(0.58f, 0.44f, 0.22f);
        var sand = Color.Lerp(sandLow, sandHigh, ny);

        float dune = Mathf.Sin(nx * 7f + Perlin(nx, 0.5f, 3f, 4f) * 2f) * 0.5f + 0.5f;
        sand *= 0.7f + dune * 0.35f;

        float sun = Mathf.Exp(-Mathf.Pow((nx - 0.8f) * 4.5f, 2f)) * Mathf.Exp(-Mathf.Pow((ny - 0.85f) * 4f, 2f));
        sand += new Color(1f, 0.78f, 0.38f) * sun * 0.45f;

        float heat = Perlin(nx, ny, 8f, 1f) * 0.08f;
        sand += new Color(heat, heat * 0.6f, 0f);

        sand += new Color(grain, grain * 0.65f, grain * 0.25f);
        sand *= 0.38f + vig * 0.68f;
        sand.a = 1f;
        return sand;
    }

    public static Sprite CreateVignetteRing(int size = 256)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        float cx = size * 0.5f;
        float cy = size * 0.5f;
        float maxR = size * 0.5f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) / maxR;
            float a = Mathf.SmoothStep(0.25f, 1f, d);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
