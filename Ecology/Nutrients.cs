namespace PlantSim.Ecology;

public enum NutrientType { Nitrogen, Phosphorus, Potassium, Magnesium }

public struct NutrientPacket
{
    public Dictionary<NutrientType, double> Levels;
    public NutrientPacket() { Levels = new Dictionary<NutrientType, double>(); }
}
