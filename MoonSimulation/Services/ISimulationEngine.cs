using MoonSimulation.Models;

namespace MoonSimulation.Services;

/// <summary>
/// Abstraction for the simulation loop that drives orbital state updates.
/// </summary>
public interface ISimulationEngine
{
    OrbitalState State { get; }
    bool IsRunning { get; }
    double Speed { get; set; }
    event Action? OnTick;
    void Start(IDispatcher dispatcher);
    void TogglePause();
}
