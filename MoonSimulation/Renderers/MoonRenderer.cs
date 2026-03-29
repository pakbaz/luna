using SkiaSharp;

namespace MoonSimulation.Renderers;

/// <summary>
/// Renders the Moon body in the orbital view with craters.
/// </summary>
public static class MoonRenderer
{
    /// <summary>
    /// Draws the Moon as a gray sphere with crater dots.
    /// </summary>
    public static void DrawMoonBody(SKCanvas canvas, SKPoint center, float radius, SKPoint lightDir)
    {
        SphereRenderer.DrawSphere(canvas, center, radius, new SKColor(180, 180, 175), lightDir, 0.1f);

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
}
