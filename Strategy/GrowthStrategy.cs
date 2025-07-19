using PlantSim.Core;
using PlantSim.Ecology;
using PlantSim.Strategy;
using System;
using System.Linq;
using System.Windows.Media.Media3D;

namespace PlantSim.Strategy
{
    public enum GrowthType { Apical, Basal, Vine, Rosette }

    public class GrowthStrategy : IStrategy
    {
        private readonly GrowthType _growthType;
        private readonly double _apicalDominanceFactor;
        private readonly double _branchingAngle;

        public GrowthStrategy(GrowthType growthType = GrowthType.Apical, double apicalDominanceFactor = 0.5, double branchingAngle = 60)
        {
            _growthType = growthType;
            _apicalDominanceFactor = apicalDominanceFactor;
            _branchingAngle = branchingAngle;
        }

        public void Execute(Plant plant, SimulationContext context)
        {
            if (plant.State != PlantState.Vegetative) return;

            switch (_growthType)
            {
                case GrowthType.Apical:
                    ExecuteApicalGrowth(plant, context);
                    break;
                case GrowthType.Basal:
                case GrowthType.Rosette:
                    ExecuteBasalGrowth(plant, context);
                    break;
                case GrowthType.Vine:
                    ExecuteVineGrowth(plant, context);
                    break;
            }
        }

        private void ExecuteBasalGrowth(Plant plant, SimulationContext context)
        {
            var cost = 5;
            if (plant.Energy < cost) return;
            plant.Energy -= cost;

            var root = plant.Organs.OfType<Root>().First();
            var angle = _growthType == GrowthType.Rosette ? (context.Random.NextDouble() - 0.5) * 30 : 0;
            var rotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), angle));
            var dir = rotation.Transform(new Vector3D((context.Random.NextDouble() - 0.5) * 0.5, 1, (context.Random.NextDouble() - 0.5) * 0.5));
            dir.Normalize();
            plant.AddOrgan(new Leaf(plant, root, root.Position, dir, plant.Gene.LeafSize));
        }

        private void ExecuteApicalGrowth(Plant plant, SimulationContext context)
        {
            var cost = 10;
            if (plant.Energy < cost) return;

            var meristems = plant.Organs.OfType<Meristem>().Where(m => m.IsActive).ToList();

            foreach (var meristem in meristems)
            {
                if (plant.Energy < cost) break;

                var parentOrgan = meristem.Parent;
                if (parentOrgan == null) continue;

                plant.Energy -= cost;

                var dir = meristem.Direction;
                dir += new Vector3D((context.Random.NextDouble() - 0.5) * 0.4, _apicalDominanceFactor, (context.Random.NextDouble() - 0.5) * 0.4) * plant.Gene.ApicalDominance;
                dir.Normalize();

                var newPos = meristem.Position;
                var newEndPos = newPos + dir * plant.Gene.GrowthSpeed;

                double newThickness = (parentOrgan is Stem parentStem) ? parentStem.Thickness * 0.98 : plant.Gene.TrunkThickness * 0.8;

                var newStem = new Stem(plant, parentOrgan, newPos, dir, newThickness, newEndPos);
                plant.AddOrgan(newStem);

                meristem.Position = newEndPos;
                meristem.Direction = dir;
                meristem.Parent = newStem;

                plant.AddOrgan(new Leaf(plant, newStem, newEndPos, dir, plant.Gene.LeafSize));

                if (context.Random.NextDouble() < plant.Gene.BranchingChance)
                {
                    var axis = new Vector3D(context.Random.NextDouble() - 0.5, context.Random.NextDouble() - 0.5, context.Random.NextDouble() - 0.5);
                    if (axis.LengthSquared > 0) axis.Normalize(); else axis = new Vector3D(1, 0, 0);
                    var q = new Quaternion(axis, _branchingAngle * (context.Random.NextDouble() * 0.5 + 0.75));
                    var m = new Matrix3D();
                    m.Rotate(q);
                    var branchDir = m.Transform(dir);
                    var branchEndPos = newPos + branchDir * plant.Gene.GrowthSpeed * 0.8;
                    var branchStem = new Stem(plant, parentOrgan, newPos, branchDir, newStem.Thickness * 0.7, branchEndPos);
                    plant.AddOrgan(branchStem);
                    plant.AddOrgan(new Meristem(plant, branchStem, branchEndPos, branchDir));
                }
            }
        }

        private void ExecuteVineGrowth(Plant plant, SimulationContext context)
        {
            var cost = 8;
            if (plant.Energy < cost) return;

            var meristems = plant.Organs.OfType<Meristem>().Where(m => m.IsActive).ToList();
            if (meristems.Count == 0) return;

            plant.Energy -= cost;
            var tip = meristems.First(); // 蔓は主に一方向に伸びる

            var parentOrgan = tip.Parent;
            if (parentOrgan == null) return;

            // 地面を這うように、Y方向の成長を抑制
            var dir = tip.Direction;
            dir += new Vector3D((context.Random.NextDouble() - 0.5) * 1.5, (context.Random.NextDouble() - 0.5) * 0.2, (context.Random.NextDouble() - 0.5) * 1.5);
            dir.Normalize();

            var newPos = tip.Position;
            var newEndPos = newPos + dir * plant.Gene.GrowthSpeed;
            double newThickness = (parentOrgan is Stem parentStem) ? parentStem.Thickness * 0.99 : plant.Gene.TrunkThickness;

            var newStem = new Stem(plant, parentOrgan, newPos, dir, newThickness, newEndPos);
            plant.AddOrgan(newStem);

            tip.Position = newEndPos;
            tip.Direction = dir;
            tip.Parent = newStem;

            // 節ごとに葉や花をつける
            if (context.Random.NextDouble() < 0.5)
            {
                plant.AddOrgan(new Leaf(plant, newStem, newEndPos, dir, plant.Gene.LeafSize));
            }
        }
    }
}
