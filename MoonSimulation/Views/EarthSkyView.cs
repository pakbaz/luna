using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using MoonSimulation.Helpers;
using MoonSimulation.Models;

namespace MoonSimulation.Views;

/// <summary>
/// Shows the view from Earth's surface — blue sky + sun during day,
/// dark starry sky + moon phase at night, with sunrise/sunset transitions.
/// </summary>
public class EarthSkyView : SKCanvasView
{
    private OrbitalState? _state;

    public void SetState(OrbitalState state) => _state = state;

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if (_state == null) return;

        var bounds = new SKRect(0, 0, info.Width, info.Height);

        SpaceRenderer.DrawEarthSkyView(canvas, bounds,
            _state.SunElevation,
            _state.MoonAngleDegrees,
            _state.EarthRotationDegrees,
            _state.Illumination,
            _state.Phase,
            _state.TimeOfDay,
            _state.HourOfDay);
    }
}
