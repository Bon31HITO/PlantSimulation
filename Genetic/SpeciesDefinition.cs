using PlantSim.Rendering;
using PlantSim.Strategy;

namespace PlantSim.Genetic;

public class CultivarBlueprint
{
    public string CultivarName { get; set; }
    public GeneProfile GeneOverrides { get; set; }
}

public class SpeciesBlueprint
{
    public string SpeciesID { get; set; }
    public string AppearanceName { get; set; }
    public string LifecycleStrategy { get; set; }
    public string EnergyStrategy { get; set; }
    public string GrowthStrategy { get; set; }
    public string ReproductionStrategy { get; set; }
    public OrganMorphology Morphology { get; set; }
    public GeneProfile BaseGene { get; set; }
    public List<CultivarBlueprint> Cultivars { get; set; }
}

public class SpeciesDefinition
{
    public string SpeciesID { get; }
    public GeneProfile BaseGene { get; }
    public IStrategy LifecycleStrategy { get; }
    public IStrategy EnergyStrategy { get; }
    public IStrategy GrowthStrategy { get; }
    public IStrategy ReproductionStrategy { get; }
    public IPlantAppearance Appearance { get; }
    public OrganMorphology Morphology { get; }

    public SpeciesDefinition(string id, GeneProfile baseGene, IStrategy lifecycle, IStrategy energy, IStrategy growth, IStrategy reproduction, IPlantAppearance appearance, OrganMorphology morphology)
    {
        SpeciesID = id;
        BaseGene = baseGene;
        LifecycleStrategy = lifecycle;
        EnergyStrategy = energy;
        GrowthStrategy = growth;
        ReproductionStrategy = reproduction;
        Appearance = appearance;
        Morphology = morphology;
    }
}
