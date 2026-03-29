using SkiaSharp;

namespace MoonSimulation.Renderers;

/// <summary>
/// Orchestrates the "view from Earth" sky scene — what the observer sees.
/// Instance-based because it shares the starfield state.
/// </summary>
public class SkySceneRenderer
{
    private readonly StarfieldRenderer _starfield;

    public SkySceneRenderer(StarfieldRenderer starfield)
    {
        _starfield = starfield;
    }

    /// <summary>
    /// Draws the complete Earth sky view scene.
    /// </summary>
    public void DrawEarthSkyView(SKCanvas canvas, SKRect bounds,
        double sunElevation, double moonAngleDeg, double earthRotationDeg, double illumination,
        (string Name, string Emoji) phase, (string Name, string Emoji) timeOfDay, double hourOfDay)
    {
        float w = bounds.Width;
        float h = bounds.Height;
        float groundY = bounds.Top + h * 0.75f;

        // Sky gradient based on sun elevation
        DrawSkyGradient(canvas, bounds, groundY, sunElevation);

        // Ground
        DrawGround(canvas, bounds, groundY, sunElevation);

        // Sun in the sky (when above horizon)
        if (sunElevation > -0.1)
        {
            float sunArc = (float)(1.0 - (earthRotationDeg % 360) / 180.0);
            float sunSkyX = bounds.Left + w * 0.5f + w * 0.35f * (float)Math.Cos(earthRotationDeg * Math.PI / 180.0);
            float sunSkyY = groundY - (groundY - bounds.Top) * 0.8f * Math.Max(0, (float)sunElevation);

            if (sunElevation > 0)
            {
                float sunR = Math.Min(w, h) * 0.06f;
                // Sun glow
                using var glowPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = new SKColor(255, 230, 100, (byte)(40 * sunElevation)),
                    MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, sunR * 2)
                };
                canvas.DrawCircle(sunSkyX, sunSkyY, sunR * 3, glowPaint);

                // Sun disk
                using var sunPaint = new SKPaint { IsAntialias = true };
                using var sunShader = SKShader.CreateRadialGradient(
                    new SKPoint(sunSkyX, sunSkyY), sunR,
                    new SKColor[] { new(255, 255, 230), new(255, 200, 50) },
                    null, SKShaderTileMode.Clamp);
                sunPaint.Shader = sunShader;
                canvas.DrawCircle(sunSkyX, sunSkyY, sunR, sunPaint);
            }
        }

        // Moon in the sky — visible when above observer's horizon AND sufficiently illuminated.
        // moonAngleDeg: Moon's orbital angle (0° = between Sun and Earth, 180° = far side)
        // earthRotationDeg: observer's facing (0° = facing Sun/noon, 180° = away/midnight)
        // The angular difference tells us how far the Moon is from the observer's zenith.
        double moonObserverAngle = (moonAngleDeg - earthRotationDeg) * Math.PI / 180.0;
        double moonElevation = Math.Cos(moonObserverAngle);

        // Moon is only visually noticeable if:
        // 1. Above the horizon (elevation > 0)
        // 2. Has enough illumination to be seen (near New Moon it's invisible)
        // 3. During daytime, needs significant illumination to stand out against bright sky
        double minIlluminationToSee = sunElevation > 0.2 ? 0.25 : 0.03;
        bool moonVisible = moonElevation > 0.05 && illumination > minIlluminationToSee;

        if (moonVisible)
        {
            float moonSkyX = bounds.Left + w * 0.5f - w * 0.3f * (float)Math.Sin(moonObserverAngle);
            float moonSkyY = groundY - (groundY - bounds.Top) * 0.7f * (float)moonElevation;
            float moonR = Math.Min(w, h) * 0.055f;

            // Fade the moon based on how close it is to the horizon and its illumination
            float moonAlpha = (float)Math.Min(1.0, moonElevation * 2) * (float)Math.Min(1.0, illumination * 3);

            DrawMiniMoonPhase(canvas, new SKPoint(moonSkyX, moonSkyY), moonR,
                moonAngleDeg, sunElevation, moonAlpha);
        }

        // Stars (only visible at night)
        if (sunElevation < 0.2)
        {
            float starAlpha = (float)Math.Clamp((0.2 - sunElevation) / 0.4, 0, 1);
            DrawSkyStars(canvas, bounds, groundY, starAlpha);

            // Shooting star at night
            if (sunElevation < -0.2)
                DrawShootingStar(canvas, bounds, groundY, _starfield.TwinkleFrame);
        }

        // Clouds during daytime
        SceneryRenderer.DrawClouds(canvas, bounds, groundY, sunElevation, _starfield.TwinkleFrame);

        // Little house on the ground
        SceneryRenderer.DrawLittleHouse(canvas, bounds, groundY, sunElevation);

        // Time label
        int hours = (int)hourOfDay;
        int mins = (int)((hourOfDay - hours) * 60);
        string timeStr = $"{hours:D2}:{mins:D2}";

        SphereRenderer.DrawLabel(canvas, timeStr,
            new SKPoint(bounds.Left + w * 0.5f, bounds.Top + 22),
            16, new SKColor(230, 230, 230));

        // Day/Night label
        SphereRenderer.DrawLabel(canvas, timeOfDay.Name,
            new SKPoint(bounds.Left + w * 0.5f, bounds.Top + 44),
            14, new SKColor(200, 200, 220));
    }

    /// <summary>
    /// Draws a small moon with the correct phase for the sky view.
    /// Delegates shadow to MoonPhaseRenderer.DrawPhaseOverlay.
    /// </summary>
    public static void DrawMiniMoonPhase(SKCanvas canvas, SKPoint center, float radius,
        double moonAngleDeg, double sunElevation, float alpha = 1f)
    {
        // Moon glow (brighter at night), scaled by alpha
        float glowAlpha = (sunElevation < 0 ? 0.4f : 0.15f) * alpha;
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(200, 200, 220, (byte)(glowAlpha * 255)),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, radius * 0.8f)
        };
        canvas.DrawCircle(center, radius * 1.5f, glowPaint);

        // Moon disk
        byte diskAlpha = (byte)(alpha * 255);
        using var moonPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(220, 220, 215, diskAlpha)
        };
        canvas.DrawCircle(center, radius, moonPaint);

        // Phase shadow — uses the shared terminator algorithm
        byte shadowAlpha = (byte)(alpha * (sunElevation < 0 ? 230 : 200));
        var shadowColor = sunElevation < 0
            ? new SKColor(5, 5, 25, shadowAlpha)
            : new SKColor(100, 140, 200, shadowAlpha);
        MoonPhaseRenderer.DrawPhaseOverlay(canvas, center, radius, moonAngleDeg, shadowColor);
    }

    public static void DrawSkyGradient(SKCanvas canvas, SKRect bounds, float groundY,
        double sunElevation)
    {
        SKColor topColor, bottomColor;

        if (sunElevation > 0.3)
        {
            topColor = new SKColor(40, 100, 220);
            bottomColor = new SKColor(135, 190, 250);
        }
        else if (sunElevation > 0.0)
        {
            float t = (float)(sunElevation / 0.3);
            topColor = SphereRenderer.BlendColor(new SKColor(30, 30, 80), new SKColor(40, 100, 220), t);
            bottomColor = SphereRenderer.BlendColor(new SKColor(220, 100, 40), new SKColor(135, 190, 250), t);
        }
        else if (sunElevation > -0.3)
        {
            float t = (float)((sunElevation + 0.3) / 0.3);
            topColor = SphereRenderer.BlendColor(new SKColor(8, 8, 35), new SKColor(30, 30, 80), t);
            bottomColor = SphereRenderer.BlendColor(new SKColor(20, 15, 40), new SKColor(220, 100, 40), t);
        }
        else
        {
            topColor = new SKColor(5, 5, 25);
            bottomColor = new SKColor(10, 10, 35);
        }

        using var skyPaint = new SKPaint();
        using var skyShader = SKShader.CreateLinearGradient(
            new SKPoint(bounds.MidX, bounds.Top),
            new SKPoint(bounds.MidX, groundY),
            new SKColor[] { topColor, bottomColor },
            null, SKShaderTileMode.Clamp);
        skyPaint.Shader = skyShader;
        canvas.DrawRect(new SKRect(bounds.Left, bounds.Top, bounds.Right, groundY), skyPaint);
    }

    public static void DrawGround(SKCanvas canvas, SKRect bounds, float groundY,
        double sunElevation)
    {
        SKColor groundTop, groundBottom;

        if (sunElevation > 0.1)
        {
            groundTop = new SKColor(40, 120, 40);
            groundBottom = new SKColor(30, 80, 30);
        }
        else if (sunElevation > -0.1)
        {
            float t = (float)((sunElevation + 0.1) / 0.2);
            groundTop = SphereRenderer.BlendColor(new SKColor(15, 40, 15), new SKColor(40, 120, 40), t);
            groundBottom = SphereRenderer.BlendColor(new SKColor(10, 25, 10), new SKColor(30, 80, 30), t);
        }
        else
        {
            groundTop = new SKColor(15, 40, 15);
            groundBottom = new SKColor(10, 25, 10);
        }

        using var groundPaint = new SKPaint();
        using var groundShader = SKShader.CreateLinearGradient(
            new SKPoint(bounds.MidX, groundY),
            new SKPoint(bounds.MidX, bounds.Bottom),
            new SKColor[] { groundTop, groundBottom },
            null, SKShaderTileMode.Clamp);
        groundPaint.Shader = groundShader;
        canvas.DrawRect(new SKRect(bounds.Left, groundY, bounds.Right, bounds.Bottom), groundPaint);

        // Horizon line glow during sunrise/sunset
        if (sunElevation > -0.1 && sunElevation < 0.2)
        {
            using var horizonPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 180, 80, (byte)(60 * (1 - Math.Abs(sunElevation) * 5))),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
            };
            canvas.DrawLine(bounds.Left, groundY, bounds.Right, groundY, horizonPaint);
        }
    }

    public void DrawSkyStars(SKCanvas canvas, SKRect bounds, float groundY, float alpha)
    {
        if (_starfield.Stars == null) return;

        using var starPaint = new SKPaint { IsAntialias = true };

        for (int i = 0; i < 60; i++)
        {
            float x = _starfield.Stars[i].X % bounds.Width + bounds.Left;
            float y = _starfield.Stars[i].Y % (groundY - bounds.Top) + bounds.Top;

            float twinkle = _starfield.StarBrightness![i] *
                (0.7f + 0.3f * MathF.Sin((_starfield.TwinkleFrame + i * 37) * 0.02f));
            byte a = (byte)(twinkle * alpha * 255);

            starPaint.Color = new SKColor(255, 255, 240, a);
            canvas.DrawCircle(x, y, _starfield.StarBrightness[i] > 0.8f ? 1.5f : 0.8f, starPaint);
        }
    }

    /// <summary>
    /// Draws a shooting star animation at night.
    /// </summary>
    public static void DrawShootingStar(SKCanvas canvas, SKRect bounds, float groundY, long frame)
    {
        int cycle = (int)(frame % 300);
        if (cycle > 30) return;

        float progress = cycle / 30f;
        float startX = bounds.Left + bounds.Width * 0.7f;
        float startY = bounds.Top + 20;
        float endX = bounds.Left + bounds.Width * 0.3f;
        float endY = groundY * 0.4f;

        float curX = startX + (endX - startX) * progress;
        float curY = startY + (endY - startY) * progress;

        // Tail
        float tailLen = 30f * (1 - progress);
        float tailX = curX - (endX - startX) / (endY - startY) * -tailLen;
        float tailY = curY - tailLen;

        using var starPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            StrokeCap = SKStrokeCap.Round
        };
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(tailX, tailY), new SKPoint(curX, curY),
            new SKColor[] { SKColors.Transparent, new SKColor(255, 255, 220, (byte)(200 * (1 - progress))) },
            null, SKShaderTileMode.Clamp);
        starPaint.Shader = shader;
        canvas.DrawLine(tailX, tailY, curX, curY, starPaint);

        // Head glow
        using var headPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(255, 255, 230, (byte)(255 * (1 - progress))),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3)
        };
        canvas.DrawCircle(curX, curY, 2, headPaint);
    }
}
