import { OrbitalState } from '../models/orbitalState.js';

// Drives the simulation loop using requestAnimationFrame.
export class SimulationEngine {
  static OrbitPeriodDays = 29.5;
  static BaseSecondsPerOrbit = 30.0;

  constructor() {
    this.state = new OrbitalState();
    this._isRunning = false;
    this._animFrameId = null;
    this._lastTimestamp = null;
    this.speed = 1.0;
    this.onTick = null;
  }

  get isRunning() {
    return this._isRunning;
  }

  start() {
    if (this._animFrameId !== null) return;
    this._isRunning = true;
    this._lastTimestamp = null;
    this._animFrameId = requestAnimationFrame((t) => this._loop(t));
  }

  togglePause() {
    if (this._isRunning) {
      this._isRunning = false;
      if (this._animFrameId !== null) {
        cancelAnimationFrame(this._animFrameId);
        this._animFrameId = null;
      }
    } else {
      this._isRunning = true;
      this._lastTimestamp = null;
      this._animFrameId = requestAnimationFrame((t) => this._loop(t));
    }
  }

  _loop(timestamp) {
    if (!this._isRunning) return;

    if (this._lastTimestamp !== null) {
      const dtSeconds = (timestamp - this._lastTimestamp) / 1000.0;
      // Cap dt to avoid huge jumps on tab switch
      const clampedDt = Math.min(dtSeconds, 0.1);
      this._tick(clampedDt);
    }
    this._lastTimestamp = timestamp;
    this._animFrameId = requestAnimationFrame((t) => this._loop(t));
  }

  _tick(dt) {
    const degreesPerSecond = 360.0 / SimulationEngine.BaseSecondsPerOrbit;

    // Advance moon angle
    const deltaDegrees = degreesPerSecond * this.speed * dt;
    this.state.moonAngleDegrees =
      (this.state.moonAngleDegrees + deltaDegrees) % 360.0;

    // Advance Earth rotation — Earth spins ~29.5 times per lunar orbit
    const earthDegreesPerSecond =
      degreesPerSecond * SimulationEngine.OrbitPeriodDays;
    const earthDelta = earthDegreesPerSecond * this.speed * dt;
    this.state.earthRotationDegrees =
      (this.state.earthRotationDegrees + earthDelta) % 360.0;

    // Track elapsed days
    const daysPerSecond =
      SimulationEngine.OrbitPeriodDays / SimulationEngine.BaseSecondsPerOrbit;
    this.state.elapsedDays += daysPerSecond * this.speed * dt;

    if (this.onTick) this.onTick();
  }
}
