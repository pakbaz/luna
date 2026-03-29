import { SkySceneRenderer } from '../renderers/skySceneRenderer.js';

// Bottom-right panel: What the observer on Earth sees in the sky.
export class EarthSkyView {
  constructor(canvas, starfield) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d');
    this._skyScene = new SkySceneRenderer(starfield);
  }

  draw(state) {
    const ctx = this.ctx;
    const dpr = window.devicePixelRatio || 1;
    const w = this.canvas.width / dpr;
    const h = this.canvas.height / dpr;
    ctx.clearRect(0, 0, w, h);

    if (!state) return;

    this._skyScene.drawEarthSkyView(
      ctx, 0, 0, w, h,
      state.sunElevation,
      state.moonAngleDegrees,
      state.earthRotationDegrees,
      state.illumination,
      state.phase,
      state.timeOfDay,
      state.hourOfDay
    );
  }
}
