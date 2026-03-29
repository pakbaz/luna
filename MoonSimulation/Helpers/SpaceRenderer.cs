using SkiaSharp;

namespace MoonSimulation.Helpers;

/// <summary>
/// Shared SkiaSharp rendering utilities for space objects.
/// </summary>
public static class SpaceRenderer
{
    private static readonly Random _rng = new(42); // Deterministic stars
    private static SKPoint[]? _stars;
    private static float[]? _starBrightness;
    private static long _twinkleFrame;

    /// <summary>
    /// Draws a dark starry background with subtle twinkle.
    /// </summary>
    public static void DrawStarryBackground(SKCanvas canvas, SKRect bounds)
    {
        // Deep space gradient
        using var bgPaint = new SKPaint();
        using var bgShader = SKShader.CreateLinearGradient(
            new SKPoint(bounds.MidX, bounds.Top),
            new SKPoint(bounds.MidX, bounds.Bottom),
            new SKColor[] { new(8, 8, 40), new(2, 2, 18) },
            null, SKShaderTileMode.Clamp);
        bgPaint.Shader = bgShader;
        canvas.DrawRect(bounds, bgPaint);

        // Generate stars once
        if (_stars == null || _stars.Length < 200)
        {
            _stars = new SKPoint[200];
            _starBrightness = new float[200];
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i] = new SKPoint(
                    _rng.NextSingle() * 2000,
                    _rng.NextSingle() * 2000);
                _starBrightness[i] = 0.3f + _rng.NextSingle() * 0.7f;
            }
        }

        _twinkleFrame++;
        using var starPaint = new SKPaint { IsAntialias = true };

        for (int i = 0; i < _stars.Length; i++)
        {
            float x = _stars[i].X % bounds.Width + bounds.Left;
            float y = _stars[i].Y % bounds.Height + bounds.Top;

            // Gentle twinkle
            float twinkle = _starBrightness![i] *
                (0.7f + 0.3f * MathF.Sin((_twinkleFrame + i * 37) * 0.02f));
            byte alpha = (byte)(twinkle * 255);

            starPaint.Color = new SKColor(255, 255, 240, alpha);
            float size = _starBrightness[i] > 0.8f ? 1.8f : 1.0f;
            canvas.DrawCircle(x, y, size, starPaint);
        }
    }

    /// <summary>
    /// Draws a 3D-shaded sphere with a light direction.
    /// </summary>
    public static void DrawSphere(SKCanvas canvas, SKPoint center, float radius,
        SKColor baseColor, SKPoint lightDir, float ambientLight = 0.15f)
    {
        using var paint = new SKPaint { IsAntialias = true };

        // Offset highlight toward the light source
        float highlightX = center.X + lightDir.X * radius * 0.35f;
        float highlightY = center.Y + lightDir.Y * radius * 0.35f;

        // Radial gradient from highlight to dark edge
        var lightColor = Lighten(baseColor, 0.6f);
        var darkColor = Darken(baseColor, 0.7f);

        using var shader = SKShader.CreateRadialGradient(
            new SKPoint(highlightX, highlightY),
            radius * 1.8f,
            new SKColor[] { lightColor, baseColor, darkColor, Darken(baseColor, 0.9f) },
            new float[] { 0f, 0.35f, 0.75f, 1f },
            SKShaderTileMode.Clamp);

        paint.Shader = shader;
        canvas.DrawCircle(center, radius, paint);
    }

    /// <summary>
    /// Draws the Sun with corona glow.
    /// </summary>
    public static void DrawSun(SKCanvas canvas, SKPoint center, float radius)
    {
        // Outer glow layers
        for (int i = 3; i >= 0; i--)
        {
            float glowRadius = radius * (1.5f + i * 0.4f);
            byte alpha = (byte)(30 - i * 6);
            using var glowPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 200, 50, alpha),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, glowRadius * 0.3f)
            };
            canvas.DrawCircle(center, glowRadius, glowPaint);
        }

        // Sun body with radial gradient
        using var sunPaint = new SKPaint { IsAntialias = true };
        using var sunShader = SKShader.CreateRadialGradient(
            new SKPoint(center.X - radius * 0.2f, center.Y - radius * 0.2f),
            radius * 1.5f,
            new SKColor[] {
                new(255, 255, 220),
                new(255, 220, 80),
                new(255, 160, 20),
                new(200, 100, 0)
            },
            new float[] { 0f, 0.3f, 0.7f, 1f },
            SKShaderTileMode.Clamp);

        sunPaint.Shader = sunShader;
        canvas.DrawCircle(center, radius, sunPaint);
    }

    /// <summary>
    /// Draws Earth with a blue/green sphere and simple continent hint.
    /// </summary>
    public static void DrawEarth(SKCanvas canvas, SKPoint center, float radius, SKPoint lightDir)
    {
        // Base blue sphere
        DrawSphere(canvas, center, radius, new SKColor(40, 80, 200), lightDir, 0.2f);

        // Green continent hints (simple arcs)
        using var continentPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(50, 160, 60, 100),
            Style = SKPaintStyle.Fill
        };

        // Draw a few green blobs to hint at continents
        canvas.DrawOval(
            new SKRect(center.X - radius * 0.3f, center.Y - radius * 0.4f,
                       center.X + radius * 0.1f, center.Y - radius * 0.1f),
            continentPaint);
        canvas.DrawOval(
            new SKRect(center.X - radius * 0.15f, center.Y + radius * 0.05f,
                       center.X + radius * 0.25f, center.Y + radius * 0.35f),
            continentPaint);

        // Atmosphere glow
        using var atmoPaint = new SKPaint { IsAntialias = true };
        using var atmoShader = SKShader.CreateRadialGradient(
            center, radius * 1.15f,
            new SKColor[] { SKColors.Transparent, new SKColor(100, 180, 255, 30), SKColors.Transparent },
            new float[] { 0.75f, 0.9f, 1f },
            SKShaderTileMode.Clamp);
        atmoPaint.Shader = atmoShader;
        canvas.DrawCircle(center, radius * 1.15f, atmoPaint);
    }

    /// <summary>
    /// Draws the Moon as a gray sphere with crater dots.
    /// </summary>
    public static void DrawMoonBody(SKCanvas canvas, SKPoint center, float radius, SKPoint lightDir)
    {
        DrawSphere(canvas, center, radius, new SKColor(180, 180, 175), lightDir, 0.1f);

        // Crater dots
        using var craterPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(140, 140, 135, 80)
        };

        var craters = new (float dx, float dy, float r)[]
        {
            (-0.25f, -0.15f, 0.08f),
            (0.15f, -0.3f, 0.06f),
            (0.3f, 0.1f, 0.1f),
            (-0.1f, 0.3f, 0.07f),
            (0.05f, 0.05f, 0.12f),
            (-0.35f, 0.15f, 0.05f),
        };

        foreach (var (dx, dy, r) in craters)
        {
            float cx = center.X + dx * radius;
            float cy = center.Y + dy * radius;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist < 0.85f) // Only draw if within the visible disk
            {
                canvas.DrawCircle(cx, cy, r * radius, craterPaint);
            }
        }
    }

    /// <summary>
    /// Draws a dashed orbit path (ellipse).
    /// </summary>
    public static void DrawOrbitPath(SKCanvas canvas, SKPoint center, float radiusX, float radiusY)
    {
        using var pathPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            Color = new SKColor(100, 100, 150, 80),
            PathEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0)
        };

        canvas.DrawOval(center, new SKSize(radiusX, radiusY), pathPaint);
    }

    /// <summary>
    /// Draws a text label with shadow.
    /// </summary>
    public static void DrawLabel(SKCanvas canvas, string text, SKPoint position, float fontSize,
        SKColor color, SKTextAlign align = SKTextAlign.Center)
    {
        using var font = new SKFont(SKTypeface.Default, fontSize);
        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(0, 0, 0, 150)
        };
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = color
        };

        canvas.DrawText(text, position.X + 1, position.Y + 1, align, font, shadowPaint);
        canvas.DrawText(text, position.X, position.Y, align, font, textPaint);
    }

    #region Color Helpers

    private static SKColor Lighten(SKColor color, float amount)
    {
        return new SKColor(
            (byte)Math.Min(255, color.Red + (255 - color.Red) * amount),
            (byte)Math.Min(255, color.Green + (255 - color.Green) * amount),
            (byte)Math.Min(255, color.Blue + (255 - color.Blue) * amount),
            color.Alpha);
    }

    private static SKColor Darken(SKColor color, float amount)
    {
        return new SKColor(
            (byte)(color.Red * (1 - amount)),
            (byte)(color.Green * (1 - amount)),
            (byte)(color.Blue * (1 - amount)),
            color.Alpha);
    }

    #endregion
}
