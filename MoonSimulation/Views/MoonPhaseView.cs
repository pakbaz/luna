using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using MoonSimulation.Helpers;
using MoonSimulation.Models;

namespace MoonSimulation.Views;

/// <summary>
/// Bottom panel: Shows the Moon as it appears from Earth,
/// with accurate illumination based on the current phase angle.
/// Uses the elliptical terminator technique for crescent/gibbous shapes.
/// </summary>
public class MoonPhaseView : SKCanvasView
{
    private OrbitalState? _state;

    public void SetState(OrbitalState state) => _state = state;

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        var bounds = new SKRect(0, 0, info.Width, info.Height);
        SpaceRenderer.DrawStarryBackground(canvas, bounds);

        if (_state == null) return;

        float w = info.Width;
        float h = info.Height;
        float moonRadius = Math.Min(w, h) * 0.3f;
        var center = new SKPoint(w * 0.5f, h * 0.4f);

        // Draw the fully lit moon body first
        DrawFullMoon(canvas, center, moonRadius);

        // Draw shadow overlay based on phase
        DrawPhaseIllumination(canvas, center, moonRadius, _state.MoonAngleDegrees);

        // Moon edge highlight (limb)
        using var limbPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            Color = new SKColor(220, 220, 215, 60)
        };
        canvas.DrawCircle(center, moonRadius, limbPaint);

        // Cute face on the lit portion of the moon
        string faceExpression = SpaceRenderer.GetMoonFaceExpression(_state.MoonAngleDegrees);
        if (_state.Illumination > 0.15) // Only show face when there's enough visible moon
        {
            SpaceRenderer.DrawCuteFace(canvas, center, moonRadius * 0.7f, faceExpression);
        }

        // Phase label with fun kid-friendly name (no emoji — SkiaSharp can't render them)
        var (phaseName, _) = _state.Phase;
        SpaceRenderer.DrawLabel(canvas, phaseName,
            new SKPoint(w * 0.5f, center.Y + moonRadius + 40),
            22, new SKColor(230, 230, 230));
    }

    /// <summary>
    /// Draws the base moon with craters and subtle surface detail.
    /// </summary>
    private static void DrawFullMoon(SKCanvas canvas, SKPoint center, float radius)
    {
        // Base gray sphere with gentle 3D shading (light from the right for default)
        using var basePaint = new SKPaint { IsAntialias = true };
        using var baseShader = SKShader.CreateRadialGradient(
            new SKPoint(center.X, center.Y),
            radius,
            new SKColor[] { new(200, 200, 195), new(170, 170, 165), new(130, 130, 125) },
            new float[] { 0f, 0.6f, 1f },
            SKShaderTileMode.Clamp);
        basePaint.Shader = baseShader;
        canvas.DrawCircle(center, radius, basePaint);

        // Crater details (maria — dark patches)
        DrawMaria(canvas, center, radius);

        // Small crater dots
        using var craterPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(140, 140, 135, 90)
        };

        var craters = new (float dx, float dy, float r)[]
        {
            (-0.2f, -0.15f, 0.06f),
            (0.1f, -0.25f, 0.04f),
            (0.25f, 0.1f, 0.08f),
            (-0.08f, 0.25f, 0.05f),
            (0.02f, 0.02f, 0.1f),
            (-0.3f, 0.1f, 0.04f),
            (0.15f, 0.3f, 0.03f),
            (-0.15f, -0.35f, 0.03f),
        };

        foreach (var (dx, dy, r) in craters)
        {
            float cx = center.X + dx * radius;
            float cy = center.Y + dy * radius;
            canvas.DrawCircle(cx, cy, r * radius, craterPaint);
        }
    }

    /// <summary>
    /// Draws dark mare (sea) patches on the moon surface.
    /// </summary>
    private static void DrawMaria(SKCanvas canvas, SKPoint center, float radius)
    {
        using var mariaPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(120, 120, 115, 60)
        };

        // Mare Imbrium (top-left area)
        canvas.DrawOval(
            new SKRect(center.X - radius * 0.4f, center.Y - radius * 0.5f,
                       center.X + radius * 0.1f, center.Y - radius * 0.1f),
            mariaPaint);

        // Mare Serenitatis (top-right)
        canvas.DrawOval(
            new SKRect(center.X + radius * 0.05f, center.Y - radius * 0.35f,
                       center.X + radius * 0.35f, center.Y - radius * 0.05f),
            mariaPaint);

        // Mare Tranquillitatis (center-right)
        canvas.DrawOval(
            new SKRect(center.X + radius * 0.0f, center.Y - radius * 0.1f,
                       center.X + radius * 0.4f, center.Y + radius * 0.2f),
            mariaPaint);
    }

    /// <summary>
    /// Draws the shadow on the moon using the elliptical terminator technique.
    /// At moonAngle 0° = New Moon (fully shadowed), 180° = Full Moon (fully lit).
    /// </summary>
    private static void DrawPhaseIllumination(SKCanvas canvas, SKPoint center, float radius,
        double moonAngleDegrees)
    {
        double angle = ((moonAngleDegrees % 360) + 360) % 360;
        double phaseRad = angle * Math.PI / 180.0;

        // Save canvas state
        canvas.Save();

        // Clip to the moon circle
        using var clipPath = new SKPath();
        clipPath.AddCircle(center.X, center.Y, radius);
        canvas.ClipPath(clipPath);

        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(5, 5, 20, 220),
            Style = SKPaintStyle.Fill
        };

        // The terminator (shadow boundary) is an ellipse whose x-scale
        // varies with cos(phaseAngle).
        // terminatorX = radius * cos(phaseAngle)
        // When phaseAngle = 0 (new moon) → terminatorX = radius → full shadow
        // When phaseAngle = π (full moon) → terminatorX = -radius → no shadow

        float terminatorScale = (float)Math.Cos(phaseRad);

        using var shadowPath = new SKPath();

        if (angle <= 180)
        {
            // Waxing phases: right side illuminated, shadow on the left
            // Shadow covers from left edge to terminator
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
