using PlantSim.Ecology;
using PlantSim.Genetic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlantSim.Genetic;

public class Gene
{
    public double GrowthSpeed { get; }
    public double BranchingChance { get; }
    public double LeafSize { get; }
    public double FlowerChance { get; }
    public double ApicalDominance { get; }
    public double TrunkThickness { get; }
    public int MaxAge { get; }
    public int EnergyToFlower { get; }
    public int SeedCount { get; }
    public double FruitSize { get; }
    public Dictionary<NutrientType, double> NutrientUptakeRates { get; }
    public string FlowerColor { get; }
    public bool IsVariegated { get; }

    public Gene(GeneProfile baseProfile, Random rand)
    {
        GrowthSpeed = baseProfile.GrowthSpeed * (1.0 + (rand.NextDouble() - 0.5) * 0.2);
        BranchingChance = baseProfile.BranchingChance * (1.0 + (rand.NextDouble() - 0.5) * 0.2);
        LeafSize = baseProfile.LeafSize * (1.0 + (rand.NextDouble() - 0.5) * 0.2);
        FlowerChance = baseProfile.FlowerChance * (1.0 + (rand.NextDouble() - 0.5) * 0.2);
        ApicalDominance = baseProfile.ApicalDominance * (1.0 + (rand.NextDouble() - 0.5) * 0.2);
        TrunkThickness = baseProfile.TrunkThickness * (1.0 + (rand.NextDouble() - 0.5) * 0.2);
        MaxAge = (int)(baseProfile.MaxAge * (1.0 + (rand.NextDouble() - 0.5) * 0.3));
        EnergyToFlower = (int)(baseProfile.EnergyToFlower * (1.0 + (rand.NextDouble() - 0.5) * 0.2));
        SeedCount = (int)(baseProfile.SeedCount * (1.0 + (rand.NextDouble() - 0.5) * 0.2));
        FruitSize = baseProfile.FruitSize * (1.0 + (rand.NextDouble() - 0.5) * 0.2);
        NutrientUptakeRates = baseProfile.NutrientUptakeRates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        FlowerColor = baseProfile.PossibleFlowerColors.Count > 0 ? baseProfile.PossibleFlowerColors[rand.Next(baseProfile.PossibleFlowerColors.Count)] : "Default";
        IsVariegated = rand.NextDouble() < baseProfile.VariegationChance;
    }

    public Gene Mutate(Random rand)
    {
        var parentProfile = new GeneProfile
        {
            GrowthSpeed = this.GrowthSpeed,
            BranchingChance = this.BranchingChance,
            LeafSize = this.LeafSize,
            FlowerChance = this.FlowerChance,
            ApicalDominance = this.ApicalDominance,
            TrunkThickness = this.TrunkThickness,
            MaxAge = this.MaxAge,
            EnergyToFlower = this.EnergyToFlower,
            SeedCount = this.SeedCount,
            FruitSize = this.FruitSize,
            NutrientUptakeRates = this.NutrientUptakeRates,
            PossibleFlowerColors = new List<string> { this.FlowerColor },
            VariegationChance = this.IsVariegated ? 0.8 : 0.01
        };

        if (rand.NextDouble() < 0.1) parentProfile.GrowthSpeed = Math.Max(0.01, this.GrowthSpeed + (rand.NextDouble() - 0.5) * 0.05);
        if (rand.NextDouble() < 0.1) parentProfile.LeafSize = Math.Max(0.1, this.LeafSize + (rand.NextDouble() - 0.5) * 0.1);
        if (rand.NextDouble() < 0.1) parentProfile.FruitSize = Math.Max(0.1, this.FruitSize + (rand.NextDouble() - 0.5) * 0.1);

        return new Gene(parentProfile, rand);
    }
}
