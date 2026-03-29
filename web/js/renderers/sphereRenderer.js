// Generic 3D sphere rendering with lighting, labels, and color helpers.

// Lighten a color by amount (0-1). color is [r,g,b,a].
export function lighten(color, amount) {
  return [
    Math.min(255, color[0] + (255 - color[0]) * amount),
    Math.min(255, color[1] + (255 - color[1]) * amount),
    Math.min(255, color[2] + (255 - color[2]) * amount),
    color[3] !== undefined ? color[3] : 255,
  ];
}

// Darken a color by amount (0-1). color is [r,g,b,a].
export function darken(color, amount) {
  return [
    color[0] * (1 - amount),
    color[1] * (1 - amount),
    color[2] * (1 - amount),
    color[3] !== undefined ? color[3] : 255,
  ];
}

// Blend two colors. t=0 → a, t=1 → b. Colors are [r,g,b] or [r,g,b,a].
export function blendColor(a, b, t) {
  t = Math.max(0, Math.min(1, t));
  return [
    a[0] + (b[0] - a[0]) * t,
    a[1] + (b[1] - a[1]) * t,
    a[2] + (b[2] - a[2]) * t,
    (a[3] ?? 255) + ((b[3] ?? 255) - (a[3] ?? 255)) * t,
  ];
}

function rgba(c) {
  return `rgba(${Math.round(c[0])},${Math.round(c[1])},${Math.round(c[2])},${(c[3] !== undefined ? c[3] : 255) / 255})`;
}

// Draws a 3D-shaded sphere with a light direction.
export function drawSphere(ctx, cx, cy, radius, baseColor, lightDirX, lightDirY) {
  const highlightX = cx + lightDirX * radius * 0.35;
  const highlightY = cy + lightDirY * radius * 0.35;

  const lightColor = lighten(baseColor, 0.6);
  const darkColor = darken(baseColor, 0.7);
  const darkerColor = darken(baseColor, 0.9);

  const grad = ctx.createRadialGradient(
    highlightX, highlightY, 0,
    highlightX, highlightY, radius * 1.8
  );
  grad.addColorStop(0, rgba(lightColor));
  grad.addColorStop(0.35, rgba(baseColor));
  grad.addColorStop(0.75, rgba(darkColor));
  grad.addColorStop(1, rgba(darkerColor));

  ctx.fillStyle = grad;
  ctx.beginPath();
  ctx.arc(cx, cy, radius, 0, Math.PI * 2);
  ctx.fill();
}

// Draws a text label with shadow.
export function drawLabel(ctx, text, x, y, fontSize, color, align = 'center') {
  ctx.font = `${fontSize}px sans-serif`;
  ctx.textAlign = align;
  ctx.textBaseline = 'middle';

  // Shadow
  ctx.fillStyle = `rgba(0,0,0,0.59)`;
  ctx.fillText(text, x + 1, y + 1);

  // Text
  ctx.fillStyle = rgba(color);
  ctx.fillText(text, x, y);
}

// Draws a dashed orbit path (ellipse).
export function drawOrbitPath(ctx, cx, cy, radiusX, radiusY) {
  ctx.save();
  ctx.strokeStyle = 'rgba(100,100,150,0.31)';
  ctx.lineWidth = 1;
  ctx.setLineDash([6, 4]);
  ctx.beginPath();
  ctx.ellipse(cx, cy, radiusX, radiusY, 0, 0, Math.PI * 2);
  ctx.stroke();
  ctx.setLineDash([]);
  ctx.restore();
}
