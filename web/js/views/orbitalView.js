import { StarfieldRenderer } from '../renderers/starfieldRenderer.js';
import { drawSun, drawSunRays } from '../renderers/sunRenderer.js';
import { drawEarth } from '../renderers/earthRenderer.js';
import { drawMoonBody } from '../renderers/moonRenderer.js';
import { drawLabel, drawOrbitPath } from '../renderers/sphereRenderer.js';

// Top panel: Sun, Earth, and Moon in orbital positions.
export class OrbitalView {
  constructor(canvas) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d');
    this._starfield = new StarfieldRenderer();
  }

  draw(state) {
    const ctx = this.ctx;
    const dpr = window.devicePixelRatio || 1;
    const w = this.canvas.width / dpr;
    const h = this.canvas.height / dpr;
    ctx.clearRect(0, 0, w, h);

    this._starfield.drawStarryBackground(ctx, 0, 0, w, h);

    if (!state) return;

    // Layout
    const sunX = w * 0.15;
    const sunY = h * 0.5;
    const sunRadius = Math.min(w, h) * 0.12;

    const earthX = w * 0.6;
    const earthY = h * 0.5;
    const earthRadius = Math.min(w, h) * 0.08;

    const orbitRadiusX = Math.min(w, h) * 0.25;
    const orbitRadiusY = orbitRadiusX * 0.4;

    const angleRad = (state.moonAngleDegrees * Math.PI) / 180.0;
    const moonX = earthX - orbitRadiusX * Math.cos(angleRad);
    const moonY = earthY + orbitRadiusY * Math.sin(angleRad);
    const moonRadius = Math.min(w, h) * 0.04;

    function sunLightDir(ox, oy) {
      const dx = sunX - ox;
      const dy = sunY - oy;
      const len = Math.sqrt(dx * dx + dy * dy);
      return len > 0 ? [dx / len, dy / len] : [-1, 0];
    }

    // Sun rays
    drawSunRays(ctx, sunX, sunY, sunRadius);

    // Orbit path
    drawOrbitPath(ctx, earthX, earthY, orbitRadiusX, orbitRadiusY);

    // Draw order — Moon behind Earth when in back half of orbit
    const moonBehind = Math.sin(angleRad) < 0;

    if (moonBehind) {
      const [mlx, mly] = sunLightDir(moonX, moonY);
      drawMoonBody(ctx, moonX, moonY, moonRadius, mlx, mly);
      drawLabel(ctx, 'Moon', moonX, moonY - moonRadius - 8, 14, [200, 200, 200]);
    }

    // Sun
    drawSun(ctx, sunX, sunY, sunRadius);
    drawLabel(ctx, 'Sun', sunX, sunY + sunRadius + 20, 15, [255, 220, 100]);

    // Earth
    const [elx, ely] = sunLightDir(earthX, earthY);
    drawEarth(ctx, earthX, earthY, earthRadius, elx, ely, state.earthRotationDegrees);
    drawLabel(ctx, 'Earth', earthX, earthY + earthRadius + 20, 15, [100, 180, 255]);

    if (!moonBehind) {
      const [mlx, mly] = sunLightDir(moonX, moonY);
      drawMoonBody(ctx, moonX, moonY, moonRadius, mlx, mly);
      drawLabel(ctx, 'Moon', moonX, moonY - moonRadius - 8, 14, [200, 200, 200]);
    }

    // Subtle light line from Sun to Moon
    ctx.save();
    ctx.strokeStyle = 'rgba(255,255,150,0.08)';
    ctx.lineWidth = 1;
    ctx.setLineDash([4, 8]);
    ctx.beginPath();
    ctx.moveTo(sunX + sunRadius, sunY);
    ctx.lineTo(moonX, moonY);
    ctx.stroke();
    ctx.setLineDash([]);
    ctx.restore();

    // Day counter
    drawLabel(ctx, `Day ${Math.floor(state.elapsedDays)}`, w - 60, 25, 14, [150, 150, 180]);
  }
}
