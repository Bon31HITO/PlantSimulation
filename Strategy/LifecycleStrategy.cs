using PlantSim.Core;
using PlantSim.Ecology;
using PlantSim.Strategy;
using System.Linq;
using System.Windows.Media.Media3D;

namespace PlantSim.Strategy;

public class LifecycleStrategy : IStrategy
{
    public void Execute(Plant plant, SimulationContext context)
    {
        switch (plant.State)
        {
            case PlantState.Seed:
                plant.State = PlantState.Germinating;
                break;
            case PlantState.Germinating:
                plant.Energy -= 5;
                var root = plant.Organs.OfType<Root>().First();
                var startPos = root.Position;
                var endPos = startPos + new Vector3D(0, 0.2, 0);
                var stem = new Stem(plant, root, startPos, new Vector3D(0, 1, 0), plant.Gene.TrunkThickness, endPos);
                plant.AddOrgan(stem);
                plant.AddOrgan(new Meristem(plant, stem, endPos, new Vector3D(0, 1, 0)));
                plant.State = PlantState.Vegetative;
                break;
            case PlantState.Vegetative:
                if (plant.Energy < 10 && plant.Age > 50) plant.State = PlantState.Dormant;
                if (plant.Energy > plant.Gene.EnergyToFlower && context.Random.NextDouble() < plant.Gene.FlowerChance)
                {
                    var meristems = plant.Organs.OfType<Meristem>().Where(m => m.IsActive).ToList();
                    foreach (var tip in meristems)
                    {
                        if (plant.Energy > 20)
                        {
                            plant.Energy -= 20;
                            plant.AddOrgan(new Flower(plant, tip.Parent, tip.Position, tip.Direction));
                            tip.IsActive = false;
                        }
                    }
                    if (plant.Organs.Count(o => o.Type == OrganType.Flower) > 0) plant.State = PlantState.Flowering;
                }
                break;
            case PlantState.Dormant:
                if (plant.Energy > 50) plant.State = PlantState.Vegetative;
                break;
            case PlantState.Flowering:
                if (plant.Organs.OfType<Flower>().Any(f => f.Age > 20))
                {
                    var oldFlowers = plant.Organs.OfType<Flower>().Where(f => f.Age > 20).ToList();
                    foreach (var flower in oldFlowers)
                    {
                        plant.AddOrgan(new Fruit(plant, flower.Parent, flower.Position, flower.Direction, plant.Gene.FruitSize));
                    }
                    plant.Organs.RemoveAll(o => oldFlowers.Contains(o));
                    if (plant.Organs.Count(o => o.Type == OrganType.Fruit) > 0) plant.State = PlantState.Fruiting;
                }
                break;
            case PlantState.Fruiting:
                if (plant.Organs.Count(o => o.Type == OrganType.Fruit) == 0)
                {
                    plant.State = PlantState.Vegetative;
                }
                break;
        }
    }
}
