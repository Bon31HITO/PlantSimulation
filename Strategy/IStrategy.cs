using PlantSim.Core;
using PlantSim.Ecology;

namespace PlantSim.Strategy;

public interface IStrategy
{
    void Execute(Plant plant, SimulationContext context);
}
