using SkiaSharp;

namespace MoonSimulation.Renderers;

/// <summary>
/// Draws a dark starry background with subtle twinkle animation.
/// Instance-based to own the star field state.
/// </summary>
public class StarfieldRenderer
{
    private readonly Random _rng = new(42); // Deterministic stars
    private SKPoint[]? _stars;
    private float[]? _starBrightness;
    private long _twinkleFrame;

    /// <summary>Current twinkle frame counter (read by other renderers for animation sync).</summary>
    public long TwinkleFrame => _twinkleFrame;

    /// <summary>Generated star positions (shared with sky scene for night stars).</summary>
    public SKPoint[]? Stars => _stars;

    /// <summary>Generated star brightness values.</summary>
    public float[]? StarBrightness => _starBrightness;

    /// <summary>
    /// Draws a deep-space starry background with twinkling stars.
    /// </summary>
    public void DrawStarryBackground(SKCanvas canvas, SKRect bounds)
    {
        // Deep space gradient
        using var bgPaint = new SKPaint();
        using var bgShader = SKShader.CreateLinearGradient(
            new SKPoint(bounds.MidX, bounds.Top),
            new SKPoint(bounds.MidX, bounds.Bottom),
            new SKColor[] { new(8, 8, 40), new(2, 2, 18) },
            null, SKShaderTileMode.Clamp);
        bgPaint.Shader = bgShader;
        canvas.DrawRect(bounds, bgPaint);

        // Generate stars once
        if (_stars == null || _stars.Length < 200)
        {
            _stars = new SKPoint[200];
            _starBrightness = new float[200];
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i] = new SKPoint(
                    _rng.NextSingle() * 2000,
                    _rng.NextSingle() * 2000);
                _starBrightness[i] = 0.3f + _rng.NextSingle() * 0.7f;
            }
        }

        _twinkleFrame++;
        using var starPaint = new SKPaint { IsAntialias = true };

        for (int i = 0; i < _stars.Length; i++)
        {
            float x = _stars[i].X % bounds.Width + bounds.Left;
            float y = _stars[i].Y % bounds.Height + bounds.Top;

            // Gentle twinkle
            float twinkle = _starBrightness![i] *
                (0.7f + 0.3f * MathF.Sin((_twinkleFrame + i * 37) * 0.02f));
            byte alpha = (byte)(twinkle * 255);

            starPaint.Color = new SKColor(255, 255, 240, alpha);
            float size = _starBrightness[i] > 0.8f ? 1.8f : 1.0f;
            canvas.DrawCircle(x, y, size, starPaint);
        }
    }
}
