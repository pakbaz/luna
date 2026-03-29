import { blendColor } from './sphereRenderer.js';
import { drawLabel } from './sphereRenderer.js';
import { drawPhaseOverlay } from './moonPhaseRenderer.js';
import { drawClouds, drawLittleHouse } from './sceneryRenderer.js';

// Orchestrates the "view from Earth" sky scene.
export class SkySceneRenderer {
  constructor(starfield) {
    this._starfield = starfield;
  }

  drawEarthSkyView(ctx, x, y, w, h,
    sunElevation, moonAngleDeg, earthRotationDeg, illumination,
    phase, timeOfDay, hourOfDay) {

    const groundY = y + h * 0.75;

    // Sky gradient
    this._drawSkyGradient(ctx, x, y, w, h, groundY, sunElevation);

    // Ground
    this._drawGround(ctx, x, y, w, h, groundY, sunElevation);

    // Sun in the sky
    if (sunElevation > -0.1) {
      const sunSkyX = x + w * 0.5 + w * 0.35 * Math.cos(earthRotationDeg * Math.PI / 180.0);
      const sunSkyY = groundY - (groundY - y) * 0.8 * Math.max(0, sunElevation);

      if (sunElevation > 0) {
        const sunR = Math.min(w, h) * 0.06;

        // Sun glow
        ctx.save();
        ctx.shadowBlur = sunR * 2;
        ctx.shadowColor = `rgba(255,230,100,${0.16 * sunElevation})`;
        ctx.fillStyle = `rgba(255,230,100,${0.16 * sunElevation})`;
        ctx.beginPath();
        ctx.arc(sunSkyX, sunSkyY, sunR * 3, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();

        // Sun disk
        const sunGrad = ctx.createRadialGradient(sunSkyX, sunSkyY, 0, sunSkyX, sunSkyY, sunR);
        sunGrad.addColorStop(0, 'rgb(255,255,230)');
        sunGrad.addColorStop(1, 'rgb(255,200,50)');
        ctx.fillStyle = sunGrad;
        ctx.beginPath();
        ctx.arc(sunSkyX, sunSkyY, sunR, 0, Math.PI * 2);
        ctx.fill();
      }
    }

    // Moon in the sky
    const moonObserverAngle = (moonAngleDeg - earthRotationDeg) * Math.PI / 180.0;
    const moonElevation = Math.cos(moonObserverAngle);

    const minIlluminationToSee = sunElevation > 0.2 ? 0.25 : 0.03;
    const moonVisible = moonElevation > 0.05 && illumination > minIlluminationToSee;

    if (moonVisible) {
      const moonSkyX = x + w * 0.5 - w * 0.3 * Math.sin(moonObserverAngle);
      const moonSkyY = groundY - (groundY - y) * 0.7 * moonElevation;
      const moonR = Math.min(w, h) * 0.055;

      const moonAlpha = Math.min(1.0, moonElevation * 2) * Math.min(1.0, illumination * 3);

      this._drawMiniMoonPhase(ctx, moonSkyX, moonSkyY, moonR,
        moonAngleDeg, sunElevation, moonAlpha);
    }

    // Stars (only visible at night)
    if (sunElevation < 0.2) {
      const starAlpha = Math.max(0, Math.min(1, (0.2 - sunElevation) / 0.4));
      this._drawSkyStars(ctx, x, y, w, groundY, starAlpha);

      if (sunElevation < -0.2) {
        this._drawShootingStar(ctx, x, y, w, h, groundY, this._starfield.twinkleFrame);
      }
    }

    // Clouds
    drawClouds(ctx, x, y, w, h, groundY, sunElevation, this._starfield.twinkleFrame);

    // Little house
    drawLittleHouse(ctx, x, y, w, h, groundY, sunElevation);

    // Time label
    const hours = Math.floor(hourOfDay);
    const mins = Math.floor((hourOfDay - hours) * 60);
    const timeStr = `${String(hours).padStart(2, '0')}:${String(mins).padStart(2, '0')}`;
    drawLabel(ctx, timeStr, x + w * 0.5, y + 22, 16, [230, 230, 230]);

    // Day/Night label
    drawLabel(ctx, timeOfDay.name, x + w * 0.5, y + 44, 14, [200, 200, 220]);
  }

  _drawMiniMoonPhase(ctx, cx, cy, radius, moonAngleDeg, sunElevation, alpha) {
    // Moon glow
    const glowAlpha = (sunElevation < 0 ? 0.4 : 0.15) * alpha;
    ctx.save();
    ctx.shadowBlur = radius * 0.8;
    ctx.shadowColor = `rgba(200,200,220,${glowAlpha})`;
    ctx.fillStyle = `rgba(200,200,220,${glowAlpha})`;
    ctx.beginPath();
    ctx.arc(cx, cy, radius * 1.5, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // Moon disk
    ctx.fillStyle = `rgba(220,220,215,${alpha})`;
    ctx.beginPath();
    ctx.arc(cx, cy, radius, 0, Math.PI * 2);
    ctx.fill();

    // Phase shadow
    const shadowAlpha = alpha * (sunElevation < 0 ? 230 : 200) / 255;
    const shadowColor = sunElevation < 0
      ? `rgba(5,5,25,${shadowAlpha})`
      : `rgba(100,140,200,${shadowAlpha})`;
    drawPhaseOverlay(ctx, cx, cy, radius, moonAngleDeg, shadowColor);
  }

  _drawSkyGradient(ctx, bx, by, bw, bh, groundY, sunElevation) {
    let topColor, bottomColor;

    if (sunElevation > 0.3) {
      topColor = [40, 100, 220];
      bottomColor = [135, 190, 250];
    } else if (sunElevation > 0.0) {
      const t = sunElevation / 0.3;
      topColor = blendColor([30, 30, 80], [40, 100, 220], t);
      bottomColor = blendColor([220, 100, 40], [135, 190, 250], t);
    } else if (sunElevation > -0.3) {
      const t = (sunElevation + 0.3) / 0.3;
      topColor = blendColor([8, 8, 35], [30, 30, 80], t);
      bottomColor = blendColor([20, 15, 40], [220, 100, 40], t);
    } else {
      topColor = [5, 5, 25];
      bottomColor = [10, 10, 35];
    }

    const grad = ctx.createLinearGradient(bx + bw / 2, by, bx + bw / 2, groundY);
    grad.addColorStop(0, `rgb(${Math.round(topColor[0])},${Math.round(topColor[1])},${Math.round(topColor[2])})`);
    grad.addColorStop(1, `rgb(${Math.round(bottomColor[0])},${Math.round(bottomColor[1])},${Math.round(bottomColor[2])})`);
    ctx.fillStyle = grad;
    ctx.fillRect(bx, by, bw, groundY - by);
  }

  _drawGround(ctx, bx, by, bw, bh, groundY, sunElevation) {
    let groundTop, groundBottom;

    if (sunElevation > 0.1) {
      groundTop = [40, 120, 40];
      groundBottom = [30, 80, 30];
    } else if (sunElevation > -0.1) {
      const t = (sunElevation + 0.1) / 0.2;
      groundTop = blendColor([15, 40, 15], [40, 120, 40], t);
      groundBottom = blendColor([10, 25, 10], [30, 80, 30], t);
    } else {
      groundTop = [15, 40, 15];
      groundBottom = [10, 25, 10];
    }

    const grad = ctx.createLinearGradient(bx + bw / 2, groundY, bx + bw / 2, by + bh);
    grad.addColorStop(0, `rgb(${Math.round(groundTop[0])},${Math.round(groundTop[1])},${Math.round(groundTop[2])})`);
    grad.addColorStop(1, `rgb(${Math.round(groundBottom[0])},${Math.round(groundBottom[1])},${Math.round(groundBottom[2])})`);
    ctx.fillStyle = grad;
    ctx.fillRect(bx, groundY, bw, by + bh - groundY);

    // Horizon line glow during sunrise/sunset
    if (sunElevation > -0.1 && sunElevation < 0.2) {
      ctx.save();
      ctx.shadowBlur = 8;
      const a = (60 * (1 - Math.abs(sunElevation) * 5)) / 255;
      ctx.shadowColor = `rgba(255,180,80,${a})`;
      ctx.strokeStyle = `rgba(255,180,80,${a})`;
      ctx.lineWidth = 1;
      ctx.beginPath();
      ctx.moveTo(bx, groundY);
      ctx.lineTo(bx + bw, groundY);
      ctx.stroke();
      ctx.restore();
    }
  }

  _drawSkyStars(ctx, bx, by, bw, groundY, alpha) {
    const stars = this._starfield.stars;
    if (!stars) return;

    for (let i = 0; i < 60; i++) {
      const sx = (stars[i].x % bw) + bx;
      const sy = (stars[i].y % (groundY - by)) + by;

      const twinkle = this._starfield.starBrightness[i] *
        (0.7 + 0.3 * Math.sin((this._starfield.twinkleFrame + i * 37) * 0.02));
      const a = twinkle * alpha;

      ctx.fillStyle = `rgba(255,255,240,${a})`;
      const size = this._starfield.starBrightness[i] > 0.8 ? 1.5 : 0.8;
      ctx.beginPath();
      ctx.arc(sx, sy, size, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  _drawShootingStar(ctx, bx, by, bw, bh, groundY, frame) {
    const cycle = frame % 300;
    if (cycle > 30) return;

    const progress = cycle / 30;
    const startX = bx + bw * 0.7;
    const startY = by + 20;
    const endX = bx + bw * 0.3;
    const endY = groundY * 0.4;

    const curX = startX + (endX - startX) * progress;
    const curY = startY + (endY - startY) * progress;

    const tailLen = 30 * (1 - progress);
    const tailX = curX - ((endX - startX) / (endY - startY)) * -tailLen;
    const tailY = curY - tailLen;

    // Tail
    const tailGrad = ctx.createLinearGradient(tailX, tailY, curX, curY);
    tailGrad.addColorStop(0, 'rgba(255,255,220,0)');
    tailGrad.addColorStop(1, `rgba(255,255,220,${(200 * (1 - progress)) / 255})`);
    ctx.strokeStyle = tailGrad;
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.beginPath();
    ctx.moveTo(tailX, tailY);
    ctx.lineTo(curX, curY);
    ctx.stroke();

    // Head glow
    ctx.save();
    ctx.shadowBlur = 3;
    ctx.shadowColor = `rgba(255,255,230,${(255 * (1 - progress)) / 255})`;
    ctx.fillStyle = `rgba(255,255,230,${(255 * (1 - progress)) / 255})`;
    ctx.beginPath();
    ctx.arc(curX, curY, 2, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }
}
