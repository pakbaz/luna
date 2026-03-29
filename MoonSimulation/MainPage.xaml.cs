using MoonSimulation.Services;

namespace MoonSimulation;

public partial class MainPage : ContentPage
{
	private readonly SimulationEngine _engine = new();

	public MainPage()
	{
		InitializeComponent();

		OrbitalCanvas.SetState(_engine.State);
		PhaseCanvas.SetState(_engine.State);

		_engine.OnTick += OnSimulationTick;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_engine.Start(Dispatcher);
	}

	private void OnSimulationTick()
	{
		OrbitalCanvas.InvalidateSurface();
		PhaseCanvas.InvalidateSurface();

		var (name, emoji) = _engine.State.Phase;
		PhaseLabel.Text = $"{emoji}  {name}";
	}

	private void OnPlayPauseClicked(object? sender, EventArgs e)
	{
		_engine.TogglePause();
		PlayPauseBtn.Text = _engine.IsRunning ? "⏸️" : "▶️";
	}

	private void OnSpeedChanged(object? sender, ValueChangedEventArgs e)
	{
		_engine.Speed = e.NewValue;
		SpeedLabel.Text = $"{e.NewValue:F1}x";
	}
}
