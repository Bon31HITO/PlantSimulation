using PlantSim.Core;
using PlantSim.Ecology;
using System.Windows.Media.Media3D;

namespace PlantSim.Strategy;

public class ReproductionStrategy : IStrategy
{
    public void Execute(Plant plant, SimulationContext context)
    {
        if (plant.State != PlantState.Fruiting) return;

        var ripeFruits = plant.Organs.OfType<Fruit>().Where(f => f.Age > 50).ToList();
        if (!ripeFruits.Any()) return;

        foreach (var fruit in ripeFruits)
        {
            for (int i = 0; i < plant.Gene.SeedCount; i++)
            {
                double spread = 3.0; // Seeds from fruits don't spread as far as wind-dispersed
                var p = fruit.Position + new Vector3D((context.Random.NextDouble() - 0.5) * spread, 0.5, (context.Random.NextDouble() - 0.5) * spread);
                p.Y = 0.1;
                plant.NewSeeds.Add(new Plant(p, plant, context.Random));
            }
        }
        plant.Organs.RemoveAll(o => ripeFruits.Contains(o));

        // Re-activate meristems that were used for flowering
        foreach (var meristem in plant.Organs.OfType<Meristem>().Where(m => !m.IsActive))
        {
            meristem.IsActive = true;
        }
    }
}