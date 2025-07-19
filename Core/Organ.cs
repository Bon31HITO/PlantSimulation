using System.Windows.Media.Media3D;

namespace PlantSim.Core;

/// <summary>
/// 全ての植物器官の基底クラス。
/// </summary>
public abstract class Organ
{
    public Plant Owner { get; }
    public Organ Parent { get; set; }
    public OrganType Type { get; }
    public Point3D Position { get; set; }
    public Vector3D Direction { get; set; }
    public int Age { get; set; }

    protected Organ(Plant owner, Organ parent, OrganType type, Point3D position, Vector3D direction)
    {
        Owner = owner; Parent = parent; Type = type; Position = position; Direction = direction; Age = 0;
    }
}

// --- 具体的な器官クラス ---

public class Root : Organ
{
    public double Thickness { get; set; }
    public Root(Plant owner, Organ parent, Point3D position, Vector3D direction, double thickness)
        : base(owner, parent, OrganType.Root, position, direction) { Thickness = thickness; }
}

public class Stem : Organ
{
    public Point3D EndPosition { get; set; }
    public double Thickness { get; set; }
    public Stem(Plant owner, Organ parent, Point3D position, Vector3D direction, double thickness, Point3D endPosition)
        : base(owner, parent, OrganType.Stem, position, direction) { Thickness = thickness; EndPosition = endPosition; }
}

public class Leaf : Organ
{
    public double Area { get; set; }
    public Leaf(Plant owner, Organ parent, Point3D position, Vector3D direction, double area)
        : base(owner, parent, OrganType.Leaf, position, direction) { Area = area; }
}

public class Flower : Organ
{
    public Flower(Plant owner, Organ parent, Point3D position, Vector3D direction)
        : base(owner, parent, OrganType.Flower, position, direction) { }
}

public class Fruit : Organ
{
    public double Size { get; set; }
    public Fruit(Plant owner, Organ parent, Point3D position, Vector3D direction, double size)
        : base(owner, parent, OrganType.Fruit, position, direction) { Size = size; }
}

/// <summary>
/// 植物の成長を司る成長点。
/// </summary>
public class Meristem : Organ
{
    public bool IsActive { get; set; } = true;
    public Meristem(Plant owner, Organ parent, Point3D position, Vector3D direction)
        : base(owner, parent, OrganType.Meristem, position, direction) { }
}
