using SkiaSharp;

namespace MoonSimulation.Helpers;

/// <summary>
/// Shared SkiaSharp rendering utilities for space objects.
/// </summary>
public static class SpaceRenderer
{
    private static readonly Random _rng = new(42); // Deterministic stars
    private static SKPoint[]? _stars;
    private static float[]? _starBrightness;
    private static long _twinkleFrame;

    /// <summary>
    /// Draws a dark starry background with subtle twinkle.
    /// </summary>
    public static void DrawStarryBackground(SKCanvas canvas, SKRect bounds)
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

    /// <summary>
    /// Draws a 3D-shaded sphere with a light direction.
    /// </summary>
    public static void DrawSphere(SKCanvas canvas, SKPoint center, float radius,
        SKColor baseColor, SKPoint lightDir, float ambientLight = 0.15f)
    {
        using var paint = new SKPaint { IsAntialias = true };

        // Offset highlight toward the light source
        float highlightX = center.X + lightDir.X * radius * 0.35f;
        float highlightY = center.Y + lightDir.Y * radius * 0.35f;

        // Radial gradient from highlight to dark edge
        var lightColor = Lighten(baseColor, 0.6f);
        var darkColor = Darken(baseColor, 0.7f);

        using var shader = SKShader.CreateRadialGradient(
            new SKPoint(highlightX, highlightY),
            radius * 1.8f,
            new SKColor[] { lightColor, baseColor, darkColor, Darken(baseColor, 0.9f) },
            new float[] { 0f, 0.35f, 0.75f, 1f },
            SKShaderTileMode.Clamp);

        paint.Shader = shader;
        canvas.DrawCircle(center, radius, paint);
    }

