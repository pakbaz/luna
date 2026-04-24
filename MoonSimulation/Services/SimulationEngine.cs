namespace MoonSimulation.Services;

/// <summary>
/// Drives the simulation loop — advances the moon's orbital position
/// at a configurable speed and notifies subscribers on each tick.
/// </summary>
public class SimulationEngine : ISimulationEngine
{
    private IDispatcherTimer? _timer;
    private readonly Models.OrbitalState _state = new();
    private bool _isRunning;

    // One full orbit = 29.5 Earth days.  At 1x speed we complete it in 120 seconds real-time.
    private const double OrbitPeriodDays = 29.5;
    private const double BaseSecondsPerOrbit = 120.0;
    private const double TickIntervalSeconds = 1.0 / 60.0; // 60 fps

    /// <summary>Speed multiplier: 0.1 (slow) → 3 (fast).</summary>
    public double Speed { get; set; } = 0.5;

    public Models.OrbitalState State => _state;
    public bool IsRunning => _isRunning;

    /// <summary>Fired every tick so views can redraw.</summary>
    public event Action? OnTick;

    public void Start(IDispatcher dispatcher)
    {
        if (_timer != null) return;

        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(TickIntervalSeconds);
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
        _isRunning = true;
    }

    public void TogglePause()
    {
        if (_timer == null) return;
        if (_isRunning)
        {
            _timer.Stop();
            _isRunning = false;
        }
        else
        {
            _timer.Start();
            _isRunning = true;
        }
    }

    private void Tick()
    {
        // Degrees per second at 1x speed
        double degreesPerSecond = 360.0 / BaseSecondsPerOrbit;

        // Advance moon angle
        double deltaDegrees = degreesPerSecond * Speed * TickIntervalSeconds;
        _state.MoonAngleDegrees = (_state.MoonAngleDegrees + deltaDegrees) % 360.0;

        // Advance Earth rotation — Earth spins ~29.5 times per lunar orbit
        double earthDegreesPerSecond = degreesPerSecond * OrbitPeriodDays;
        double earthDelta = earthDegreesPerSecond * Speed * TickIntervalSeconds;
        _state.EarthRotationDegrees = (_state.EarthRotationDegrees + earthDelta) % 360.0;

        // Track elapsed days
        double daysPerSecond = OrbitPeriodDays / BaseSecondsPerOrbit;
        _state.ElapsedDays += daysPerSecond * Speed * TickIntervalSeconds;

        OnTick?.Invoke();
    }
}
