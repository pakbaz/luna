import { PhaseConstants } from '../models/orbitalState.js';
import { drawLabel } from './sphereRenderer.js';

// Returns a face expression name based on moon phase angle.
export function getMoonFaceExpression(moonAngleDeg) {
  const angle = ((moonAngleDeg % 360) + 360) % 360;
  if (angle < PhaseConstants.NewMoonEnd) return 'sleepy';
  if (angle < PhaseConstants.WaxingCrescentEnd) return 'content';
  if (angle < PhaseConstants.FirstQuarterEnd) return 'happy';
  if (angle < PhaseConstants.WaxingGibbousEnd) return 'happy';
  if (angle < PhaseConstants.FullMoonEnd) return 'wow';
  if (angle < PhaseConstants.WaningGibbousEnd) return 'content';
  if (angle < PhaseConstants.LastQuarterEnd) return 'happy';
  if (angle < PhaseConstants.WaningCrescentEnd) return 'sleepy';
  return 'sleepy';
}

// Draws a cute cartoon face on a celestial body.
export function drawCuteFace(ctx, cx, cy, radius, expression) {
  const eyeY = cy - radius * 0.12;
  const eyeSpacing = radius * 0.25;
  const eyeRadius = radius * 0.07;

  const eyeColor = 'rgba(40,40,40,0.78)';
  const eyeHighlightColor = 'rgba(255,255,255,0.78)';
  const mouthWidth = radius * 0.04;
  const mouthColor = 'rgba(40,40,40,0.71)';
  const blushColor = 'rgba(255,130,130,0.2)';

  switch (expression) {
    case 'happy':
      drawHappyFace(ctx, cx, cy, radius, eyeY, eyeSpacing, eyeRadius,
        eyeColor, eyeHighlightColor, mouthWidth, mouthColor, blushColor);
      break;
    case 'sleepy':
      drawSleepyFace(ctx, cx, cy, radius, eyeY, eyeSpacing, eyeRadius,
        mouthWidth, mouthColor);
      break;
    case 'wow':
      drawWowFace(ctx, cx, cy, radius, eyeY, eyeSpacing, eyeRadius,
        eyeColor, eyeHighlightColor, mouthWidth, mouthColor, blushColor);
      break;
    case 'content':
      drawContentFace(ctx, cx, cy, radius, eyeY, eyeSpacing, eyeRadius,
        eyeColor, eyeHighlightColor, mouthWidth, mouthColor);
      break;
  }
}

function fillCircle(ctx, x, y, r, color) {
  ctx.fillStyle = color;
  ctx.beginPath();
  ctx.arc(x, y, r, 0, Math.PI * 2);
  ctx.fill();
}

function drawHappyFace(ctx, cx, cy, radius, eyeY, eyeSpacing, eyeRadius,
  eyeColor, eyeHighlightColor, mouthWidth, mouthColor, blushColor) {
  // Eyes
  fillCircle(ctx, cx - eyeSpacing, eyeY, eyeRadius, eyeColor);
  fillCircle(ctx, cx + eyeSpacing, eyeY, eyeRadius, eyeColor);
  // Eye highlights
  fillCircle(ctx, cx - eyeSpacing + eyeRadius * 0.3, eyeY - eyeRadius * 0.3,
    eyeRadius * 0.35, eyeHighlightColor);
  fillCircle(ctx, cx + eyeSpacing + eyeRadius * 0.3, eyeY - eyeRadius * 0.3,
    eyeRadius * 0.35, eyeHighlightColor);
  // Smile
  ctx.strokeStyle = mouthColor;
  ctx.lineWidth = mouthWidth;
  ctx.lineCap = 'round';
  ctx.beginPath();
  ctx.moveTo(cx - radius * 0.2, cy + radius * 0.1);
  ctx.quadraticCurveTo(cx, cy + radius * 0.3, cx + radius * 0.2, cy + radius * 0.1);
  ctx.stroke();
  // Blush cheeks
  fillCircle(ctx, cx - radius * 0.35, cy + radius * 0.08, radius * 0.1, blushColor);
  fillCircle(ctx, cx + radius * 0.35, cy + radius * 0.08, radius * 0.1, blushColor);
}

