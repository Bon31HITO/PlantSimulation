using PlantSim.Rendering;
using PlantSim.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace PlantSim;

/// <summary>
/// アプリケーションのメインウィンドウ。UIイベントの処理と、シミュレーションの進行管理クラスの呼び出しに専念します。
/// </summary>
public partial class MainWindow : Window
{
    private SimulationManager _simManager;
    private PlantModelFactory _modelFactory;
    private readonly Random _random = new Random();
    private readonly DispatcherTimer _timer = new DispatcherTimer();
    private readonly List<string> _logHistory = new List<string>();

    private int _simulationSpeed = 1;
    private const int MaxSpeed = 50;
    private const int InitialSeedCount = 25;
    private const int MaxLogLines = 15;

    private readonly Dictionary<Guid, ModelVisual3D> _plantModels = new();

    private bool _isLeftMouseButtonDown, _isRightMouseButtonDown;
    private Point _lastMousePosition;
    private Point3D _cameraTarget;
    private readonly Point3D _defaultCameraPosition = new(0, 20, 60);
    private readonly Point3D _defaultCameraTarget = new(0, 5, 0);

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetupScene();
        _timer.Interval = TimeSpan.FromMilliseconds(20);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void SetupScene()
    {
        foreach (var plantModel in _plantModels.Values) mainViewport.Children.Remove(plantModel);
        _plantModels.Clear();
        _logHistory.Clear();

        _simManager = new SimulationManager(_random);
        _modelFactory = new PlantModelFactory(_random);
        _simManager.Initialize(InitialSeedCount);

        ResetCamera();
        _logHistory.Add("Simulation (re)started. Welcome!");
        UpdateLog();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        for (int i = 0; i < _simulationSpeed; i++) _simManager.SimulateSingleStep(_logHistory);
        RenderScene();
        UpdateLog();
    }

    private void RenderScene()
    {
        var deadPlantIds = _plantModels.Keys.Where(id => !_simManager.Plants.Any(p => p.Id == id)).ToList();
        foreach (var id in deadPlantIds)
        {
            mainViewport.Children.Remove(_plantModels[id]);
            _plantModels.Remove(id);
        }

        foreach (var plant in _simManager.Plants)
        {
            var newOrgans = plant.FlushNewOrgans();
            if (!newOrgans.Any()) continue;

            if (!_plantModels.ContainsKey(plant.Id))
            {
                var plantContainer = new ModelVisual3D();
                _plantModels[plant.Id] = plantContainer;
                mainViewport.Children.Add(plantContainer);
            }

            foreach (var organ in newOrgans)
            {
                var model = _modelFactory.CreateOrganModel(organ);
                if (model != null) _plantModels[plant.Id].Children.Add(model);
            }
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        bool isCtrlDown = Keyboard.Modifiers == ModifierKeys.Control;
        if (e.Key == Key.Up && !isCtrlDown) _simulationSpeed = Math.Min(MaxSpeed, _simulationSpeed + 1);
        if (e.Key == Key.Down && !isCtrlDown) _simulationSpeed = Math.Max(1, _simulationSpeed - 1);
        if (e.Key == Key.Up && isCtrlDown) _simManager.World.LightIntensity = Math.Min(3.0, _simManager.World.LightIntensity + 0.1);
        if (e.Key == Key.Down && isCtrlDown) _simManager.World.LightIntensity = Math.Max(0.1, _simManager.World.LightIntensity - 0.1);
        if (e.Key == Key.R) SetupScene();
    }

    private void UpdateLog()
    {
        if (_logHistory.Count > MaxLogLines) _logHistory.RemoveRange(0, _logHistory.Count - MaxLogLines);
        var sb = new StringBuilder();
        sb.AppendLine($"--- PlantSim | Spd: {_simulationSpeed}x (Up/Down) | Light: {_simManager.World.LightIntensity:F1} (Ctrl+Up/Down) | Reset Sim [R] ---");
        var speciesCounts = _simManager.Plants.GroupBy(p => p.Species.SpeciesID).Select(g => $"{g.Key}: {g.Count()}").OrderBy(s => s);
        sb.AppendLine($"Time: {_simManager.TimeStep} | Plants: {_simManager.Plants.Count} ({string.Join(", ", speciesCounts)})");
        sb.AppendLine("--------------------------------------------------------------------------------------------------");
        for (int i = _logHistory.Count - 1; i >= 0; i--) sb.AppendLine(_logHistory[i]);
        logTextBlock.Text = sb.ToString();
    }

    #region Camera Controls
    private void Viewport_MouseButtonDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) { _isLeftMouseButtonDown = true; _lastMousePosition = e.GetPosition(mainViewport); } if (e.RightButton == MouseButtonState.Pressed) { _isRightMouseButtonDown = true; _lastMousePosition = e.GetPosition(mainViewport); } }
    private void Viewport_MouseButtonUp(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Released) _isLeftMouseButtonDown = false; if (e.RightButton == MouseButtonState.Released) _isRightMouseButtonDown = false; }
    private void Viewport_MouseMove(object sender, MouseEventArgs e) { var currentMousePosition = e.GetPosition(mainViewport); var delta = currentMousePosition - _lastMousePosition; if (_isLeftMouseButtonDown) Orbit(delta.X, delta.Y); else if (_isRightMouseButtonDown) Pan(delta.X, delta.Y); _lastMousePosition = currentMousePosition; }
    private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e) => Zoom(e.Delta);
    private void UpdateCamera() { mainCamera.LookDirection = _cameraTarget - mainCamera.Position; }
    private void Orbit(double dx, double dy) { var toCamera = mainCamera.Position - _cameraTarget; var yawTransform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), -dx * 0.2)); toCamera = yawTransform.Transform(toCamera); var right = Vector3D.CrossProduct(mainCamera.LookDirection, new Vector3D(0, 1, 0)); var pitchTransform = new RotateTransform3D(new AxisAngleRotation3D(right, -dy * 0.2)); toCamera = pitchTransform.Transform(toCamera); mainCamera.Position = _cameraTarget + toCamera; UpdateCamera(); }
    private void Pan(double dx, double dy) { var look = mainCamera.LookDirection; look.Normalize(); var right = Vector3D.CrossProduct(look, new Vector3D(0, 1, 0)); right.Normalize(); var up = Vector3D.CrossProduct(right, look); var offset = right * -dx * 0.1 + up * dy * 0.1; mainCamera.Position += offset; _cameraTarget += offset; UpdateCamera(); }
    private void Zoom(double delta) { var look = mainCamera.LookDirection; look.Normalize(); var zoomAmount = delta > 0 ? 2.0 : -2.0; mainCamera.Position += look * zoomAmount; if ((mainCamera.Position - _cameraTarget).Length < 3.0 && delta < 0) { mainCamera.Position -= look * zoomAmount; } UpdateCamera(); }
    private void ResetCamera() { mainCamera.Position = _defaultCameraPosition; _cameraTarget = _defaultCameraTarget; UpdateCamera(); }
    #endregion
}
