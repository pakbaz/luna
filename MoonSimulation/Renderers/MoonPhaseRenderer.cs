using SkiaSharp;

namespace MoonSimulation.Renderers;

/// <summary>
/// Shared moon phase shadow algorithm — the single source of truth
/// for the elliptical terminator technique used by both MoonPhaseView
/// and the mini moon in the sky scene.
/// </summary>
public static class MoonPhaseRenderer
{
    /// <summary>
    /// Draws the phase shadow overlay on a moon circle using the elliptical terminator technique.
    /// At moonAngleDeg 0° = New Moon (fully shadowed), 180° = Full Moon (fully lit).
    /// </summary>
    public static void DrawPhaseOverlay(SKCanvas canvas, SKPoint center, float radius,
        double moonAngleDeg, SKColor shadowColor)
    {
        double angle = ((moonAngleDeg % 360) + 360) % 360;
        double phaseRad = angle * Math.PI / 180.0;

        canvas.Save();

        // Clip to the moon circle
        using var clipPath = new SKPath();
        clipPath.AddCircle(center.X, center.Y, radius);
        canvas.ClipPath(clipPath);

        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = shadowColor,
            Style = SKPaintStyle.Fill
        };

        float terminatorScale = (float)Math.Cos(phaseRad);

        using var shadowPath = new SKPath();

        if (angle <= 180)
        {
            // Waxing phases: right side illuminated, shadow on the left
            shadowPath.MoveTo(center.X, center.Y - radius);

            // Left arc (left half of moon)
            shadowPath.ArcTo(
                new SKRect(center.X - radius, center.Y - radius,
                           center.X + radius, center.Y + radius),
                -90, -180, false);

            // Terminator arc (elliptical)
            float termW = radius * Math.Abs(terminatorScale);
            shadowPath.ArcTo(
                new SKRect(center.X - termW, center.Y - radius,
                           center.X + termW, center.Y + radius),
                90, terminatorScale >= 0 ? -180 : 180, false);

            shadowPath.Close();
        }
        else
        {
            // Waning phases: left side illuminated, shadow on the right
            shadowPath.MoveTo(center.X, center.Y - radius);

            // Right arc (right half of moon)
            shadowPath.ArcTo(
                new SKRect(center.X - radius, center.Y - radius,
                           center.X + radius, center.Y + radius),
                -90, 180, false);

            // Terminator arc (elliptical)
            float termW = radius * Math.Abs(terminatorScale);
            shadowPath.ArcTo(
                new SKRect(center.X - termW, center.Y - radius,
                           center.X + termW, center.Y + radius),
                90, terminatorScale >= 0 ? 180 : -180, false);

            shadowPath.Close();
        }

        canvas.DrawPath(shadowPath, shadowPaint);

        canvas.Restore();
    }
}
