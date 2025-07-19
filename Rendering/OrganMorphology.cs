namespace PlantSim.Rendering;

public enum LeafShape { Simple, Lobed, Palmate, Needle }

public class OrganMorphology
{
    public LeafShape LeafShape { get; set; } = LeafShape.Simple;
}
