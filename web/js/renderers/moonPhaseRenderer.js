// Shared moon phase shadow algorithm — the single source of truth
// for the elliptical terminator technique.

export function drawPhaseOverlay(ctx, cx, cy, radius, moonAngleDeg, shadowColor) {
  let angle = ((moonAngleDeg % 360) + 360) % 360;
  const phaseRad = (angle * Math.PI) / 180.0;

  ctx.save();

  // Clip to the moon circle
  ctx.beginPath();
  ctx.arc(cx, cy, radius, 0, Math.PI * 2);
  ctx.clip();

  ctx.fillStyle = shadowColor;

  const terminatorScale = Math.cos(phaseRad);
  const termW = radius * Math.abs(terminatorScale);

  ctx.beginPath();

  if (angle <= 180) {
    // Waxing phases: right side illuminated, shadow on the left
    // Start at top
    ctx.moveTo(cx, cy - radius);

    // Left arc (left half of moon) — sweep from -90° by -180° (clockwise)
    ctx.arc(cx, cy, radius, -Math.PI / 2, Math.PI / 2, true);

    // Terminator arc (elliptical) from bottom to top
    if (terminatorScale >= 0) {
      // Terminator bulges right — sweep counterclockwise
      ctx.ellipse(cx, cy, termW, radius, 0, Math.PI / 2, -Math.PI / 2, true);
    } else {
      // Terminator bulges left — sweep clockwise
      ctx.ellipse(cx, cy, termW, radius, 0, Math.PI / 2, -Math.PI / 2, false);
    }
  } else {
    // Waning phases: left side illuminated, shadow on the right
    // Start at top
    ctx.moveTo(cx, cy - radius);

    // Right arc (right half of moon) — sweep from -90° by +180° (counterclockwise)
    ctx.arc(cx, cy, radius, -Math.PI / 2, Math.PI / 2, false);

    // Terminator arc (elliptical) from bottom to top
    if (terminatorScale >= 0) {
      // Sweep clockwise
      ctx.ellipse(cx, cy, termW, radius, 0, Math.PI / 2, -Math.PI / 2, false);
    } else {
      // Sweep counterclockwise
      ctx.ellipse(cx, cy, termW, radius, 0, Math.PI / 2, -Math.PI / 2, true);
    }
  }

  ctx.closePath();
  ctx.fill();

  ctx.restore();
}
