// Seeded PRNG — mulberry32
function mulberry32(seed) {
  return function () {
    seed |= 0;
    seed = (seed + 0x6d2b79f5) | 0;
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

// Draws a dark starry background with subtle twinkle animation.
export class StarfieldRenderer {
  constructor() {
    const rng = mulberry32(42);
    this.stars = [];
    this.starBrightness = [];
    for (let i = 0; i < 200; i++) {
      this.stars.push({ x: rng() * 2000, y: rng() * 2000 });
      this.starBrightness.push(0.3 + rng() * 0.7);
    }
    this.twinkleFrame = 0;
  }

  drawStarryBackground(ctx, x, y, w, h) {
    // Deep space gradient
    const bg = ctx.createLinearGradient(x + w / 2, y, x + w / 2, y + h);
    bg.addColorStop(0, 'rgb(8,8,40)');
    bg.addColorStop(1, 'rgb(2,2,18)');
    ctx.fillStyle = bg;
    ctx.fillRect(x, y, w, h);

    this.twinkleFrame++;

    for (let i = 0; i < this.stars.length; i++) {
      const sx = (this.stars[i].x % w) + x;
      const sy = (this.stars[i].y % h) + y;

      const twinkle =
        this.starBrightness[i] *
        (0.7 + 0.3 * Math.sin((this.twinkleFrame + i * 37) * 0.02));
      const alpha = twinkle;

      ctx.fillStyle = `rgba(255,255,240,${alpha})`;
      const size = this.starBrightness[i] > 0.8 ? 1.8 : 1.0;
      ctx.beginPath();
      ctx.arc(sx, sy, size, 0, Math.PI * 2);
      ctx.fill();
    }
  }
}
