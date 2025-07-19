using PlantSim.Core;
using System.Windows.Media.Media3D;

namespace PlantSim.Rendering
{
    public class PlantModelFactory
    {
        private readonly Random _rand;
        public PlantModelFactory(Random rand) { _rand = rand; }

        public ModelVisual3D CreateOrganModel(Organ organ)
        {
            if (organ.Parent == null && organ.Type != OrganType.Root) return null;

            var appearance = organ.Owner.Species.Appearance;
            var gene = organ.Owner.Gene;
            var ageRatio = (double)organ.Owner.Age / gene.MaxAge;

            return organ.Type switch
            {
                OrganType.Stem => CreateStemModel(organ as Stem, appearance.GetTrunkMaterial(gene, ageRatio)),
                OrganType.Leaf => CreateLeafModel(organ as Leaf, appearance.GetLeafMaterial(gene, ageRatio, 0)),
                OrganType.Flower => CreateFlowerModel(organ as Flower, appearance.GetFlowerMaterial(gene)),
                _ => null,
            };
        }

        private ModelVisual3D CreateStemModel(Stem stem, Material material)
        {
            return CreateCylinder(stem.Position, stem.EndPosition, stem.Parent is Stem parentStem ? parentStem.Thickness : stem.Thickness, stem.Thickness, material);
        }

        private ModelVisual3D CreateLeafModel(Leaf leaf, Material material)
        {
            return leaf.Owner.Species.Morphology.LeafShape switch
            {
                LeafShape.Needle => CreateCylinder(leaf.Position, leaf.Position + leaf.Direction * leaf.Area, 0.02, 0, material),
                LeafShape.Palmate => CreateComplexLeaf(leaf, material, 5),
                _ => CreateSimpleLeaf(leaf, material),
            };
        }

        private ModelVisual3D CreateFlowerModel(Flower flower, Material material)
        {
            return CreateSphere(flower.Position, flower.Owner.Gene.LeafSize * 0.5, material);
        }

        #region Primitive Creation Methods

        private ModelVisual3D CreateCylinder(Point3D p1, Point3D p2, double r1, double r2, Material m)
        {
            if (p1 == p2) return null;
            var mesh = new MeshGeometry3D(); int seg = 8; var axis = p2 - p1; axis.Normalize();
            var v1 = Math.Abs(axis.Y) > 0.9 ? new Vector3D(1, 0, 0) : new Vector3D(0, 1, 0);
            var v2 = Vector3D.CrossProduct(axis, v1); v2.Normalize(); var v3 = Vector3D.CrossProduct(axis, v2); v3.Normalize();
            for (int i = 0; i <= seg; i++)
            {
                double a = 2 * Math.PI / seg * i; double noise1 = 1.0 + (_rand.NextDouble() - 0.5) * 0.15; double noise2 = 1.0 + (_rand.NextDouble() - 0.5) * 0.15;
                mesh.Positions.Add(p1 + (v2 * Math.Cos(a) + v3 * Math.Sin(a)) * r1 * noise1);
                mesh.Positions.Add(p2 + (v2 * Math.Cos(a) + v3 * Math.Sin(a)) * r2 * noise2);
            }
            for (int i = 0; i < seg; i++)
            {
                int i0 = i * 2, i1 = i0 + 1, i2 = (i + 1) * 2, i3 = i2 + 1;
                mesh.TriangleIndices.Add(i0); mesh.TriangleIndices.Add(i2); mesh.TriangleIndices.Add(i1);
                mesh.TriangleIndices.Add(i1); mesh.TriangleIndices.Add(i2); mesh.TriangleIndices.Add(i3);
            }
            return new ModelVisual3D { Content = new GeometryModel3D(mesh, m) };
        }

        private ModelVisual3D CreateSimpleLeaf(Leaf leaf, Material material)
        {
            var c = leaf.Position; var d = leaf.Direction; var s = leaf.Area;
            var mesh = new MeshGeometry3D(); var n = d; n.Normalize();
            var up = Math.Abs(Vector3D.DotProduct(n, new Vector3D(0, 1, 0))) > 0.99 ? new Vector3D(1, 0, 0) : new Vector3D(0, 1, 0);
            var r = Vector3D.CrossProduct(n, up); r.Normalize(); var bendAxis = r; double bendAmount = s * 0.1 * (_rand.NextDouble() + 0.5);
            var pts = new Point3D[5]; pts[0] = c; pts[1] = c + n * s * 0.4 - r * s * 0.5 + bendAxis * bendAmount; pts[2] = c + n * s * 1.0;
            pts[3] = c + n * s * 0.4 + r * s * 0.5 + bendAxis * bendAmount; pts[4] = c + n * s * 0.2 + bendAxis * bendAmount * 0.5;
            foreach (var p in pts) mesh.Positions.Add(p);
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(3);
            return new ModelVisual3D { Content = new GeometryModel3D(mesh, material) };
        }

        private ModelVisual3D CreateComplexLeaf(Leaf leaf, Material material, int lobes)
        {
            var group = new Model3DGroup();
            var mainDirection = leaf.Direction; mainDirection.Normalize();
            for (int i = 0; i < lobes; i++)
            {
                var angle = -45 + (90.0 / (lobes - 1) * i) + (_rand.NextDouble() - 0.5) * 10;
                var rotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), angle));
                var lobeDirection = rotation.Transform(mainDirection);
                var lobeLeaf = new Leaf(leaf.Owner, leaf.Parent, leaf.Position, lobeDirection, leaf.Area / lobes);
                group.Children.Add(CreateSimpleLeaf(lobeLeaf, material).Content);
            }
            return new ModelVisual3D { Content = group };
        }

        private ModelVisual3D CreateSphere(Point3D center, double r, Material m)
        {
            var mesh = new MeshGeometry3D(); int lat = 8, lon = 8;
            for (int i = 0; i <= lat; i++) { double lt = Math.PI / lat * i; for (int j = 0; j <= lon; j++) { double ln = 2 * Math.PI / lon * j; mesh.Positions.Add(new Point3D(center.X + r * Math.Sin(lt) * Math.Cos(ln), center.Y + r * Math.Cos(lt), center.Z + r * Math.Sin(lt) * Math.Sin(ln))); } }
            for (int i = 0; i < lat; i++) { for (int j = 0; j < lon; j++) { int v0 = i * (lon + 1) + j, v1 = v0 + 1, v2 = (i + 1) * (lon + 1) + j, v3 = v2 + 1; mesh.TriangleIndices.Add(v0); mesh.TriangleIndices.Add(v2); mesh.TriangleIndices.Add(v1); mesh.TriangleIndices.Add(v1); mesh.TriangleIndices.Add(v2); mesh.TriangleIndices.Add(v3); } }
            return new ModelVisual3D { Content = new GeometryModel3D(mesh, m) };
        }

        #endregion
    }
}
