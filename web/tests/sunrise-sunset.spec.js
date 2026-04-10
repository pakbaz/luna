// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Pause the simulation, set earthRotationDegrees to the given value,
 * force a label update, and return the text content of #timeLabel.
 */
async function timeLabelAtRotation(page, degrees) {
  await page.evaluate((deg) => {
    const engine = window.__lunaEngine;
    if (engine.isRunning) engine.togglePause();
    engine.state.earthRotationDegrees = deg;
    if (engine.onTick) engine.onTick();
  }, degrees);
  return page.locator('#timeLabel').textContent();
}

test.beforeEach(async ({ page }) => {
  await page.goto('/');
  // Wait until the simulation has started and labels are rendered
  await page.waitForFunction(() => {
    const el = document.getElementById('timeLabel');
    return el && el.textContent && el.textContent.trim().length > 0;
  });
  // Pause the simulation so rotation doesn't drift during assertions
  await page.evaluate(() => {
    const engine = window.__lunaEngine;
    if (engine.isRunning) engine.togglePause();
  });
});

test.describe('Sunrise / Sunset label direction', () => {

  test('noon (0°) → Day', async ({ page }) => {
    const label = await timeLabelAtRotation(page, 0);
    expect(label).toContain('Day');
  });

  test('6 PM (90°) → Sunset', async ({ page }) => {
    const label = await timeLabelAtRotation(page, 90);
    expect(label).toContain('Sunset');
  });

  test('midnight (180°) → Night', async ({ page }) => {
    const label = await timeLabelAtRotation(page, 180);
    expect(label).toContain('Night');
  });

  test('6 AM (270°) → Sunrise', async ({ page }) => {
    const label = await timeLabelAtRotation(page, 270);
    expect(label).toContain('Sunrise');
  });

  test('full cycle order: Day → Sunset → Night → Sunrise → Day', async ({ page }) => {
    // Sample many rotation angles and verify the label sequence
    const steps = [];
    for (let deg = 0; deg < 360; deg += 5) {
      const label = await timeLabelAtRotation(page, deg);
      steps.push({ deg, label });
    }

    // Build the sequence of unique labels in order
    const sequence = [];
    for (const s of steps) {
      const name = s.label.includes('Day') ? 'Day'
        : s.label.includes('Sunset') ? 'Sunset'
        : s.label.includes('Night') ? 'Night'
        : s.label.includes('Sunrise') ? 'Sunrise'
        : 'Unknown';
      if (sequence.length === 0 || sequence[sequence.length - 1] !== name) {
        sequence.push(name);
      }
    }

    // Starting at noon (0°) the expected order is Day → Sunset → Night → Sunrise → Day
    expect(sequence).toEqual(['Day', 'Sunset', 'Night', 'Sunrise', 'Day']);
  });

  test('Sunset should NOT appear when sun is rising (270°)', async ({ page }) => {
    const label = await timeLabelAtRotation(page, 270);
    expect(label).not.toContain('Sunset');
  });

  test('Sunrise should NOT appear when sun is setting (90°)', async ({ page }) => {
    const label = await timeLabelAtRotation(page, 90);
    expect(label).not.toContain('Sunrise');
  });

  test('time display shows correct hours for known angles', async ({ page }) => {
    // 0° = noon → 12:00
    let label = await timeLabelAtRotation(page, 0);
    expect(label).toContain('12:00');

    // 180° = midnight → 00:00
    label = await timeLabelAtRotation(page, 180);
    expect(label).toContain('00:00');

    // 90° → 18:00
    label = await timeLabelAtRotation(page, 90);
    expect(label).toContain('18:00');

    // 270° → 06:00
    label = await timeLabelAtRotation(page, 270);
    expect(label).toContain('06:00');
  });
});
