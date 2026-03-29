using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using MoonSimulation.Helpers;
using MoonSimulation.Models;

namespace MoonSimulation.Views;

/// <summary>
/// Top panel: Shows Sun, Earth, and Moon in their orbital positions.
/// Rendered with a slight tilt for a 3D perspective feel.
/// </summary>
public class OrbitalView : SKCanvasView
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

        // Layout: Sun on the left, Earth in the center-right, Moon orbiting Earth
        float sunX = w * 0.15f;
        float sunY = h * 0.5f;
        float sunRadius = Math.Min(w, h) * 0.12f;

        float earthX = w * 0.6f;
        float earthY = h * 0.5f;
        float earthRadius = Math.Min(w, h) * 0.08f;

        // Moon orbit radius
        float orbitRadiusX = Math.Min(w, h) * 0.25f;
        float orbitRadiusY = orbitRadiusX * 0.4f; // Elliptical for 3D tilt

        // Moon position along orbit
        double angleRad = _state.MoonAngleDegrees * Math.PI / 180.0;
        float moonX = earthX + orbitRadiusX * (float)Math.Cos(angleRad);
        float moonY = earthY + orbitRadiusY * (float)Math.Sin(angleRad);
        float moonRadius = Math.Min(w, h) * 0.04f;

        // Light direction from Sun to each object
        SKPoint SunLightDir(SKPoint objCenter)
        {
            float dx = sunX - objCenter.X;
            float dy = sunY - objCenter.Y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            return len > 0 ? new SKPoint(dx / len, dy / len) : new SKPoint(-1, 0);
        }

        // Draw Sun rays (subtle lines emanating from the sun)
        DrawSunRays(canvas, new SKPoint(sunX, sunY), sunRadius);

        // Draw orbit path
        SpaceRenderer.DrawOrbitPath(canvas, new SKPoint(earthX, earthY), orbitRadiusX, orbitRadiusY);

        // Determine draw order — Moon behind Earth when in back half of orbit
        bool moonBehind = Math.Sin(angleRad) < 0;

        if (moonBehind)
        {
            // Moon first (behind), then Earth
            SpaceRenderer.DrawMoonBody(canvas, new SKPoint(moonX, moonY), moonRadius,
                SunLightDir(new SKPoint(moonX, moonY)));
            SpaceRenderer.DrawLabel(canvas, "Moon", new SKPoint(moonX, moonY - moonRadius - 8),
                14, new SKColor(200, 200, 200));
        }

        // Sun (always behind everything at its position)
        SpaceRenderer.DrawSun(canvas, new SKPoint(sunX, sunY), sunRadius);
        SpaceRenderer.DrawLabel(canvas, "Sun", new SKPoint(sunX, sunY + sunRadius + 20),
            15, new SKColor(255, 220, 100));

        // Earth
        SpaceRenderer.DrawEarth(canvas, new SKPoint(earthX, earthY), earthRadius,
            SunLightDir(new SKPoint(earthX, earthY)), _state.EarthRotationDegrees);
        SpaceRenderer.DrawLabel(canvas, "Earth", new SKPoint(earthX, earthY + earthRadius + 20),
            15, new SKColor(100, 180, 255));

        if (!moonBehind)
        {
            // Moon in front
            SpaceRenderer.DrawMoonBody(canvas, new SKPoint(moonX, moonY), moonRadius,
                SunLightDir(new SKPoint(moonX, moonY)));
            SpaceRenderer.DrawLabel(canvas, "Moon", new SKPoint(moonX, moonY - moonRadius - 8),
                14, new SKColor(200, 200, 200));
        }

        // Draw a line from Sun to Moon to show light direction (very subtle)
        using var linePaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(255, 255, 150, 20),
            StrokeWidth = 1f,
            Style = SKPaintStyle.Stroke,
            PathEffect = SKPathEffect.CreateDash(new float[] { 4, 8 }, 0)
        };
        canvas.DrawLine(sunX + sunRadius, sunY, moonX, moonY, linePaint);

        // Day counter
        SpaceRenderer.DrawLabel(canvas, $"Day {_state.ElapsedDays:F0}",
            new SKPoint(w - 60, 25), 14, new SKColor(150, 150, 180));
    }

    private static void DrawSunRays(SKCanvas canvas, SKPoint center, float radius)
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
