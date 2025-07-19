using PlantSim.Genetic;
using System.Windows.Media.Media3D;

namespace PlantSim.Rendering
{
    public interface IPlantAppearance
    {
        string Name { get; }
        Material GetTrunkMaterial(Gene gene, double ageRatio);
        Material GetLeafMaterial(Gene gene, double ageRatio, int season);
        Material GetFlowerMaterial(Gene gene);
    }
}
