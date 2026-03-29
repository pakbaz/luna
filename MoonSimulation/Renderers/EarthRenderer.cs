using SkiaSharp;

namespace MoonSimulation.Renderers;

/// <summary>
/// Renders Earth with day/night terminator, city lights, and observer dot.
/// </summary>
public static class EarthRenderer
{
    /// <summary>
    /// Draws Earth with a blue/green sphere, day/night terminator, city lights, and observer dot.
    /// </summary>
    public static void DrawEarth(SKCanvas canvas, SKPoint center, float radius, SKPoint lightDir,
        double earthRotationDeg = 0)
    {
        // Base blue sphere (day side)
        SphereRenderer.DrawSphere(canvas, center, radius, new SKColor(40, 80, 200), lightDir, 0.2f);

        // Green continent hints
        using var continentPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(50, 160, 60, 100),
            Style = SKPaintStyle.Fill
        };
        canvas.DrawOval(
            new SKRect(center.X - radius * 0.3f, center.Y - radius * 0.4f,
                       center.X + radius * 0.1f, center.Y - radius * 0.1f),
            continentPaint);
        canvas.DrawOval(
            new SKRect(center.X - radius * 0.15f, center.Y + radius * 0.05f,
                       center.X + radius * 0.25f, center.Y + radius * 0.35f),
            continentPaint);

        // Day/Night terminator shadow
        DrawDayNightTerminator(canvas, center, radius, lightDir);

        // City lights on the dark side
        DrawCityLights(canvas, center, radius, lightDir);

        // Atmosphere glow
        using var atmoPaint = new SKPaint { IsAntialias = true };
        using var atmoShader = SKShader.CreateRadialGradient(
            center, radius * 1.15f,
            new SKColor[] { SKColors.Transparent, new SKColor(100, 180, 255, 30), SKColors.Transparent },
            new float[] { 0.75f, 0.9f, 1f },
            SKShaderTileMode.Clamp);
        atmoPaint.Shader = atmoShader;
        canvas.DrawCircle(center, radius * 1.15f, atmoPaint);

        // Observer dot on the equator
        DrawObserver(canvas, center, radius, earthRotationDeg, lightDir);
    }

    /// <summary>
    /// Draws the dark (night) half of Earth facing away from the Sun.
    /// </summary>
    public static void DrawDayNightTerminator(SKCanvas canvas, SKPoint center, float radius,
        SKPoint lightDir)
    {
        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddCircle(center.X, center.Y, radius);
        canvas.ClipPath(clipPath);

        float angle = MathF.Atan2(-lightDir.Y, -lightDir.X);

        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(5, 5, 30, 180),
            Style = SKPaintStyle.Fill
        };

        using var shadowPath = new SKPath();
        float perpAngle1 = angle + MathF.PI / 2;
        float perpAngle2 = angle - MathF.PI / 2;

        shadowPath.MoveTo(
            center.X + MathF.Cos(perpAngle1) * radius,
            center.Y + MathF.Sin(perpAngle1) * radius);

        shadowPath.ArcTo(
            new SKRect(center.X - radius, center.Y - radius,
                       center.X + radius, center.Y + radius),
            (perpAngle1 * 180f / MathF.PI),
            -180, false);

        shadowPath.LineTo(
            center.X + MathF.Cos(perpAngle1) * radius,
            center.Y + MathF.Sin(perpAngle1) * radius);
        shadowPath.Close();

        canvas.DrawPath(shadowPath, shadowPaint);

        // Soft terminator glow (twilight zone)
        using var twilightPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = radius * 0.15f,
            Color = new SKColor(255, 150, 80, 30)
        };
        canvas.DrawLine(
            center.X + MathF.Cos(perpAngle1) * radius,
            center.Y + MathF.Sin(perpAngle1) * radius,
            center.X + MathF.Cos(perpAngle2) * radius,
            center.Y + MathF.Sin(perpAngle2) * radius,
            twilightPaint);

        canvas.Restore();
    }

    /// <summary>
    /// Draws tiny city light dots on the night side of Earth.
    /// </summary>
    public static void DrawCityLights(SKCanvas canvas, SKPoint center, float radius,
        SKPoint lightDir)
    {
        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddCircle(center.X, center.Y, radius * 0.95f);
        canvas.ClipPath(clipPath);

        using var lightPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(255, 220, 100, 140)
        };

        var cities = new (float dx, float dy)[]
        {
            (-0.15f, -0.1f), (0.2f, -0.2f), (0.1f, 0.15f),
            (-0.25f, 0.2f), (0.3f, -0.05f), (-0.1f, -0.3f),
            (0.0f, 0.1f), (-0.3f, -0.05f), (0.15f, 0.25f),
        };

        foreach (var (dx, dy) in cities)
        {
            float cx = center.X + dx * radius;
            float cy = center.Y + dy * radius;

            float toCityX = (cx - center.X) / radius;
            float toCityY = (cy - center.Y) / radius;
            float dot = toCityX * lightDir.X + toCityY * lightDir.Y;

            if (dot < -0.1f) // On the dark side
            {
                float brightness = Math.Min(1f, Math.Abs(dot));
                lightPaint.Color = new SKColor(255, 220, 100, (byte)(brightness * 120));
                canvas.DrawCircle(cx, cy, 1.2f, lightPaint);
            }
        }

        canvas.Restore();
    }

    /// <summary>
    /// Draws a small observer indicator on Earth's equator.
    /// </summary>
    public static void DrawObserver(SKCanvas canvas, SKPoint center, float radius,
        double earthRotationDeg, SKPoint lightDir)
    {
        double rotRad = earthRotationDeg * Math.PI / 180.0;

        float obsX = center.X + radius * 0.9f * (float)Math.Cos(rotRad + Math.PI);
        float obsY = center.Y + radius * 0.3f * (float)Math.Sin(rotRad + Math.PI);

        float sinComponent = (float)Math.Sin(rotRad + Math.PI);
        if (sinComponent > -0.3f) // Visible hemisphere (with tilt)
        {
            float relX = (obsX - center.X) / radius;
            float dot = relX * lightDir.X;
            bool isDay = dot > 0;

            using var obsPaint = new SKPaint
            {
                IsAntialias = true,
                Color = isDay ? new SKColor(255, 255, 0, 220) : new SKColor(255, 150, 50, 220)
            };
            canvas.DrawCircle(obsX, obsY, 3.5f, obsPaint);

            SphereRenderer.DrawLabel(canvas, isDay ? "You" : "zzZ",
                new SKPoint(obsX, obsY - 10), 10,
                isDay ? new SKColor(255, 255, 200) : new SKColor(200, 200, 255));
        }
    }
}
