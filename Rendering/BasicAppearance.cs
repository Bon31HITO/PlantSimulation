using PlantSim.Genetic;
using PlantSim.Rendering;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PlantSim.Rendering;

public class BasicAppearance : IPlantAppearance
{
    public string Name { get; }
    private static readonly Dictionary<string, BasicAppearance> Appearances = new();

    static BasicAppearance()
    {
        var maple = new BasicAppearance("Maple")
        {
            _trunk = new DiffuseMaterial(new LinearGradientBrush(Color.FromRgb(101, 67, 33), Color.FromRgb(139, 90, 43), 90)),
            _leaf = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(50, 160, 50))),
            _variegatedLeaf = new DiffuseMaterial(new LinearGradientBrush(Colors.White, Color.FromRgb(50, 160, 50), 45)),
            _flowerMaterials = { { "Default", new DiffuseMaterial(Brushes.Transparent) } }
        };
        Appearances.Add(maple.Name, maple);

        var pine = new BasicAppearance("Pine")
        {
            _trunk = new DiffuseMaterial(new LinearGradientBrush(Color.FromRgb(80, 50, 20), Color.FromRgb(110, 70, 40), 90)),
            _leaf = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(20, 80, 20))),
            _variegatedLeaf = new DiffuseMaterial(new LinearGradientBrush(Colors.Yellow, Color.FromRgb(20, 80, 20), 45)),
            _flowerMaterials = { { "Default", new DiffuseMaterial(Brushes.SaddleBrown) } }
        };
        Appearances.Add(pine.Name, pine);

        var rose = new BasicAppearance("Rose")
        {
            _trunk = new DiffuseMaterial(new LinearGradientBrush(Color.FromRgb(80, 50, 20), Color.FromRgb(110, 70, 40), 90)),
            _leaf = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(40, 120, 50))),
            _variegatedLeaf = new DiffuseMaterial(new LinearGradientBrush(Colors.LightYellow, Color.FromRgb(40, 120, 50), 45)),
            _flowerMaterials = {
                { "HotPink", new DiffuseMaterial(new RadialGradientBrush(Colors.White, Colors.HotPink)) },
                { "White", new DiffuseMaterial(new SolidColorBrush(Colors.WhiteSmoke)) },
                { "Red", new DiffuseMaterial(new SolidColorBrush(Colors.DarkRed)) },
                { "Yellow", new DiffuseMaterial(new SolidColorBrush(Colors.Gold)) },
                { "LightPink", new DiffuseMaterial(new SolidColorBrush(Colors.LightPink)) }
            }
        };
        Appearances.Add(rose.Name, rose);

        var dandelion = new BasicAppearance("Dandelion")
        {
            _trunk = new DiffuseMaterial(new SolidColorBrush(Colors.Transparent)),
            _leaf = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(34, 139, 34))),
            _variegatedLeaf = new DiffuseMaterial(new LinearGradientBrush(Colors.LightGoldenrodYellow, Color.FromRgb(34, 139, 34), 45)),
            _flowerMaterials = { { "Default", new DiffuseMaterial(Brushes.Yellow) } }
        };
        Appearances.Add(dandelion.Name, dandelion);
    }

    private Material _trunk;
    private Material _leaf;
    private Material _variegatedLeaf;
    private readonly Dictionary<string, Material> _flowerMaterials = new();

    public BasicAppearance(string name) { Name = name; }

    public Material GetTrunkMaterial(Gene gene, double ageRatio)
    {
        return Appearances.TryGetValue(Name, out var a) ? a._trunk : new DiffuseMaterial(Brushes.Gray);
    }

    public Material GetLeafMaterial(Gene gene, double ageRatio, int season)
    {
        if (!Appearances.TryGetValue(Name, out var a)) return new DiffuseMaterial(Brushes.Gray);
        return gene.IsVariegated ? a._variegatedLeaf : a._leaf;
    }

    public Material GetFlowerMaterial(Gene gene)
    {
        if (Appearances.TryGetValue(Name, out var a) && a._flowerMaterials.TryGetValue(gene.FlowerColor, out var material))
        {
            return material;
        }
        // フォールバック
        return Appearances.TryGetValue(Name, out a) && a._flowerMaterials.Count > 0
            ? a._flowerMaterials.First().Value
            : new DiffuseMaterial(Brushes.Gray);
    }
}