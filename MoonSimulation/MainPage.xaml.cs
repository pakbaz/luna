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
		SkyCanvas.SetState(_engine.State);

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
		SkyCanvas.InvalidateSurface();

		var (phaseName, phaseEmoji) = _engine.State.Phase;
		PhaseLabel.Text = $"{phaseEmoji}  {phaseName}";

		var (timeName, timeEmoji) = _engine.State.TimeOfDay;
		int hours = (int)_engine.State.HourOfDay;
		int mins = (int)((_engine.State.HourOfDay - hours) * 60);
		TimeLabel.Text = $"{timeEmoji}  {timeName} {hours:D2}:{mins:D2}";
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
