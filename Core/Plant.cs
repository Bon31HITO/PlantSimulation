using PlantSim.Ecology;
using PlantSim.Genetic;
using System.Windows.Media.Media3D;

namespace PlantSim.Core;

public class Plant
{
    public Guid Id { get; } = Guid.NewGuid();
    public SpeciesDefinition Species { get; }
    public Gene Gene { get; }
    public PlantState State { get; set; } = PlantState.Seed;
    public double Energy { get; set; }
    public int Age { get; set; }

    public List<Organ> Organs { get; } = new();
    public List<Organ> NewlyCreatedOrgans { get; } = new();
    public List<Plant> NewSeeds { get; } = new();
    public bool HasNewSeeds => NewSeeds.Any();

    public Plant(Point3D position, SpeciesDefinition species, Random rand)
    {
        Species = species;
        Gene = new Gene(species.BaseGene, rand);
        Energy = 50;
        var root = new Root(this, null, position, new Vector3D(0, -1, 0), Gene.TrunkThickness);
        AddOrgan(root, false);
    }

    public Plant(Point3D position, Plant parent, Random rand)
    {
        Species = parent.Species;
        Gene = parent.Gene.Mutate(rand);
        Energy = 50;
        var root = new Root(this, null, position, new Vector3D(0, -1, 0), Gene.TrunkThickness);
        AddOrgan(root, false);
    }

    public void Update(SimulationContext context)
    {
        if (State == PlantState.Dead) return;
        Age++;
        Organs.ForEach(o => o.Age++);

        Species.LifecycleStrategy.Execute(this, context);
        Species.EnergyStrategy.Execute(this, context);
        Species.GrowthStrategy.Execute(this, context);
        Species.ReproductionStrategy.Execute(this, context);

        if ((Energy <= 0 || Age > Gene.MaxAge) && State != PlantState.Dead)
        {
            context.Log.Add($"[T:{context.TimeStep}] A {Species.SpeciesID} died (Age: {Age}, Energy: {Energy:F0}).");
            State = PlantState.Dead;
        }
    }

    public void AddOrgan(Organ organ, bool isRenderable = true)
    {
        Organs.Add(organ);
        if (isRenderable) NewlyCreatedOrgans.Add(organ);
    }

    public Point3D GetRootPosition() => Organs.OfType<Root>().First().Position;
    public List<Plant> HarvestSeeds() { var h = new List<Plant>(NewSeeds); NewSeeds.Clear(); return h; }
    public List<Organ> FlushNewOrgans() { var n = new List<Organ>(NewlyCreatedOrgans); NewlyCreatedOrgans.Clear(); return n; }
}
