using SkiaSharp;
using SkiaSharp.Views.Maui;
using MoonSimulation.Renderers;
using MoonSimulation.Models;

namespace MoonSimulation.Views;

/// <summary>
/// Shows the view from Earth's surface — blue sky + sun during day,
/// dark starry sky + moon phase at night, with sunrise/sunset transitions.
/// </summary>
public class EarthSkyView : SimulationCanvasView
{
    private readonly StarfieldRenderer _starfield = new();
    private readonly SkySceneRenderer _skyScene;

    public EarthSkyView()
    {
        _skyScene = new SkySceneRenderer(_starfield);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        // Ensure starfield state is initialized (tick the starfield for frame counter)
        var bounds = new SKRect(0, 0, info.Width, info.Height);
        _starfield.DrawStarryBackground(canvas, bounds);

        if (State == null) return;

        // Clear and redraw — the sky scene draws its own background
        canvas.Clear();

        _skyScene.DrawEarthSkyView(canvas, bounds,
            State.SunElevation,
            State.MoonAngleDegrees,
            State.EarthRotationDegrees,
            State.Illumination,
            State.Phase,
            State.TimeOfDay,
            State.HourOfDay);
    }
}
