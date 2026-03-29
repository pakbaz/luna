import { drawSphere, drawLabel } from './sphereRenderer.js';

// Renders Earth with day/night terminator, city lights, and observer dot.

export function drawEarth(ctx, cx, cy, radius, lightDirX, lightDirY, earthRotationDeg = 0) {
  // Base blue sphere (day side)
  drawSphere(ctx, cx, cy, radius, [40, 80, 200], lightDirX, lightDirY);

  // Green continent hints
  ctx.fillStyle = 'rgba(50,160,60,0.39)';
  ctx.beginPath();
  ctx.ellipse(
    cx - radius * 0.1, cy - radius * 0.25,
    radius * 0.2, radius * 0.15, 0, 0, Math.PI * 2
  );
  ctx.fill();

  ctx.beginPath();
  ctx.ellipse(
    cx + radius * 0.05, cy + radius * 0.2,
    radius * 0.2, radius * 0.15, 0, 0, Math.PI * 2
  );
  ctx.fill();

  // Day/Night terminator shadow
  drawDayNightTerminator(ctx, cx, cy, radius, lightDirX, lightDirY);

  // City lights on the dark side
  drawCityLights(ctx, cx, cy, radius, lightDirX, lightDirY);

  // Atmosphere glow
  const atmoGrad = ctx.createRadialGradient(cx, cy, 0, cx, cy, radius * 1.15);
  atmoGrad.addColorStop(0.75, 'rgba(100,180,255,0)');
  atmoGrad.addColorStop(0.9, 'rgba(100,180,255,0.12)');
  atmoGrad.addColorStop(1, 'rgba(100,180,255,0)');
  ctx.fillStyle = atmoGrad;
  ctx.beginPath();
  ctx.arc(cx, cy, radius * 1.15, 0, Math.PI * 2);
  ctx.fill();

  // Observer dot on the equator
  drawObserver(ctx, cx, cy, radius, earthRotationDeg, lightDirX, lightDirY);
}

function drawDayNightTerminator(ctx, cx, cy, radius, lightDirX, lightDirY) {
  ctx.save();

  // Clip to Earth circle
  ctx.beginPath();
  ctx.arc(cx, cy, radius, 0, Math.PI * 2);
  ctx.clip();

  const angle = Math.atan2(-lightDirY, -lightDirX);
  const perpAngle1 = angle + Math.PI / 2;

  // Shadow path — dark half facing away from light
  ctx.fillStyle = 'rgba(5,5,30,0.71)';
  ctx.beginPath();
  ctx.moveTo(
    cx + Math.cos(perpAngle1) * radius,
    cy + Math.sin(perpAngle1) * radius
  );
  ctx.arc(
    cx, cy, radius,
    perpAngle1,
    perpAngle1 - Math.PI,
    true
  );
  ctx.lineTo(
    cx + Math.cos(perpAngle1) * radius,
    cy + Math.sin(perpAngle1) * radius
  );
  ctx.closePath();
  ctx.fill();

  // Soft terminator glow (twilight zone)
  const perpAngle2 = angle - Math.PI / 2;
  ctx.strokeStyle = 'rgba(255,150,80,0.12)';
  ctx.lineWidth = radius * 0.15;
  ctx.beginPath();
  ctx.moveTo(
    cx + Math.cos(perpAngle1) * radius,
    cy + Math.sin(perpAngle1) * radius
  );
  ctx.lineTo(
    cx + Math.cos(perpAngle2) * radius,
    cy + Math.sin(perpAngle2) * radius
  );
  ctx.stroke();

  ctx.restore();
}

function drawCityLights(ctx, cx, cy, radius, lightDirX, lightDirY) {
  ctx.save();
  ctx.beginPath();
  ctx.arc(cx, cy, radius * 0.95, 0, Math.PI * 2);
  ctx.clip();

  const cities = [
    [-0.15, -0.1], [0.2, -0.2], [0.1, 0.15],
    [-0.25, 0.2], [0.3, -0.05], [-0.1, -0.3],
    [0.0, 0.1], [-0.3, -0.05], [0.15, 0.25],
  ];

  for (const [dx, dy] of cities) {
    const ccx = cx + dx * radius;
    const ccy = cy + dy * radius;
    const toCityX = dx;
    const toCityY = dy;
    const dot = toCityX * lightDirX + toCityY * lightDirY;

    if (dot < -0.1) {
      const brightness = Math.min(1, Math.abs(dot));
      ctx.fillStyle = `rgba(255,220,100,${brightness * 120 / 255})`;
      ctx.beginPath();
      ctx.arc(ccx, ccy, 1.2, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  ctx.restore();
}

function drawObserver(ctx, cx, cy, radius, earthRotationDeg, lightDirX, lightDirY) {
  const rotRad = (earthRotationDeg * Math.PI) / 180.0;

  const obsX = cx - radius * 0.9 * Math.cos(rotRad);
  const obsY = cy + radius * 0.3 * Math.sin(rotRad);

  const sinComponent = Math.sin(rotRad);
  if (sinComponent > -0.3) {
    const relX = (obsX - cx) / radius;
    const dot = relX * lightDirX;
    const isDay = dot > 0;

    ctx.fillStyle = isDay
      ? 'rgba(255,255,0,0.86)'
      : 'rgba(255,150,50,0.86)';
    ctx.beginPath();
    ctx.arc(obsX, obsY, 3.5, 0, Math.PI * 2);
    ctx.fill();

    drawLabel(
      ctx,
      isDay ? 'You' : 'zzZ',
      obsX, obsY - 10,
      10,
      isDay ? [255, 255, 200] : [200, 200, 255]
    );
  }
}
