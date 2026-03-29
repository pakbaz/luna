using SkiaSharp;
using MoonSimulation.Models;

namespace MoonSimulation.Renderers;

/// <summary>
/// Draws cute cartoon faces on celestial bodies.
/// </summary>
public static class FaceRenderer
{
    /// <summary>
    /// Returns a face expression name based on moon phase angle.
    /// Uses PhaseConstants for consistent angle thresholds.
    /// </summary>
    public static string GetMoonFaceExpression(double moonAngleDeg)
    {
        double angle = ((moonAngleDeg % 360) + 360) % 360;
        return angle switch
        {
            < PhaseConstants.NewMoonEnd => "sleepy",
            < PhaseConstants.WaxingCrescentEnd => "content",
            < PhaseConstants.FirstQuarterEnd => "happy",
            < PhaseConstants.WaxingGibbousEnd => "happy",
            < PhaseConstants.FullMoonEnd => "wow",
            < PhaseConstants.WaningGibbousEnd => "content",
            < PhaseConstants.LastQuarterEnd => "happy",
            < PhaseConstants.WaningCrescentEnd => "sleepy",
            _ => "sleepy"
        };
    }

    /// <summary>
    /// Draws a cute cartoon face on a celestial body.
    /// </summary>
    public static void DrawCuteFace(SKCanvas canvas, SKPoint center, float radius,
        string expression)
    {
        float eyeY = center.Y - radius * 0.12f;
        float eyeSpacing = radius * 0.25f;
        float eyeRadius = radius * 0.07f;

        using var eyePaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(40, 40, 40, 200)
        };
        using var eyeHighlight = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(255, 255, 255, 200)
        };
        using var mouthPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = radius * 0.04f,
            StrokeCap = SKStrokeCap.Round,
            Color = new SKColor(40, 40, 40, 180)
        };
        using var blushPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(255, 130, 130, 50)
        };

        switch (expression)
        {
            case "happy":
                DrawHappyFace(canvas, center, radius, eyeY, eyeSpacing, eyeRadius,
                    eyePaint, eyeHighlight, mouthPaint, blushPaint);
                break;
            case "sleepy":
                DrawSleepyFace(canvas, center, radius, eyeY, eyeSpacing, eyeRadius,
                    mouthPaint);
                break;
            case "wow":
                DrawWowFace(canvas, center, radius, eyeY, eyeSpacing, eyeRadius,
                    eyePaint, eyeHighlight, mouthPaint, blushPaint);
                break;
            case "content":
                DrawContentFace(canvas, center, radius, eyeY, eyeSpacing, eyeRadius,
                    eyePaint, eyeHighlight, mouthPaint);
                break;
        }
    }

    private static void DrawHappyFace(SKCanvas canvas, SKPoint center, float radius,
        float eyeY, float eyeSpacing, float eyeRadius,
        SKPaint eyePaint, SKPaint eyeHighlight, SKPaint mouthPaint, SKPaint blushPaint)
    {
        // Eyes
        canvas.DrawCircle(center.X - eyeSpacing, eyeY, eyeRadius, eyePaint);
        canvas.DrawCircle(center.X + eyeSpacing, eyeY, eyeRadius, eyePaint);
        // Eye highlights
        canvas.DrawCircle(center.X - eyeSpacing + eyeRadius * 0.3f, eyeY - eyeRadius * 0.3f,
            eyeRadius * 0.35f, eyeHighlight);
        canvas.DrawCircle(center.X + eyeSpacing + eyeRadius * 0.3f, eyeY - eyeRadius * 0.3f,
            eyeRadius * 0.35f, eyeHighlight);
        // Smile
        using var smilePath = new SKPath();
        smilePath.MoveTo(center.X - radius * 0.2f, center.Y + radius * 0.1f);
        smilePath.QuadTo(center.X, center.Y + radius * 0.3f,
            center.X + radius * 0.2f, center.Y + radius * 0.1f);
        canvas.DrawPath(smilePath, mouthPaint);
        // Blush cheeks
        canvas.DrawCircle(center.X - radius * 0.35f, center.Y + radius * 0.08f,
            radius * 0.1f, blushPaint);
        canvas.DrawCircle(center.X + radius * 0.35f, center.Y + radius * 0.08f,
            radius * 0.1f, blushPaint);
    }

    private static void DrawSleepyFace(SKCanvas canvas, SKPoint center, float radius,
        float eyeY, float eyeSpacing, float eyeRadius, SKPaint mouthPaint)
    {
        // Closed eyes (arcs)
        using (var closedEyePath = new SKPath())
        {
            closedEyePath.MoveTo(center.X - eyeSpacing - eyeRadius, eyeY);
            closedEyePath.QuadTo(center.X - eyeSpacing, eyeY + eyeRadius * 0.8f,
                center.X - eyeSpacing + eyeRadius, eyeY);
            canvas.DrawPath(closedEyePath, mouthPaint);
        }
        using (var closedEyePath2 = new SKPath())
        {
            closedEyePath2.MoveTo(center.X + eyeSpacing - eyeRadius, eyeY);
            closedEyePath2.QuadTo(center.X + eyeSpacing, eyeY + eyeRadius * 0.8f,
                center.X + eyeSpacing + eyeRadius, eyeY);
            canvas.DrawPath(closedEyePath2, mouthPaint);
        }
        // Gentle smile
        using (var sleepSmile = new SKPath())
        {
            sleepSmile.MoveTo(center.X - radius * 0.1f, center.Y + radius * 0.15f);
            sleepSmile.QuadTo(center.X, center.Y + radius * 0.22f,
                center.X + radius * 0.1f, center.Y + radius * 0.15f);
            canvas.DrawPath(sleepSmile, mouthPaint);
        }
        // Zzz
        SphereRenderer.DrawLabel(canvas, "z z", new SKPoint(center.X + radius * 0.5f, center.Y - radius * 0.3f),
            radius * 0.2f, new SKColor(200, 200, 255, 150));
    }

    private static void DrawWowFace(SKCanvas canvas, SKPoint center, float radius,
        float eyeY, float eyeSpacing, float eyeRadius,
        SKPaint eyePaint, SKPaint eyeHighlight, SKPaint mouthPaint, SKPaint blushPaint)
    {
        // Big sparkly eyes
        canvas.DrawCircle(center.X - eyeSpacing, eyeY, eyeRadius * 1.3f, eyePaint);
        canvas.DrawCircle(center.X + eyeSpacing, eyeY, eyeRadius * 1.3f, eyePaint);
        // Star-like highlights
        canvas.DrawCircle(center.X - eyeSpacing + eyeRadius * 0.3f, eyeY - eyeRadius * 0.4f,
            eyeRadius * 0.45f, eyeHighlight);
        canvas.DrawCircle(center.X + eyeSpacing + eyeRadius * 0.3f, eyeY - eyeRadius * 0.4f,
            eyeRadius * 0.45f, eyeHighlight);
        canvas.DrawCircle(center.X - eyeSpacing - eyeRadius * 0.3f, eyeY + eyeRadius * 0.2f,
            eyeRadius * 0.2f, eyeHighlight);
        canvas.DrawCircle(center.X + eyeSpacing - eyeRadius * 0.3f, eyeY + eyeRadius * 0.2f,
            eyeRadius * 0.2f, eyeHighlight);
        // Open mouth (O shape)
        canvas.DrawOval(center.X, center.Y + radius * 0.18f,
            radius * 0.1f, radius * 0.12f, mouthPaint);
        // Extra blush
        blushPaint.Color = new SKColor(255, 130, 130, 70);
        canvas.DrawCircle(center.X - radius * 0.35f, center.Y + radius * 0.1f,
            radius * 0.12f, blushPaint);
        canvas.DrawCircle(center.X + radius * 0.35f, center.Y + radius * 0.1f,
            radius * 0.12f, blushPaint);
    }

    private static void DrawContentFace(SKCanvas canvas, SKPoint center, float radius,
        float eyeY, float eyeSpacing, float eyeRadius,
        SKPaint eyePaint, SKPaint eyeHighlight, SKPaint mouthPaint)
    {
        // Relaxed eyes
        canvas.DrawCircle(center.X - eyeSpacing, eyeY, eyeRadius * 0.9f, eyePaint);
        canvas.DrawCircle(center.X + eyeSpacing, eyeY, eyeRadius * 0.9f, eyePaint);
        canvas.DrawCircle(center.X - eyeSpacing + eyeRadius * 0.3f, eyeY - eyeRadius * 0.3f,
            eyeRadius * 0.3f, eyeHighlight);
        canvas.DrawCircle(center.X + eyeSpacing + eyeRadius * 0.3f, eyeY - eyeRadius * 0.3f,
            eyeRadius * 0.3f, eyeHighlight);
        // Gentle smile
        using var gentleSmile = new SKPath();
        gentleSmile.MoveTo(center.X - radius * 0.15f, center.Y + radius * 0.12f);
        gentleSmile.QuadTo(center.X, center.Y + radius * 0.22f,
            center.X + radius * 0.15f, center.Y + radius * 0.12f);
        canvas.DrawPath(gentleSmile, mouthPaint);
    }
}
