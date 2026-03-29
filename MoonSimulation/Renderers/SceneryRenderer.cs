using SkiaSharp;

namespace MoonSimulation.Renderers;

/// <summary>
/// Draws ground scenery: little house, clouds, and trees.
/// </summary>
public static class SceneryRenderer
{
    /// <summary>
    /// Draws fluffy clouds in the day sky.
    /// </summary>
    public static void DrawClouds(SKCanvas canvas, SKRect bounds, float groundY,
        double sunElevation, long frame)
    {
        if (sunElevation < -0.1) return; // No clouds at night

        byte alpha = (byte)(Math.Clamp(sunElevation + 0.1, 0, 1) * 120);
        using var cloudPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(255, 255, 255, alpha)
        };

        float w = bounds.Width;
        float skyH = groundY - bounds.Top;

        // Slowly drifting clouds
        float drift = (frame * 0.3f) % (w + 200) - 100;

        DrawCloud(canvas, bounds.Left + drift, bounds.Top + skyH * 0.2f, w * 0.12f, cloudPaint);
        DrawCloud(canvas, bounds.Left + (drift * 0.6f + w * 0.5f) % (w + 200) - 100,
            bounds.Top + skyH * 0.35f, w * 0.09f, cloudPaint);
        DrawCloud(canvas, bounds.Left + (drift * 0.4f + w * 0.3f) % (w + 200) - 100,
            bounds.Top + skyH * 0.5f, w * 0.07f, cloudPaint);
    }

    /// <summary>
    /// Draws a single fluffy cloud from circle clusters.
    /// </summary>
    public static void DrawCloud(SKCanvas canvas, float x, float y, float size, SKPaint paint)
    {
        canvas.DrawCircle(x, y, size * 0.5f, paint);
        canvas.DrawCircle(x - size * 0.35f, y + size * 0.1f, size * 0.35f, paint);
        canvas.DrawCircle(x + size * 0.35f, y + size * 0.1f, size * 0.35f, paint);
        canvas.DrawCircle(x + size * 0.15f, y - size * 0.15f, size * 0.4f, paint);
        canvas.DrawCircle(x - size * 0.15f, y + size * 0.2f, size * 0.3f, paint);
    }

    /// <summary>
    /// Draws a little house with a tree on the ground (cozy scene for kids).
    /// </summary>
    public static void DrawLittleHouse(SKCanvas canvas, SKRect bounds, float groundY,
        double sunElevation)
    {
        float w = bounds.Width;
        float houseX = bounds.Left + w * 0.7f;
        float houseW = w * 0.1f;
        float houseH = houseW * 0.8f;
        float houseY = groundY - houseH;
        bool isNight = sunElevation < 0;

        // House body
        using var housePaint = new SKPaint
        {
            IsAntialias = true,
            Color = isNight ? new SKColor(80, 50, 40) : new SKColor(180, 120, 80)
        };
        canvas.DrawRect(houseX, houseY, houseW, houseH, housePaint);

        // Roof (triangle)
        using var roofPaint = new SKPaint
        {
            IsAntialias = true,
            Color = isNight ? new SKColor(100, 40, 40) : new SKColor(180, 60, 60)
        };
        using var roofPath = new SKPath();
        roofPath.MoveTo(houseX - houseW * 0.15f, houseY);
        roofPath.LineTo(houseX + houseW * 0.5f, houseY - houseH * 0.6f);
        roofPath.LineTo(houseX + houseW * 1.15f, houseY);
        roofPath.Close();
        canvas.DrawPath(roofPath, roofPaint);

        // Window (glowing at night!)
        float winX = houseX + houseW * 0.3f;
        float winY = houseY + houseH * 0.25f;
        float winSize = houseW * 0.35f;

        if (isNight)
        {
            // Warm window glow
            using var glowPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 200, 80, 60),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, winSize)
            };
            canvas.DrawCircle(winX + winSize / 2, winY + winSize / 2, winSize * 1.5f, glowPaint);
        }

        using var windowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = isNight ? new SKColor(255, 220, 120) : new SKColor(150, 200, 255)
        };
        canvas.DrawRect(winX, winY, winSize, winSize, windowPaint);

        // Window cross
        using var framePaint = new SKPaint
        {
            IsAntialias = true,
            Color = isNight ? new SKColor(80, 50, 40) : new SKColor(140, 100, 60),
            StrokeWidth = 1.5f,
            Style = SKPaintStyle.Stroke
        };
        canvas.DrawLine(winX, winY + winSize / 2, winX + winSize, winY + winSize / 2, framePaint);
        canvas.DrawLine(winX + winSize / 2, winY, winX + winSize / 2, winY + winSize, framePaint);

        // Tree next to house
        float treeX = houseX - houseW * 0.6f;
        float treeTopY = groundY - houseH * 1.2f;

        // Trunk
        using var trunkPaint = new SKPaint
        {
            IsAntialias = true,
            Color = isNight ? new SKColor(60, 35, 20) : new SKColor(120, 80, 40)
        };
        canvas.DrawRect(treeX - 2, groundY - houseH * 0.5f, 5, houseH * 0.5f, trunkPaint);

        // Foliage (circle cluster)
        using var foliagePaint = new SKPaint
        {
            IsAntialias = true,
            Color = isNight ? new SKColor(20, 60, 20) : new SKColor(50, 150, 50)
        };
        float fRadius = houseW * 0.25f;
        canvas.DrawCircle(treeX, treeTopY + fRadius, fRadius, foliagePaint);
        canvas.DrawCircle(treeX - fRadius * 0.6f, treeTopY + fRadius * 1.5f, fRadius * 0.8f, foliagePaint);
        canvas.DrawCircle(treeX + fRadius * 0.6f, treeTopY + fRadius * 1.5f, fRadius * 0.8f, foliagePaint);
    }
}
