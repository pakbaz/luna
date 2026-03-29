using SkiaSharp;
using SkiaSharp.Views.Maui;
using MoonSimulation.Renderers;
using MoonSimulation.Models;

namespace MoonSimulation.Views;

/// <summary>
/// Top panel: Shows Sun, Earth, and Moon in their orbital positions.
/// Rendered with a slight tilt for a 3D perspective feel.
/// </summary>
public class OrbitalView : SimulationCanvasView
{
    private readonly StarfieldRenderer _starfield = new();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        var bounds = new SKRect(0, 0, info.Width, info.Height);
        _starfield.DrawStarryBackground(canvas, bounds);

        if (State == null) return;

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
        double angleRad = State.MoonAngleDegrees * Math.PI / 180.0;
        float moonX = earthX - orbitRadiusX * (float)Math.Cos(angleRad);
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

        // Draw Sun rays
        SunRenderer.DrawSunRays(canvas, new SKPoint(sunX, sunY), sunRadius);

        // Draw orbit path
        SphereRenderer.DrawOrbitPath(canvas, new SKPoint(earthX, earthY), orbitRadiusX, orbitRadiusY);

        // Determine draw order — Moon behind Earth when in back half of orbit
        bool moonBehind = Math.Sin(angleRad) < 0;

        if (moonBehind)
        {
            MoonRenderer.DrawMoonBody(canvas, new SKPoint(moonX, moonY), moonRadius,
                SunLightDir(new SKPoint(moonX, moonY)));
            SphereRenderer.DrawLabel(canvas, "Moon", new SKPoint(moonX, moonY - moonRadius - 8),
                14, new SKColor(200, 200, 200));
        }

        // Sun (always behind everything at its position)
        SunRenderer.DrawSun(canvas, new SKPoint(sunX, sunY), sunRadius);
        SphereRenderer.DrawLabel(canvas, "Sun", new SKPoint(sunX, sunY + sunRadius + 20),
            15, new SKColor(255, 220, 100));

        // Earth
        EarthRenderer.DrawEarth(canvas, new SKPoint(earthX, earthY), earthRadius,
            SunLightDir(new SKPoint(earthX, earthY)), State.EarthRotationDegrees);
        SphereRenderer.DrawLabel(canvas, "Earth", new SKPoint(earthX, earthY + earthRadius + 20),
            15, new SKColor(100, 180, 255));

        if (!moonBehind)
        {
            MoonRenderer.DrawMoonBody(canvas, new SKPoint(moonX, moonY), moonRadius,
                SunLightDir(new SKPoint(moonX, moonY)));
            SphereRenderer.DrawLabel(canvas, "Moon", new SKPoint(moonX, moonY - moonRadius - 8),
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
        SphereRenderer.DrawLabel(canvas, $"Day {State.ElapsedDays:F0}",
            new SKPoint(w - 60, 25), 14, new SKColor(150, 150, 180));
    }
}
