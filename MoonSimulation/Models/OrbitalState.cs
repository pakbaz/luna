namespace MoonSimulation.Models;

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

    private static (string Name, string Emoji) GetPhase(double angle)
    {
        // Normalize to 0-360
        angle = ((angle % 360) + 360) % 360;

        return angle switch
        {
            < 22.5 => ("New Moon", "🌑"),
            < 67.5 => ("Waxing Crescent", "🌒"),
            < 112.5 => ("First Quarter", "🌓"),
            < 157.5 => ("Waxing Gibbous", "🌔"),
            < 202.5 => ("Full Moon", "🌕"),
            < 247.5 => ("Waning Gibbous", "🌖"),
            < 292.5 => ("Last Quarter", "🌗"),
            < 337.5 => ("Waning Crescent", "🌘"),
            _ => ("New Moon", "🌑")
        };
    }
}
