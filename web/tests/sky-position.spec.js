// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Physics regression tests for the shared sky-dome projection used to place
 * the Sun and Moon in the "view from Earth" panel.
 *
 * Convention (see OrbitalState):
 *   - earthRotationDegrees: observer facing. 0° = noon (toward Sun),
 *     90° = sunset, 180° = midnight, 270° = sunrise.
 *   - An object's angle is measured from the Sun direction: Sun = 0°,
 *     Moon = its phase angle (0° New … 180° Full).
 *   - skyPosition returns { xFraction, elevation } where
 *     xFraction: -1 = east/left horizon, 0 = meridian, +1 = west/right horizon
 *     elevation: -1 = below horizon, +1 = overhead.
 */

/** Load the pure projection helper from the served ES module. */
async function loadSkyPosition(page) {
  return page.evaluate(async () => {
    const mod = await import('/js/renderers/skySceneRenderer.js');
    /** @type {(o:number,e:number)=>{xFraction:number,elevation:number}} */
    const fn = mod.skyPosition;
    // Sample the angles we care about and return plain numbers.
    const sample = (obj, earth) => {
      const p = fn(obj, earth);
      return { xFraction: p.xFraction, elevation: p.elevation };
    };
    return {
      // Sun (object angle 0) through a full day
      sunNoon: sample(0, 0),
      sunSunset: sample(0, 90),
      sunMidnight: sample(0, 180),
      sunSunrise: sample(0, 270),
      // Moon scenarios
      fullMoonMidnight: sample(180, 180),
      firstQuarterSunset: sample(90, 90),
      waxingCrescentSunset: sample(45, 90),
    };
  });
}

const EPS = 1e-9;

test.beforeEach(async ({ page }) => {
  await page.goto('/');
  await page.waitForFunction(() => !!window.__lunaEngine);
});

test.describe('Sky-dome projection (Sun & Moon placement)', () => {
  test('Sun rises in the east, transits overhead at noon, sets in the west', async ({ page }) => {
    const s = await loadSkyPosition(page);

    // Noon: highest (elevation ~ +1) and on the meridian (x ~ 0)
    expect(s.sunNoon.elevation).toBeCloseTo(1, 6);
    expect(Math.abs(s.sunNoon.xFraction)).toBeLessThan(1e-6);

    // Sunrise (270°): horizon (elevation ~ 0) on the EAST/left side (x < 0)
    expect(s.sunSunrise.elevation).toBeCloseTo(0, 6);
    expect(s.sunSunrise.xFraction).toBeLessThan(-0.5);

    // Sunset (90°): horizon (elevation ~ 0) on the WEST/right side (x > 0)
    expect(s.sunSunset.elevation).toBeCloseTo(0, 6);
    expect(s.sunSunset.xFraction).toBeGreaterThan(0.5);

    // Midnight: below horizon
    expect(s.sunMidnight.elevation).toBeCloseTo(-1, 6);
  });

  test('Sun does NOT rise and set at the same horizontal point (the cos bug)', async ({ page }) => {
    const s = await loadSkyPosition(page);
    // East horizon at sunrise must be opposite the west horizon at sunset.
    expect(s.sunSunrise.xFraction).toBeLessThan(0);
    expect(s.sunSunset.xFraction).toBeGreaterThan(0);
    expect(Math.sign(s.sunSunrise.xFraction)).not.toBe(Math.sign(s.sunSunset.xFraction));
  });

  test('Full Moon is overhead at midnight', async ({ page }) => {
    const s = await loadSkyPosition(page);
    expect(s.fullMoonMidnight.elevation).toBeCloseTo(1, 6);
    expect(Math.abs(s.fullMoonMidnight.xFraction)).toBeLessThan(1e-6);
  });

  test('First-quarter Moon transits overhead at sunset', async ({ page }) => {
    const s = await loadSkyPosition(page);
    expect(s.firstQuarterSunset.elevation).toBeCloseTo(1, 6);
    expect(Math.abs(s.firstQuarterSunset.xFraction)).toBeLessThan(1e-6);
  });

  test('Sun and Moon share one convention: at sunset the Sun is west of an overhead first-quarter Moon', async ({ page }) => {
    const s = await loadSkyPosition(page);
    // First-quarter Moon overhead/centre, Sun on the west (right) horizon — the
    // Moon's lit side (right, waxing) therefore points toward the Sun.
    expect(Math.abs(s.firstQuarterSunset.xFraction)).toBeLessThan(1e-6);
    expect(s.sunSunset.xFraction).toBeGreaterThan(0);
  });

  test('Waxing crescent sets after the Sun (higher in the west at sunset)', async ({ page }) => {
    const s = await loadSkyPosition(page);
    // At sunset a 45° waxing crescent is still well above the horizon...
    expect(s.waxingCrescentSunset.elevation).toBeGreaterThan(0.5);
    // ...and to the west (same side the Sun just set), so it follows the Sun down.
    expect(s.waxingCrescentSunset.xFraction).toBeGreaterThan(0);
    expect(s.waxingCrescentSunset.elevation).toBeGreaterThan(s.sunSunset.elevation + EPS);
  });
});
