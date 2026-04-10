namespace MoonSimulation.Models;

/// <summary>
/// Moon phase angle thresholds (degrees) for consistent phase classification.
/// </summary>
public static class PhaseConstants
{
    public const double NewMoonEnd = 22.5;
    public const double WaxingCrescentEnd = 67.5;
    public const double FirstQuarterEnd = 112.5;
    public const double WaxingGibbousEnd = 157.5;
    public const double FullMoonEnd = 202.5;
    public const double WaningGibbousEnd = 247.5;
    public const double LastQuarterEnd = 292.5;
    public const double WaningCrescentEnd = 337.5;
}

/// <summary>
/// Represents the current state of the orbital simulation.
/// </summary>
public class OrbitalState
{
    /// <summary>
    /// Moon's orbital angle in degrees (0–360).
    /// 0° = New Moon (Moon between Sun and Earth).
    /// 180° = Full Moon (Earth between Sun and Moon).
    /// </summary>
    public double MoonAngleDegrees { get; set; }

    /// <summary>
    /// Earth's rotation angle in degrees (0–360).
    /// Represents where the observer on Earth's equator is.
    /// 0° = observer facing the Sun (noon).
    /// 180° = observer facing away from Sun (midnight).
    /// </summary>
    public double EarthRotationDegrees { get; set; }

    /// <summary>
    /// Phase angle for illumination (0 = New Moon, π = Full Moon).
    /// </summary>
    public double PhaseAngle => MoonAngleDegrees * Math.PI / 180.0;

    /// <summary>
    /// Illumination fraction (0.0 = new moon, 1.0 = full moon).
    /// </summary>
    public double Illumination => (1.0 - Math.Cos(PhaseAngle)) / 2.0;

    /// <summary>
    /// The current phase name and emoji.
    /// </summary>
    public (string Name, string Emoji) Phase => GetPhase(MoonAngleDegrees);

    /// <summary>
    /// Total elapsed simulation days.
    /// </summary>
    public double ElapsedDays { get; set; }

    /// <summary>
    /// Sun elevation for the observer (-1 = midnight, 0 = horizon, 1 = noon).
    /// Based on Earth rotation: cos(rotation) gives 1 at noon, -1 at midnight.
    /// </summary>
    public double SunElevation => Math.Cos(EarthRotationDegrees * Math.PI / 180.0);

    /// <summary>
    /// True when the observer is on the sunlit side of Earth.
    /// </summary>
    public bool IsDaytime => SunElevation > 0;

    /// <summary>
    /// Time of day description for the observer.
    /// </summary>
    public (string Name, string Emoji) TimeOfDay
    {
        get
        {
            double elev = SunElevation;
            if (elev > 0.3) return ("Day", "☀️");
            // Sun is rising when sin of the earth rotation angle is negative
            // (EarthRotationDegrees between 180° and 360°, i.e. midnight→noon).
            bool isRising = Math.Sin(EarthRotationDegrees * Math.PI / 180.0) < 0;
            if (elev > -0.3) return isRising ? ("Sunrise", "🌅") : ("Sunset", "🌇");
            return ("Night", "🌙");
        }
    }

    /// <summary>
    /// Fractional hour of the day (0-24) for display.
    /// 0/24 = midnight, 6 = sunrise, 12 = noon, 18 = sunset.
    /// </summary>
    public double HourOfDay
    {
        get
        {
            double norm = ((EarthRotationDegrees % 360) + 360) % 360;
            // 0° = noon (12:00), 180° = midnight (0:00)
            double hour = (norm / 360.0 * 24.0 + 12.0) % 24.0;
            return hour;
        }
    }

    private static (string Name, string Emoji) GetPhase(double angle)
    {
        angle = ((angle % 360) + 360) % 360;

        return angle switch
        {
            < PhaseConstants.NewMoonEnd => ("Moon is Hiding!", "🌑"),
            < PhaseConstants.WaxingCrescentEnd => ("A Little Sliver!", "🌒"),
            < PhaseConstants.FirstQuarterEnd => ("Half Moon!", "🌓"),
            < PhaseConstants.WaxingGibbousEnd => ("Almost Full!", "🌔"),
            < PhaseConstants.FullMoonEnd => ("WOW Full Moon!", "🌕"),
            < PhaseConstants.WaningGibbousEnd => ("Getting Smaller!", "🌖"),
            < PhaseConstants.LastQuarterEnd => ("Half Moon!", "🌗"),
            < PhaseConstants.WaningCrescentEnd => ("Tiny Sliver!", "🌘"),
            _ => ("Moon is Hiding!", "🌑")
        };
    }

    /// <summary>
    /// A fun face expression for the Moon based on its phase.
    /// </summary>
    public string MoonFace
    {
        get
        {
            double angle = ((MoonAngleDegrees % 360) + 360) % 360;
            return angle switch
            {
                < PhaseConstants.NewMoonEnd => "😴",    // Hiding/sleeping
                < PhaseConstants.WaxingCrescentEnd => "😊",    // Peeking out
                < PhaseConstants.FirstQuarterEnd => "😃",   // Half and happy
                < PhaseConstants.WaxingGibbousEnd => "😄",   // Getting excited
                < PhaseConstants.FullMoonEnd => "🤩",   // Full moon = WOW
                < PhaseConstants.WaningGibbousEnd => "😌",   // Content
                < PhaseConstants.LastQuarterEnd => "😃",   // Half again
                < PhaseConstants.WaningCrescentEnd => "🥱",   // Getting sleepy
                _ => "😴"
            };
        }
    }
}
