using PlantSim.Core;
using PlantSim.Genetic;
using System.Windows;
using System.Windows.Media.Media3D;

namespace PlantSim.Ecology;

/// <summary>
/// シミュレーションの世界（土壌、天候、光など）を管理します。
/// </summary>
public class World
{
    public double LightIntensity { get; set; } = 1.0;
    private readonly Dictionary<Point, double> _canopyHeightMap = new();
    private readonly Dictionary<Point, double> _soilMoistureMap = new();
    private readonly Dictionary<NutrientType, Dictionary<Point, double>> _soilNutrientMaps = new();

    private readonly Random _random;
    private const double EnvGridSize = 2.0;

    public World(Random random, double soilRange)
    {
        _random = random;
        foreach (NutrientType type in Enum.GetValues(typeof(NutrientType))) _soilNutrientMaps[type] = new();

        for (int x = (int)(-soilRange / EnvGridSize); x < (int)(soilRange / EnvGridSize); x++)
        {
            for (int z = (int)(-soilRange / EnvGridSize); z < (int)(soilRange / EnvGridSize); z++)
            {
                var gridPoint = new Point(x, z);
                _soilMoistureMap[gridPoint] = 1.0;
                foreach (var map in _soilNutrientMaps.Values) map[gridPoint] = 1.0;
            }
        }
    }

    public void Update()
    {
        var moistureKeys = _soilMoistureMap.Keys.ToList();
        foreach (var key in moistureKeys) _soilMoistureMap[key] *= 0.9995;
        if (_random.NextDouble() < 0.0025) foreach (var key in moistureKeys) _soilMoistureMap[key] = 1.0;

        foreach (var map in _soilNutrientMaps.Values)
        {
            foreach (var key in map.Keys.ToList()) map[key] = Math.Min(1.0, map[key] + 0.00008);
        }
    }

    public double GetHealth(Point3D position, GeneProfile needs)
    {
        double moisture = _soilMoistureMap.TryGetValue(GetGridPoint(position), out double m) ? m : 0;
        double minNutrientRatio = 1.0;
        var nutrients = GetNutrients(position);
        foreach (var (needType, needRate) in needs.NutrientUptakeRates)
        {
            double available = nutrients.Levels.TryGetValue(needType, out double val) ? val : 0;
            if (needRate > 0) minNutrientRatio = Math.Min(minNutrientRatio, available / needRate);
        }
        return Math.Clamp(moisture, 0.1, 1.0) * Math.Clamp(minNutrientRatio, 0.1, 1.0);
    }

    public void ConsumeNutrients(Point3D position, Dictionary<NutrientType, double> uptakeRates, double amount)
    {
        var gridPoint = GetGridPoint(position);
        foreach (var (type, rate) in uptakeRates)
        {
            if (_soilNutrientMaps[type].TryGetValue(gridPoint, out double currentLevel))
                _soilNutrientMaps[type][gridPoint] = Math.Max(0, currentLevel - amount * rate);
        }
    }

    public void ReturnNutrientsToSoil(Plant plant)
    {
        double nutrientReturn = plant.Organs.Count * 0.1;
        var gridPoint = GetGridPoint(plant.GetRootPosition());
        foreach (var map in _soilNutrientMaps.Values)
            if (map.ContainsKey(gridPoint)) map[gridPoint] = Math.Min(1.5, map[gridPoint] + nutrientReturn);
    }

    public void BuildCanopyMap(IEnumerable<Plant> plants)
    {
        _canopyHeightMap.Clear();
        foreach (var plant in plants.Where(p => p != null))
        {
            foreach (var leaf in plant.Organs.OfType<Leaf>())
            {
                var gridPoint = GetGridPoint(leaf.Position);
                if (_canopyHeightMap.TryGetValue(gridPoint, out double currentMaxY))
                    _canopyHeightMap[gridPoint] = Math.Max(currentMaxY, leaf.Position.Y);
                else
                    _canopyHeightMap[gridPoint] = leaf.Position.Y;
            }
        }
    }

    public NutrientPacket GetNutrients(Point3D position)
    {
        var packet = new NutrientPacket();
        var gridPoint = GetGridPoint(position);
        foreach (var (type, map) in _soilNutrientMaps)
        {
            packet.Levels[type] = map.TryGetValue(gridPoint, out double val) ? val : 0;
        }
        return packet;
    }

    public bool IsShadowed(Point3D position) => _canopyHeightMap.TryGetValue(GetGridPoint(position), out double maxH) && position.Y < maxH - 0.1;
    private Point GetGridPoint(Point3D position) => new((int)(position.X / EnvGridSize), (int)(position.Z / EnvGridSize));
}
