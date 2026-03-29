import { drawCuteFace } from './faceRenderer.js';

// Renders the Sun with corona glow, rays, and a happy face.

export function drawSun(ctx, cx, cy, radius) {
  // Outer glow layers
  for (let i = 3; i >= 0; i--) {
    const glowRadius = radius * (1.5 + i * 0.4);
    const alpha = (30 - i * 6) / 255;
    ctx.save();
    ctx.shadowBlur = glowRadius * 0.3;
    ctx.shadowColor = `rgba(255,200,50,${alpha})`;
    ctx.fillStyle = `rgba(255,200,50,${alpha})`;
    ctx.beginPath();
    ctx.arc(cx, cy, glowRadius, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }

  // Sun body with radial gradient
  const grad = ctx.createRadialGradient(
    cx - radius * 0.2, cy - radius * 0.2, 0,
    cx - radius * 0.2, cy - radius * 0.2, radius * 1.5
  );
  grad.addColorStop(0, 'rgb(255,255,220)');
  grad.addColorStop(0.3, 'rgb(255,220,80)');
  grad.addColorStop(0.7, 'rgb(255,160,20)');
  grad.addColorStop(1, 'rgb(200,100,0)');

  ctx.fillStyle = grad;
  ctx.beginPath();
  ctx.arc(cx, cy, radius, 0, Math.PI * 2);
  ctx.fill();

  // Happy face on Sun
  drawCuteFace(ctx, cx, cy, radius, 'happy');
}

export function drawSunRays(ctx, cx, cy, radius) {
  ctx.save();
  ctx.lineWidth = 1.5;
  ctx.lineCap = 'round';

  for (let i = 0; i < 12; i++) {
    const angle = (i * Math.PI * 2) / 12;
    const innerR = radius * 1.3;
    const outerR = radius * 1.7;

    const x1 = cx + Math.cos(angle) * innerR;
    const y1 = cy + Math.sin(angle) * innerR;
    const x2 = cx + Math.cos(angle) * outerR;
    const y2 = cy + Math.sin(angle) * outerR;

    const alpha = (40 + 20 * Math.sin(angle * 3)) / 255;
    ctx.strokeStyle = `rgba(255,230,100,${alpha})`;
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();
  }
  ctx.restore();
}
