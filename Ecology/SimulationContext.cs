using PlantSim.Core;

namespace PlantSim.Ecology;

public class SimulationContext
{
    public World World { get; }
    public List<Plant> AllPlants { get; }
    public List<string> Log { get; }
    public long TimeStep { get; }
    public Random Random { get; }

    public SimulationContext(World world, List<Plant> allPlants, List<string> log, long timeStep, Random random)
    {
        World = world; AllPlants = allPlants; Log = log; TimeStep = timeStep; Random = random;
    }
}
