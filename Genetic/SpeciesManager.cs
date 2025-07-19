using PlantSim.Genetic;
using PlantSim.Rendering;
using PlantSim.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization; // ★★★ 追加 ★★★

namespace PlantSim.Genetic;

public static class SpeciesManager
{
    public static readonly Dictionary<string, SpeciesDefinition> Definitions = new();
    private static readonly StrategyFactory _strategyFactory = new();

    static SpeciesManager()
    {
        LoadDefinitionsFromFiles();
    }

    private static void LoadDefinitionsFromFiles()
    {
        try
        {
            string speciesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Species");
            if (!Directory.Exists(speciesDir))
            {
                speciesDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Species"));
                if (!Directory.Exists(speciesDir))
                {
                    throw new DirectoryNotFoundException($"Could not find the 'Species' directory. Please ensure it exists at the root of the PlantSim project and its files are set to 'Copy to Output Directory'. Expected path: {speciesDir}");
                }
            }

            // ★★★ 修正点: JsonSerializerOptions を設定 ★★★
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // プロパティ名の大文字/小文字を区別しない
                Converters = { new JsonStringEnumConverter() } // 文字列をenumに変換するコンバータを追加
            };

            foreach (var file in Directory.GetFiles(speciesDir, "*.json"))
            {
                var jsonString = File.ReadAllText(file);
                var blueprint = JsonSerializer.Deserialize<SpeciesBlueprint>(jsonString, options);

                if (blueprint == null) continue;

                var baseAppearance = new BasicAppearance(blueprint.AppearanceName);
                var baseDefinition = new SpeciesDefinition(
                    blueprint.SpeciesID, blueprint.BaseGene,
                    _strategyFactory.GetStrategy(blueprint.LifecycleStrategy), _strategyFactory.GetStrategy(blueprint.EnergyStrategy),
                    _strategyFactory.GetStrategy(blueprint.GrowthStrategy), _strategyFactory.GetStrategy(blueprint.ReproductionStrategy),
                    baseAppearance, blueprint.Morphology);
                Definitions[baseDefinition.SpeciesID] = baseDefinition;

                if (blueprint.Cultivars != null)
                {
                    foreach (var cultivarBlueprint in blueprint.Cultivars)
                    {
                        var cultivarGene = ApplyOverrides(blueprint.BaseGene, cultivarBlueprint.GeneOverrides);
                        var cultivarId = $"{blueprint.SpeciesID} '{cultivarBlueprint.CultivarName}'";
                        var cultivarDefinition = new SpeciesDefinition(
                            cultivarId, cultivarGene,
                            baseDefinition.LifecycleStrategy, baseDefinition.EnergyStrategy,
                            baseDefinition.GrowthStrategy, baseDefinition.ReproductionStrategy,
                            baseAppearance, blueprint.Morphology);
                        Definitions[cultivarDefinition.SpeciesID] = cultivarDefinition;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to load species definitions.", ex);
        }
    }

    private static GeneProfile ApplyOverrides(GeneProfile baseGene, GeneProfile overrides)
    {
        var newGene = new GeneProfile();
        foreach (PropertyInfo prop in typeof(GeneProfile).GetProperties())
        {
            var overrideValue = prop.GetValue(overrides);
            var baseValue = prop.GetValue(baseGene);
            prop.SetValue(newGene, overrideValue ?? baseValue);
        }
        return newGene;
    }

    public static SpeciesDefinition GetRandomDefinition(Random rand)
    {
        if (Definitions.Count == 0) throw new InvalidOperationException("No species definitions are available to select from.");
        return Definitions.Values.ElementAt(rand.Next(Definitions.Count));
    }
}