function drawSleepyFace(ctx, cx, cy, radius, eyeY, eyeSpacing, eyeRadius,
  mouthWidth, mouthColor) {
  ctx.strokeStyle = mouthColor;
  ctx.lineWidth = mouthWidth;
  ctx.lineCap = 'round';

  // Closed eyes (arcs)
  ctx.beginPath();
  ctx.moveTo(cx - eyeSpacing - eyeRadius, eyeY);
  ctx.quadraticCurveTo(cx - eyeSpacing, eyeY + eyeRadius * 0.8,
    cx - eyeSpacing + eyeRadius, eyeY);
  ctx.stroke();

  ctx.beginPath();
  ctx.moveTo(cx + eyeSpacing - eyeRadius, eyeY);
  ctx.quadraticCurveTo(cx + eyeSpacing, eyeY + eyeRadius * 0.8,
    cx + eyeSpacing + eyeRadius, eyeY);
  ctx.stroke();

  // Gentle smile
  ctx.beginPath();
  ctx.moveTo(cx - radius * 0.1, cy + radius * 0.15);
  ctx.quadraticCurveTo(cx, cy + radius * 0.22, cx + radius * 0.1, cy + radius * 0.15);
  ctx.stroke();

  // Zzz
  drawLabel(ctx, 'z z',
    cx + radius * 0.5, cy - radius * 0.3,
    radius * 0.2, [200, 200, 255, 150]);
}

function drawWowFace(ctx, cx, cy, radius, eyeY, eyeSpacing, eyeRadius,
  eyeColor, eyeHighlightColor, mouthWidth, mouthColor, blushColor) {
  // Big sparkly eyes
  fillCircle(ctx, cx - eyeSpacing, eyeY, eyeRadius * 1.3, eyeColor);
  fillCircle(ctx, cx + eyeSpacing, eyeY, eyeRadius * 1.3, eyeColor);
  // Star-like highlights
  fillCircle(ctx, cx - eyeSpacing + eyeRadius * 0.3, eyeY - eyeRadius * 0.4,
    eyeRadius * 0.45, eyeHighlightColor);
  fillCircle(ctx, cx + eyeSpacing + eyeRadius * 0.3, eyeY - eyeRadius * 0.4,
    eyeRadius * 0.45, eyeHighlightColor);
  fillCircle(ctx, cx - eyeSpacing - eyeRadius * 0.3, eyeY + eyeRadius * 0.2,
    eyeRadius * 0.2, eyeHighlightColor);
  fillCircle(ctx, cx + eyeSpacing - eyeRadius * 0.3, eyeY + eyeRadius * 0.2,
    eyeRadius * 0.2, eyeHighlightColor);
  // Open mouth (O shape)
  ctx.strokeStyle = mouthColor;
  ctx.lineWidth = mouthWidth;
  ctx.beginPath();
  ctx.ellipse(cx, cy + radius * 0.18, radius * 0.1, radius * 0.12, 0, 0, Math.PI * 2);
  ctx.stroke();
  // Extra blush
  const strongBlush = 'rgba(255,130,130,0.27)';
  fillCircle(ctx, cx - radius * 0.35, cy + radius * 0.1, radius * 0.12, strongBlush);
  fillCircle(ctx, cx + radius * 0.35, cy + radius * 0.1, radius * 0.12, strongBlush);
}

function drawContentFace(ctx, cx, cy, radius, eyeY, eyeSpacing, eyeRadius,
  eyeColor, eyeHighlightColor, mouthWidth, mouthColor) {
  // Relaxed eyes
  fillCircle(ctx, cx - eyeSpacing, eyeY, eyeRadius * 0.9, eyeColor);
  fillCircle(ctx, cx + eyeSpacing, eyeY, eyeRadius * 0.9, eyeColor);
  fillCircle(ctx, cx - eyeSpacing + eyeRadius * 0.3, eyeY - eyeRadius * 0.3,
    eyeRadius * 0.3, eyeHighlightColor);
  fillCircle(ctx, cx + eyeSpacing + eyeRadius * 0.3, eyeY - eyeRadius * 0.3,
    eyeRadius * 0.3, eyeHighlightColor);
  // Gentle smile
  ctx.strokeStyle = mouthColor;
  ctx.lineWidth = mouthWidth;
  ctx.lineCap = 'round';
  ctx.beginPath();
  ctx.moveTo(cx - radius * 0.15, cy + radius * 0.12);
  ctx.quadraticCurveTo(cx, cy + radius * 0.22, cx + radius * 0.15, cy + radius * 0.12);
  ctx.stroke();
}
