using System.Windows.Media.Media3D;
using PlantSim.Core;
using PlantSim.Ecology;
using PlantSim.Genetic;

namespace PlantSim.Simulation;

public class SimulationManager
{
    public List<Plant> Plants { get; } = new();
    public World World { get; }
    public long TimeStep { get; private set; } = 0;

    private readonly Random _random;
    private const int MaxPlants = 300;
    private const double SoilRange = 80.0;

    public SimulationManager(Random random)
    {
        _random = random;
        World = new World(_random, SoilRange);
    }

    public void Initialize(int initialSeedCount)
    {
        Plants.Clear();
        TimeStep = 0;
        for (int i = 0; i < initialSeedCount; i++)
        {
            var speciesDef = SpeciesManager.GetRandomDefinition(_random);
            var seedPosition = new Point3D((_random.NextDouble() - 0.5) * SoilRange, 0.1, (_random.NextDouble() - 0.5) * SoilRange);
            Plants.Add(new Plant(seedPosition, speciesDef, _random));
        }
    }

    public void SimulateSingleStep(List<string> logHistory)
    {
        World.Update();
        World.BuildCanopyMap(Plants);
        var context = new SimulationContext(World, Plants, logHistory, TimeStep, _random);

        var plantsToRemove = new List<Plant>();
        foreach (var plant in Plants)
        {
            plant.Update(context);
            if (plant.State == PlantState.Dead) plantsToRemove.Add(plant);
        }

        foreach (var p in plantsToRemove)
        {
            Plants.Remove(p);
            World.ReturnNutrientsToSoil(p);
        }

        var seedsToAdd = new List<Plant>();
        foreach (var plant in Plants)
        {
            if (plant.HasNewSeeds) seedsToAdd.AddRange(plant.HarvestSeeds());
        }

        if (seedsToAdd.Any() && Plants.Count < MaxPlants)
        {
            Plants.AddRange(seedsToAdd.Take(MaxPlants - Plants.Count));
        }
        TimeStep++;
    }
}
