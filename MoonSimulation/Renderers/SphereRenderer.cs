using SkiaSharp;

namespace MoonSimulation.Renderers;

/// <summary>
/// Generic 3D sphere rendering with lighting, labels, and color helpers.
/// </summary>
public static class SphereRenderer
{
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

    public static SKColor Lighten(SKColor color, float amount)
    {
        return new SKColor(
            (byte)Math.Min(255, color.Red + (255 - color.Red) * amount),
            (byte)Math.Min(255, color.Green + (255 - color.Green) * amount),
            (byte)Math.Min(255, color.Blue + (255 - color.Blue) * amount),
            color.Alpha);
    }

    public static SKColor Darken(SKColor color, float amount)
    {
        return new SKColor(
            (byte)(color.Red * (1 - amount)),
            (byte)(color.Green * (1 - amount)),
            (byte)(color.Blue * (1 - amount)),
            color.Alpha);
    }

    public static SKColor BlendColor(SKColor a, SKColor b, float t)
    {
        t = Math.Clamp(t, 0, 1);
        return new SKColor(
            (byte)(a.Red + (b.Red - a.Red) * t),
            (byte)(a.Green + (b.Green - a.Green) * t),
            (byte)(a.Blue + (b.Blue - a.Blue) * t),
            (byte)(a.Alpha + (b.Alpha - a.Alpha) * t));
    }
}