    /// <summary>
    /// Draws the Sun with corona glow.
    /// </summary>
    public static void DrawSun(SKCanvas canvas, SKPoint center, float radius)
    {
        // Outer glow layers
        for (int i = 3; i >= 0; i--)
        {
            float glowRadius = radius * (1.5f + i * 0.4f);
            byte alpha = (byte)(30 - i * 6);
            using var glowPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 200, 50, alpha),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, glowRadius * 0.3f)
            };
            canvas.DrawCircle(center, glowRadius, glowPaint);
        }

        // Sun body with radial gradient
        using var sunPaint = new SKPaint { IsAntialias = true };
        using var sunShader = SKShader.CreateRadialGradient(
            new SKPoint(center.X - radius * 0.2f, center.Y - radius * 0.2f),
            radius * 1.5f,
            new SKColor[] {
                new(255, 255, 220),
                new(255, 220, 80),
                new(255, 160, 20),
                new(200, 100, 0)
            },
            new float[] { 0f, 0.3f, 0.7f, 1f },
            SKShaderTileMode.Clamp);

        sunPaint.Shader = sunShader;
        canvas.DrawCircle(center, radius, sunPaint);
    }

    /// <summary>
    /// Draws Earth with a blue/green sphere, day/night terminator, city lights, and observer dot.
    /// </summary>
    public static void DrawEarth(SKCanvas canvas, SKPoint center, float radius, SKPoint lightDir,
        double earthRotationDeg = 0)
    {
        // Base blue sphere (day side)
        DrawSphere(canvas, center, radius, new SKColor(40, 80, 200), lightDir, 0.2f);

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

        // Day/Night terminator shadow — dark half facing away from Sun
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

        // Observer dot on the equator — rotates with Earth
        DrawObserver(canvas, center, radius, earthRotationDeg, lightDir);
    }

    /// <summary>
    /// Draws the dark (night) half of Earth facing away from the Sun.
    /// </summary>
    private static void DrawDayNightTerminator(SKCanvas canvas, SKPoint center, float radius,
        SKPoint lightDir)
    {
        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddCircle(center.X, center.Y, radius);
        canvas.ClipPath(clipPath);

        // The shadow is on the opposite side from the light.
        // lightDir points TOWARD the Sun, so shadow is in the -lightDir direction.
        float angle = MathF.Atan2(-lightDir.Y, -lightDir.X);

        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(5, 5, 30, 180),
            Style = SKPaintStyle.Fill
        };

        // Draw a half-circle on the dark side
        using var shadowPath = new SKPath();
        float perpAngle1 = angle + MathF.PI / 2;
        float perpAngle2 = angle - MathF.PI / 2;

        shadowPath.MoveTo(
            center.X + MathF.Cos(perpAngle1) * radius,
            center.Y + MathF.Sin(perpAngle1) * radius);

        // Arc across the dark side
        shadowPath.ArcTo(
            new SKRect(center.X - radius, center.Y - radius,
                       center.X + radius, center.Y + radius),
            (perpAngle1 * 180f / MathF.PI),
            -180, false);

        // Terminator line (straight for simplicity at this scale)
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
    private static void DrawCityLights(SKCanvas canvas, SKPoint center, float radius,
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

            // Check if this point is on the dark side
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
    private static void DrawObserver(SKCanvas canvas, SKPoint center, float radius,
        double earthRotationDeg, SKPoint lightDir)
    {
        // Observer is on the equator. Rotation 0° = facing Sun (noon).
        // In the orbital view, the Sun is to the left, so 0° rotation = left side of Earth.
        double rotRad = earthRotationDeg * Math.PI / 180.0;

        // Position on the equator (viewed from above with tilt)
        float obsX = center.X + radius * 0.9f * (float)Math.Cos(rotRad + Math.PI);
        float obsY = center.Y + radius * 0.3f * (float)Math.Sin(rotRad + Math.PI);

        // Check if observer is on the visible side (front of the sphere)
        float sinComponent = (float)Math.Sin(rotRad + Math.PI);
        if (sinComponent > -0.3f) // Visible hemisphere (with tilt)
        {
            // Determine if on day or night side
            float relX = (obsX - center.X) / radius;
            float dot = relX * lightDir.X;
            bool isDay = dot > 0;

            // Observer dot
            using var obsPaint = new SKPaint
            {
                IsAntialias = true,
                Color = isDay ? new SKColor(255, 255, 0, 220) : new SKColor(255, 150, 50, 220)
            };
            canvas.DrawCircle(obsX, obsY, 3.5f, obsPaint);

            // Small "person" label
            string personEmoji = isDay ? "🧍" : "😴";
            DrawLabel(canvas, personEmoji, new SKPoint(obsX, obsY - 10), 10,
                isDay ? new SKColor(255, 255, 200) : new SKColor(200, 200, 255));
        }
    }

    /// <summary>
    /// Draws the "view from Earth" sky scene — shows what the observer sees.
    /// </summary>
    public static void DrawEarthSkyView(SKCanvas canvas, SKRect bounds,
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

        // Moon in the sky (visible at various phases and times)
        // Moon is roughly opposite the Sun for Full Moon, same side for New Moon
        double moonSkyAngle = (earthRotationDeg + moonAngleDeg) * Math.PI / 180.0;
        double moonElevation = -Math.Cos(moonSkyAngle);

        if (moonElevation > 0.05)
        {
            float moonSkyX = bounds.Left + w * 0.5f - w * 0.3f * (float)Math.Cos(moonSkyAngle);
            float moonSkyY = groundY - (groundY - bounds.Top) * 0.7f * (float)moonElevation;
            float moonR = Math.Min(w, h) * 0.055f;

            // Draw mini moon with phase
            DrawMiniMoonPhase(canvas, new SKPoint(moonSkyX, moonSkyY), moonR,
                moonAngleDeg, sunElevation);
        }

        // Stars (only visible at night)
        if (sunElevation < 0.2)
        {
            float starAlpha = (float)Math.Clamp((0.2 - sunElevation) / 0.4, 0, 1);
            DrawSkyStars(canvas, bounds, groundY, starAlpha);
        }

        // Time label
        int hours = (int)hourOfDay;
        int mins = (int)((hourOfDay - hours) * 60);
        string timeStr = $"{hours:D2}:{mins:D2}";

        DrawLabel(canvas, $"{timeOfDay.Emoji} {timeStr}",
            new SKPoint(bounds.Left + w * 0.5f, bounds.Top + 22),
            16, new SKColor(230, 230, 230));

        // Day/Night label
        DrawLabel(canvas, timeOfDay.Name,
            new SKPoint(bounds.Left + w * 0.5f, bounds.Top + 44),
            13, new SKColor(200, 200, 220));
    }

    private static void DrawSkyGradient(SKCanvas canvas, SKRect bounds, float groundY,
        double sunElevation)
    {
        SKColor topColor, bottomColor;

        if (sunElevation > 0.3)
        {
            // Full day — bright blue sky
            topColor = new SKColor(40, 100, 220);
            bottomColor = new SKColor(135, 190, 250);
        }
        else if (sunElevation > 0.0)
        {
            // Sunrise/sunset — orange gradient
            float t = (float)(sunElevation / 0.3);
            topColor = BlendColor(new SKColor(30, 30, 80), new SKColor(40, 100, 220), t);
            bottomColor = BlendColor(new SKColor(220, 100, 40), new SKColor(135, 190, 250), t);
        }
        else if (sunElevation > -0.3)
        {
            // Twilight — deep blue to orange at horizon
            float t = (float)((sunElevation + 0.3) / 0.3);
            topColor = BlendColor(new SKColor(8, 8, 35), new SKColor(30, 30, 80), t);
            bottomColor = BlendColor(new SKColor(20, 15, 40), new SKColor(220, 100, 40), t);
        }
        else
        {
            // Full night — dark
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

    private static void DrawGround(SKCanvas canvas, SKRect bounds, float groundY,
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
            groundTop = BlendColor(new SKColor(15, 40, 15), new SKColor(40, 120, 40), t);
            groundBottom = BlendColor(new SKColor(10, 25, 10), new SKColor(30, 80, 30), t);
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

    private static void DrawSkyStars(SKCanvas canvas, SKRect bounds, float groundY,
        float alpha)
    {
        if (_stars == null) return;

        using var starPaint = new SKPaint { IsAntialias = true };

        for (int i = 0; i < 60; i++)
        {
            float x = _stars[i].X % bounds.Width + bounds.Left;
            float y = _stars[i].Y % (groundY - bounds.Top) + bounds.Top;

            float twinkle = _starBrightness![i] *
                (0.7f + 0.3f * MathF.Sin((_twinkleFrame + i * 37) * 0.02f));
            byte a = (byte)(twinkle * alpha * 255);

            starPaint.Color = new SKColor(255, 255, 240, a);
            canvas.DrawCircle(x, y, _starBrightness[i] > 0.8f ? 1.5f : 0.8f, starPaint);
        }
    }

    /// <summary>
    /// Draws a small moon with the correct phase for the sky view.
    /// </summary>
    private static void DrawMiniMoonPhase(SKCanvas canvas, SKPoint center, float radius,
        double moonAngleDeg, double sunElevation)
    {
        // Moon glow (brighter at night)
        float glowAlpha = sunElevation < 0 ? 0.4f : 0.15f;
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(200, 200, 220, (byte)(glowAlpha * 255)),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, radius * 0.8f)
        };
        canvas.DrawCircle(center, radius * 1.5f, glowPaint);

        // Moon disk
        using var moonPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(220, 220, 215)
        };
        canvas.DrawCircle(center, radius, moonPaint);

        // Phase shadow using same terminator technique
        double angle = ((moonAngleDeg % 360) + 360) % 360;
        double phaseRad = angle * Math.PI / 180.0;
        float terminatorScale = (float)Math.Cos(phaseRad);

        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddCircle(center.X, center.Y, radius);
        canvas.ClipPath(clipPath);

        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = sunElevation < 0
                ? new SKColor(5, 5, 25, 230)
                : new SKColor(100, 140, 200, 200),
            Style = SKPaintStyle.Fill
        };

        using var shadowPath = new SKPath();

        if (angle <= 180)
        {
            shadowPath.MoveTo(center.X, center.Y - radius);
            shadowPath.ArcTo(
                new SKRect(center.X - radius, center.Y - radius,
                           center.X + radius, center.Y + radius),
                -90, -180, false);
            float termW = radius * Math.Abs(terminatorScale);
            shadowPath.ArcTo(
                new SKRect(center.X - termW, center.Y - radius,
                           center.X + termW, center.Y + radius),
                90, terminatorScale >= 0 ? -180 : 180, false);
            shadowPath.Close();
        }
        else
        {
            shadowPath.MoveTo(center.X, center.Y - radius);
            shadowPath.ArcTo(
                new SKRect(center.X - radius, center.Y - radius,
                           center.X + radius, center.Y + radius),
                -90, 180, false);
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

    private static SKColor BlendColor(SKColor a, SKColor b, float t)
    {
        t = Math.Clamp(t, 0, 1);
        return new SKColor(
            (byte)(a.Red + (b.Red - a.Red) * t),
            (byte)(a.Green + (b.Green - a.Green) * t),
            (byte)(a.Blue + (b.Blue - a.Blue) * t),
            (byte)(a.Alpha + (b.Alpha - a.Alpha) * t));
    }

    /// <summary>
    /// Draws the Moon as a gray sphere with crater dots.
    /// </summary>
    public static void DrawMoonBody(SKCanvas canvas, SKPoint center, float radius, SKPoint lightDir)
    {
        DrawSphere(canvas, center, radius, new SKColor(180, 180, 175), lightDir, 0.1f);

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

    /// <summary>
    /// Draws a dashed orbit path (ellipse).
    /// </summary>
    public static void DrawOrbitPath(SKCanvas canvas, SKPoint center, float radiusX, float radiusY)
    {
        using var pathPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            Color = new SKColor(100, 100, 150, 80),
            PathEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0)
        };

        canvas.DrawOval(center, new SKSize(radiusX, radiusY), pathPaint);
    }

    /// <summary>
    /// Draws a text label with shadow.
    /// </summary>
    public static void DrawLabel(SKCanvas canvas, string text, SKPoint position, float fontSize,
        SKColor color, SKTextAlign align = SKTextAlign.Center)
    {
        using var font = new SKFont(SKTypeface.Default, fontSize);
        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(0, 0, 0, 150)
        };
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = color
        };

        canvas.DrawText(text, position.X + 1, position.Y + 1, align, font, shadowPaint);
        canvas.DrawText(text, position.X, position.Y, align, font, textPaint);
    }

    #region Color Helpers

    private static SKColor Lighten(SKColor color, float amount)
    {
        return new SKColor(
            (byte)Math.Min(255, color.Red + (255 - color.Red) * amount),
            (byte)Math.Min(255, color.Green + (255 - color.Green) * amount),
            (byte)Math.Min(255, color.Blue + (255 - color.Blue) * amount),
            color.Alpha);
    }

    private static SKColor Darken(SKColor color, float amount)
    {
        return new SKColor(
            (byte)(color.Red * (1 - amount)),
            (byte)(color.Green * (1 - amount)),
            (byte)(color.Blue * (1 - amount)),
            color.Alpha);
    }

    #endregion
}
