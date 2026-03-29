import { SimulationEngine } from './services/simulationEngine.js';
import { StarfieldRenderer } from './renderers/starfieldRenderer.js';
import { OrbitalView } from './views/orbitalView.js';
import { MoonPhaseView } from './views/moonPhaseView.js';
import { EarthSkyView } from './views/earthSkyView.js';

// Shared starfield for sky scene
const sharedStarfield = new StarfieldRenderer();

const engine = new SimulationEngine();

let orbitalView, moonPhaseView, earthSkyView;

function resizeCanvas(canvas) {
  const rect = canvas.parentElement.getBoundingClientRect();
  const dpr = window.devicePixelRatio || 1;
  canvas.width = rect.width * dpr;
  canvas.height = rect.height * dpr;
  canvas.style.width = rect.width + 'px';
  canvas.style.height = rect.height + 'px';
  canvas.getContext('2d').setTransform(dpr, 0, 0, dpr, 0, 0);
  // Store logical dimensions for convenience
  canvas._logicalW = rect.width;
  canvas._logicalH = rect.height;
}

function resizeAll() {
  const canvases = document.querySelectorAll('canvas');
  canvases.forEach(resizeCanvas);
  // Redraw
  drawAll();
}

function drawAll() {
  orbitalView.draw(engine.state);
  moonPhaseView.draw(engine.state);
  earthSkyView.draw(engine.state);
}

function updateLabels() {
  const state = engine.state;
  const { name, emoji } = state.phase;
  const { name: todName, emoji: todEmoji } = state.timeOfDay;
  const hours = Math.floor(state.hourOfDay);
  const mins = Math.floor((state.hourOfDay - hours) * 60);
  const timeStr = `${String(hours).padStart(2, '0')}:${String(mins).padStart(2, '0')}`;

  document.getElementById('phaseLabel').textContent = `${emoji}  ${name}`;
  document.getElementById('timeLabel').textContent = `${todEmoji} ${todName}  ${timeStr}`;
}

document.addEventListener('DOMContentLoaded', () => {
  const orbitalCanvas = document.getElementById('orbitalCanvas');
  const phaseCanvas = document.getElementById('phaseCanvas');
  const skyCanvas = document.getElementById('skyCanvas');

  orbitalView = new OrbitalView(orbitalCanvas);
  moonPhaseView = new MoonPhaseView(phaseCanvas);
  earthSkyView = new EarthSkyView(skyCanvas, sharedStarfield);

  resizeAll();

  const playPauseBtn = document.getElementById('playPauseBtn');
  const speedSlider = document.getElementById('speedSlider');
  const speedLabel = document.getElementById('speedLabel');

  playPauseBtn.addEventListener('click', () => {
    engine.togglePause();
    playPauseBtn.textContent = engine.isRunning ? '⏸️' : '▶️';
  });

  speedSlider.addEventListener('input', () => {
    engine.speed = parseFloat(speedSlider.value);
    speedLabel.textContent = `${engine.speed.toFixed(1)}x`;
  });

  engine.onTick = () => {
    drawAll();
    updateLabels();
  };

  window.addEventListener('resize', resizeAll);

  engine.start();
});
