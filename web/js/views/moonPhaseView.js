import { StarfieldRenderer } from '../renderers/starfieldRenderer.js';
import { drawPhaseOverlay } from '../renderers/moonPhaseRenderer.js';
import { getMoonFaceExpression, drawCuteFace } from '../renderers/faceRenderer.js';
import { drawLabel } from '../renderers/sphereRenderer.js';

// Bottom-left panel: Moon as seen from Earth with phase illumination.
export class MoonPhaseView {
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

    const moonRadius = Math.min(w, h) * 0.3;
    const cx = w * 0.5;
    const cy = h * 0.4;

    // Base moon body
    this._drawFullMoon(ctx, cx, cy, moonRadius);

    // Phase shadow overlay
    drawPhaseOverlay(ctx, cx, cy, moonRadius,
      state.moonAngleDegrees, 'rgba(5,5,20,0.86)');

    // Limb highlight
    ctx.strokeStyle = 'rgba(220,220,215,0.24)';
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    ctx.arc(cx, cy, moonRadius, 0, Math.PI * 2);
    ctx.stroke();

    // Cute face
    const faceExpression = getMoonFaceExpression(state.moonAngleDegrees);
    if (state.illumination > 0.15) {
      drawCuteFace(ctx, cx, cy, moonRadius * 0.7, faceExpression);
    }

    // Phase label
    const { name: phaseName } = state.phase;
    drawLabel(ctx, phaseName, w * 0.5, cy + moonRadius + 40, 22, [230, 230, 230]);
  }

  _drawFullMoon(ctx, cx, cy, radius) {
    // Base gray sphere with gentle 3D shading
    const grad = ctx.createRadialGradient(cx, cy, 0, cx, cy, radius);
    grad.addColorStop(0, 'rgb(200,200,195)');
    grad.addColorStop(0.6, 'rgb(170,170,165)');
    grad.addColorStop(1, 'rgb(130,130,125)');
    ctx.fillStyle = grad;
    ctx.beginPath();
    ctx.arc(cx, cy, radius, 0, Math.PI * 2);
    ctx.fill();

    // Mare patches
    this._drawMaria(ctx, cx, cy, radius);

    // Small crater dots
    ctx.fillStyle = 'rgba(140,140,135,0.35)';
    const craters = [
      [-0.2, -0.15, 0.06],
      [0.1, -0.25, 0.04],
      [0.25, 0.1, 0.08],
      [-0.08, 0.25, 0.05],
      [0.02, 0.02, 0.1],
      [-0.3, 0.1, 0.04],
      [0.15, 0.3, 0.03],
      [-0.15, -0.35, 0.03],
    ];

    for (const [dx, dy, r] of craters) {
      ctx.beginPath();
      ctx.arc(cx + dx * radius, cy + dy * radius, r * radius, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  _drawMaria(ctx, cx, cy, radius) {
    ctx.fillStyle = 'rgba(120,120,115,0.24)';

    // Mare Imbrium (top-left)
    ctx.beginPath();
    ctx.ellipse(
      cx - radius * 0.15, cy - radius * 0.3,
      radius * 0.25, radius * 0.2, 0, 0, Math.PI * 2
    );
    ctx.fill();

    // Mare Serenitatis (top-right)
    ctx.beginPath();
    ctx.ellipse(
      cx + radius * 0.2, cy - radius * 0.2,
      radius * 0.15, radius * 0.15, 0, 0, Math.PI * 2
    );
    ctx.fill();

    // Mare Tranquillitatis (center-right)
    ctx.beginPath();
    ctx.ellipse(
      cx + radius * 0.2, cy + radius * 0.05,
      radius * 0.2, radius * 0.15, 0, 0, Math.PI * 2
    );
    ctx.fill();
  }
}
