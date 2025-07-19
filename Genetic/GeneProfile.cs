using PlantSim.Ecology;
using System.Collections.Generic;

namespace PlantSim.Genetic;

public class GeneProfile
{
    public double GrowthSpeed { get; set; }
    public double BranchingChance { get; set; }
    public double LeafSize { get; set; }
    public double FlowerChance { get; set; }
    public double ApicalDominance { get; set; }
    public double TrunkThickness { get; set; }
    public int MaxAge { get; set; }
    public int EnergyToFlower { get; set; }
    public int SeedCount { get; set; }
    public double FruitSize { get; set; } = 0.5;
    public Dictionary<NutrientType, double> NutrientUptakeRates { get; set; }
    public List<string> PossibleFlowerColors { get; set; } = new List<string> { "Default" };
    public double VariegationChance { get; set; } = 0;
}
