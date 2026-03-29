// Draws ground scenery: clouds, house, and trees.

export function drawClouds(ctx, boundsX, boundsY, boundsW, boundsH, groundY, sunElevation, frame) {
  if (sunElevation < -0.1) return;

  const alpha = Math.max(0, Math.min(1, sunElevation + 0.1)) * (120 / 255);
  ctx.fillStyle = `rgba(255,255,255,${alpha})`;

  const w = boundsW;
  const skyH = groundY - boundsY;

  const drift = (frame * 0.3) % (w + 200) - 100;

  drawCloud(ctx, boundsX + drift, boundsY + skyH * 0.2, w * 0.12);
  drawCloud(ctx, boundsX + ((drift * 0.6 + w * 0.5) % (w + 200)) - 100,
    boundsY + skyH * 0.35, w * 0.09);
  drawCloud(ctx, boundsX + ((drift * 0.4 + w * 0.3) % (w + 200)) - 100,
    boundsY + skyH * 0.5, w * 0.07);
}

function drawCloud(ctx, x, y, size) {
  ctx.beginPath();
  ctx.arc(x, y, size * 0.5, 0, Math.PI * 2);
  ctx.fill();
  ctx.beginPath();
  ctx.arc(x - size * 0.35, y + size * 0.1, size * 0.35, 0, Math.PI * 2);
  ctx.fill();
  ctx.beginPath();
  ctx.arc(x + size * 0.35, y + size * 0.1, size * 0.35, 0, Math.PI * 2);
  ctx.fill();
  ctx.beginPath();
  ctx.arc(x + size * 0.15, y - size * 0.15, size * 0.4, 0, Math.PI * 2);
  ctx.fill();
  ctx.beginPath();
  ctx.arc(x - size * 0.15, y + size * 0.2, size * 0.3, 0, Math.PI * 2);
  ctx.fill();
}

export function drawLittleHouse(ctx, boundsX, boundsY, boundsW, boundsH, groundY, sunElevation) {
  const w = boundsW;
  const houseX = boundsX + w * 0.7;
  const houseW = w * 0.1;
  const houseH = houseW * 0.8;
  const houseY = groundY - houseH;
  const isNight = sunElevation < 0;

  // House body
  ctx.fillStyle = isNight ? 'rgb(80,50,40)' : 'rgb(180,120,80)';
  ctx.fillRect(houseX, houseY, houseW, houseH);

  // Roof (triangle)
  ctx.fillStyle = isNight ? 'rgb(100,40,40)' : 'rgb(180,60,60)';
  ctx.beginPath();
  ctx.moveTo(houseX - houseW * 0.15, houseY);
  ctx.lineTo(houseX + houseW * 0.5, houseY - houseH * 0.6);
  ctx.lineTo(houseX + houseW * 1.15, houseY);
  ctx.closePath();
  ctx.fill();

  // Window
  const winX = houseX + houseW * 0.3;
  const winY = houseY + houseH * 0.25;
  const winSize = houseW * 0.35;

  if (isNight) {
    // Warm window glow
    ctx.save();
    ctx.shadowBlur = winSize;
    ctx.shadowColor = 'rgba(255,200,80,0.24)';
    ctx.fillStyle = 'rgba(255,200,80,0.24)';
    ctx.beginPath();
    ctx.arc(winX + winSize / 2, winY + winSize / 2, winSize * 1.5, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }

  ctx.fillStyle = isNight ? 'rgb(255,220,120)' : 'rgb(150,200,255)';
  ctx.fillRect(winX, winY, winSize, winSize);

  // Window cross
  ctx.strokeStyle = isNight ? 'rgb(80,50,40)' : 'rgb(140,100,60)';
  ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(winX, winY + winSize / 2);
  ctx.lineTo(winX + winSize, winY + winSize / 2);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(winX + winSize / 2, winY);
  ctx.lineTo(winX + winSize / 2, winY + winSize);
  ctx.stroke();

  // Tree next to house
  const treeX = houseX - houseW * 0.6;
  const treeTopY = groundY - houseH * 1.2;

  // Trunk
  ctx.fillStyle = isNight ? 'rgb(60,35,20)' : 'rgb(120,80,40)';
  ctx.fillRect(treeX - 2, groundY - houseH * 0.5, 5, houseH * 0.5);

  // Foliage (circle cluster)
  ctx.fillStyle = isNight ? 'rgb(20,60,20)' : 'rgb(50,150,50)';
  const fRadius = houseW * 0.25;

  ctx.beginPath();
  ctx.arc(treeX, treeTopY + fRadius, fRadius, 0, Math.PI * 2);
  ctx.fill();
  ctx.beginPath();
  ctx.arc(treeX - fRadius * 0.6, treeTopY + fRadius * 1.5, fRadius * 0.8, 0, Math.PI * 2);
  ctx.fill();
  ctx.beginPath();
  ctx.arc(treeX + fRadius * 0.6, treeTopY + fRadius * 1.5, fRadius * 0.8, 0, Math.PI * 2);
  ctx.fill();
}
