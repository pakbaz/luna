import { drawSphere } from './sphereRenderer.js';

// Renders the Moon body in the orbital view with craters.

export function drawMoonBody(ctx, cx, cy, radius, lightDirX, lightDirY) {
  drawSphere(ctx, cx, cy, radius, [180, 180, 175], lightDirX, lightDirY);

  // Crater dots
  ctx.fillStyle = 'rgba(140,140,135,0.31)';

  const craters = [
    [-0.25, -0.15, 0.08],
    [0.15, -0.3, 0.06],
    [0.3, 0.1, 0.1],
    [-0.1, 0.3, 0.07],
    [0.05, 0.05, 0.12],
    [-0.35, 0.15, 0.05],
  ];

  for (const [dx, dy, r] of craters) {
    const dist = Math.sqrt(dx * dx + dy * dy);
    if (dist < 0.85) {
      ctx.beginPath();
      ctx.arc(cx + dx * radius, cy + dy * radius, r * radius, 0, Math.PI * 2);
      ctx.fill();
    }
  }
}
