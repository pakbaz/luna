using SkiaSharp.Views.Maui.Controls;
using MoonSimulation.Models;

namespace MoonSimulation.Views;

/// <summary>
/// Base class for simulation canvas views that receive orbital state updates.
/// </summary>
public abstract class SimulationCanvasView : SKCanvasView
{
    protected OrbitalState? State { get; private set; }

    public void UpdateState(OrbitalState state) => State = state;
}
