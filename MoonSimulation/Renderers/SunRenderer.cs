using SkiaSharp;

namespace MoonSimulation.Renderers;

/// <summary>
/// Renders the Sun with corona glow and sun rays.
/// </summary>
public static class SunRenderer
{
    /// <summary>
    /// Draws the Sun with corona glow and a happy face.
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

        // Happy face on Sun
        FaceRenderer.DrawCuteFace(canvas, center, radius, "happy");
    }

    /// <summary>
    /// Draws subtle sun ray lines emanating from the sun.
    /// </summary>
    public static void DrawSunRays(SKCanvas canvas, SKPoint center, float radius)
    {
        using var rayPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f
        };

        for (int i = 0; i < 12; i++)
        {
            float angle = i * MathF.PI * 2 / 12;
            float innerR = radius * 1.3f;
            float outerR = radius * 1.7f;

            float x1 = center.X + MathF.Cos(angle) * innerR;
            float y1 = center.Y + MathF.Sin(angle) * innerR;
            float x2 = center.X + MathF.Cos(angle) * outerR;
            float y2 = center.Y + MathF.Sin(angle) * outerR;

            byte alpha = (byte)(40 + 20 * MathF.Sin(angle * 3));
            rayPaint.Color = new SKColor(255, 230, 100, alpha);
            canvas.DrawLine(x1, y1, x2, y2, rayPaint);
        }
    }
}
