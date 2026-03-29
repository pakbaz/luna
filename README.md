# Luna 🌙

An interactive moon phase simulation built with **.NET MAUI** and **SkiaSharp**. Designed to teach young children how the Moon orbits Earth, why it goes through phases, and what day and night look like — all in a fun, visual way.

## Screenshots

| Orbital View + Moon Phase + Sky View |
|:---:|
| Sun, Earth, and Moon orbit with day/night cycle, moon phase close-up, and what the sky looks like from Earth |

## Features

- **Orbital View** — Sun, Earth, and Moon with 3D-shaded spheres, correct lighting, and a tilted orbital path
- **Moon Phase Close-up** — Accurate illumination using the elliptical terminator technique (crescent, quarter, gibbous, full)
- **Earth Sky View** — What the observer sees from Earth: blue sky by day, sunset transitions, starry night sky with the Moon in its correct phase and position
- **Day/Night Cycle** — Earth rotates showing a clear day/night terminator, city lights on the dark side, and a small observer dot
- **Kid-Friendly** — Cute cartoon faces on the Sun and Moon, fun phase names ("WOW Full Moon!", "Moon is Hiding!"), fluffy clouds, shooting stars, and a cozy little house
- **Speed Control** — Adjustable from 🐢 0.1x (slow, watch each day/night) to 🐇 10x (fast, watch a full lunar month in seconds)
- **Astronomically Accurate** — Correct Sun–Earth–Moon geometry for all 8 named phases

## Platforms

Built with [.NET MAUI](https://dotnet.microsoft.com/apps/maui), Luna runs on:

| Platform | Framework Target | Status |
|----------|-----------------|--------|
| **macOS** | `net10.0-maccatalyst` | ✅ Tested |
| **iOS** | `net10.0-ios` | ✅ Supported |
| **Android** | `net10.0-android` | ✅ Supported |
| **Windows** | `net10.0-windows10.0.19041.0` | ✅ Supported |

> **Linux**: .NET MAUI does not natively target Linux. However, community projects like [Uno Platform](https://platform.uno/) can enable Linux support for MAUI-style apps.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- MAUI workload: `dotnet workload install maui`
- **macOS/iOS**: Xcode (for Mac Catalyst and iOS builds)
- **Android**: Android SDK (included with Visual Studio or `dotnet workload install android`)
- **Windows**: Visual Studio 2022+ with MAUI workload

## Getting Started

```bash
# Clone the repo
git clone https://github.com/pakbaz/luna.git
cd luna/MoonSimulation

# Install the MAUI workload (if not already installed)
dotnet workload install maui

# Run on macOS
dotnet build -f net10.0-maccatalyst -t:Run

# Run on Windows
dotnet build -f net10.0-windows10.0.19041.0 -t:Run

# Run on Android (emulator or connected device)
dotnet build -f net10.0-android -t:Run

# Run on iOS Simulator
dotnet build -f net10.0-ios -t:Run
```

## Project Structure

```
MoonSimulation/
├── Models/
│   └── OrbitalState.cs           # Moon angle, Earth rotation, phase calculation
├── Services/
│   ├── ISimulationEngine.cs      # Interface for the simulation loop
│   └── SimulationEngine.cs       # 60fps timer, speed control, orbital mechanics
├── Renderers/                    # SkiaSharp rendering (SOLID — one class per concern)
│   ├── StarfieldRenderer.cs      # Twinkling starry background
│   ├── SphereRenderer.cs         # Generic 3D sphere shading, labels, colors
│   ├── SunRenderer.cs            # Sun with corona glow and happy face
│   ├── EarthRenderer.cs          # Earth with day/night terminator, city lights
│   ├── MoonRenderer.cs           # Moon with craters (orbital view)
│   ├── MoonPhaseRenderer.cs      # Shared moon phase shadow algorithm
│   ├── FaceRenderer.cs           # Cute cartoon face expressions
│   ├── SkySceneRenderer.cs       # Earth sky view orchestration
│   └── SceneryRenderer.cs        # House, clouds, trees, shooting stars
├── Views/
│   ├── SimulationCanvasView.cs   # Abstract base class for all views
│   ├── OrbitalView.cs            # Top panel: Sun–Earth–Moon orbit
│   ├── MoonPhaseView.cs          # Bottom-left: Moon close-up with phase
│   └── EarthSkyView.cs           # Bottom-right: Sky from Earth's surface
├── MainPage.xaml / .cs           # Layout and controls
└── Platforms/                    # iOS, Android, Mac Catalyst, Windows
```

## Architecture

The codebase follows **SOLID principles**:

- **Single Responsibility** — Each renderer handles one visual concern
- **Open/Closed** — New celestial bodies = new renderer files, no edits to existing ones
- **Liskov Substitution** — All views inherit from `SimulationCanvasView`
- **Interface Segregation** — `ISimulationEngine` defines a clean simulation contract
- **Dependency Inversion** — Views and MainPage depend on abstractions

## How It Works

The simulation runs a 60fps timer that advances two angles:

1. **Moon orbital angle** (0°–360°) — One full orbit every 30 seconds at 1x speed
2. **Earth rotation angle** (0°–360°) — ~29.5 rotations per lunar orbit (one day ≈ 1 second at 1x)

The Moon's phase is derived from its orbital angle:
- **0°** = New Moon (Moon between Sun and Earth)
- **90°** = First Quarter
- **180°** = Full Moon (Earth between Sun and Moon)
- **270°** = Last Quarter

The illumination uses: `(1 - cos(angle)) / 2`, and the phase shadow is rendered with an elliptical terminator technique.

## License

MIT

## Acknowledgments

Built with ❤️ for a curious 3-year-old who wanted to know why the Moon changes shape.
