using PlantSim.Core;
using PlantSim.Ecology;

namespace PlantSim.Strategy;

public class EnergyStrategy : IStrategy
{
    public void Execute(Plant plant, SimulationContext context)
    {
        plant.Energy -= plant.Organs.Count * 0.012;

        double health = context.World.GetHealth(plant.GetRootPosition(), plant.Species.BaseGene);
        if (health < 0.3) plant.Energy -= (1.0 - health) * 1.0;

        context.World.ConsumeNutrients(plant.GetRootPosition(), plant.Gene.NutrientUptakeRates, plant.Organs.Count * 0.0001);

        foreach (var leaf in plant.Organs.OfType<Leaf>())
        {
            bool isShadowed = context.World.IsShadowed(leaf.Position);
            plant.Energy += (isShadowed ? 0.25 : 1.0) * leaf.Area * 0.95 * context.World.LightIntensity * health;
        }
    }
}
