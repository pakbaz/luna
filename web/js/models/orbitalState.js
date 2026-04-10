// Moon phase angle thresholds (degrees) for consistent phase classification.
export const PhaseConstants = {
  NewMoonEnd: 22.5,
  WaxingCrescentEnd: 67.5,
  FirstQuarterEnd: 112.5,
  WaxingGibbousEnd: 157.5,
  FullMoonEnd: 202.5,
  WaningGibbousEnd: 247.5,
  LastQuarterEnd: 292.5,
  WaningCrescentEnd: 337.5,
};

// Represents the current state of the orbital simulation.
export class OrbitalState {
  constructor() {
    // Moon's orbital angle in degrees (0–360).
    // 0° = New Moon (Moon between Sun and Earth).
    // 180° = Full Moon (Earth between Sun and Moon).
    this.moonAngleDegrees = 0;

    // Earth's rotation angle in degrees (0–360).
    // 0° = observer facing the Sun (noon).  180° = away from Sun (midnight).
    this.earthRotationDegrees = 0;

    // Total elapsed simulation days.
    this.elapsedDays = 0;
  }

  // Phase angle for illumination (0 = New Moon, π = Full Moon).
  get phaseAngle() {
    return (this.moonAngleDegrees * Math.PI) / 180.0;
  }

  // Illumination fraction (0.0 = new moon, 1.0 = full moon).
  get illumination() {
    return (1.0 - Math.cos(this.phaseAngle)) / 2.0;
  }

  // The current phase name and emoji.
  get phase() {
    return OrbitalState.getPhase(this.moonAngleDegrees);
  }

  // Sun elevation for the observer (-1 = midnight, 0 = horizon, 1 = noon).
  get sunElevation() {
    return Math.cos((this.earthRotationDegrees * Math.PI) / 180.0);
  }

  // True when the observer is on the sunlit side of Earth.
  get isDaytime() {
    return this.sunElevation > 0;
  }

  // Time of day description for the observer.
  get timeOfDay() {
    const elev = this.sunElevation;
    if (elev > 0.3) return { name: 'Day', emoji: '☀️' };
    // Sun is rising when sin of the earth rotation angle is negative
    // (earthRotationDegrees between 180° and 360°, i.e. midnight→noon).
    const isRising = Math.sin((this.earthRotationDegrees * Math.PI) / 180.0) < 0;
    if (elev > -0.3) return isRising
      ? { name: 'Sunrise', emoji: '🌅' }
      : { name: 'Sunset', emoji: '🌇' };
    return { name: 'Night', emoji: '🌙' };
  }

  // Fractional hour of the day (0-24).
  // 0/24 = midnight, 6 = sunrise, 12 = noon, 18 = sunset.
  get hourOfDay() {
    const norm = ((this.earthRotationDegrees % 360) + 360) % 360;
    return ((norm / 360.0) * 24.0 + 12.0) % 24.0;
  }

  // A fun face expression for the Moon based on its phase.
  get moonFace() {
    const angle = ((this.moonAngleDegrees % 360) + 360) % 360;
    if (angle < PhaseConstants.NewMoonEnd) return '😴';
    if (angle < PhaseConstants.WaxingCrescentEnd) return '😊';
    if (angle < PhaseConstants.FirstQuarterEnd) return '😃';
    if (angle < PhaseConstants.WaxingGibbousEnd) return '😄';
    if (angle < PhaseConstants.FullMoonEnd) return '🤩';
    if (angle < PhaseConstants.WaningGibbousEnd) return '😌';
    if (angle < PhaseConstants.LastQuarterEnd) return '😃';
    if (angle < PhaseConstants.WaningCrescentEnd) return '🥱';
    return '😴';
  }

  static getPhase(angleDeg) {
    const angle = ((angleDeg % 360) + 360) % 360;
    if (angle < PhaseConstants.NewMoonEnd)
      return { name: 'Moon is Hiding!', emoji: '🌑' };
    if (angle < PhaseConstants.WaxingCrescentEnd)
      return { name: 'A Little Sliver!', emoji: '🌒' };
    if (angle < PhaseConstants.FirstQuarterEnd)
      return { name: 'Half Moon!', emoji: '🌓' };
    if (angle < PhaseConstants.WaxingGibbousEnd)
      return { name: 'Almost Full!', emoji: '🌔' };
    if (angle < PhaseConstants.FullMoonEnd)
      return { name: 'WOW Full Moon!', emoji: '🌕' };
    if (angle < PhaseConstants.WaningGibbousEnd)
      return { name: 'Getting Smaller!', emoji: '🌖' };
    if (angle < PhaseConstants.LastQuarterEnd)
      return { name: 'Half Moon!', emoji: '🌗' };
    if (angle < PhaseConstants.WaningCrescentEnd)
      return { name: 'Tiny Sliver!', emoji: '🌘' };
    return { name: 'Moon is Hiding!', emoji: '🌑' };
  }
}
