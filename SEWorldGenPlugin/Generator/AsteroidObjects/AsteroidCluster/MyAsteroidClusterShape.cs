using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using VRage.Library.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidCluster
{
    public struct MyAsteroidClusterShape : IMyAsteroidObjectShape
    {
        /// <summary>
        /// The position of the cluster
        /// </summary>
        public Vector3D Position;

        /// <summary>
        /// The largest extend of the cluster
        /// </summary>
        public double Size;

        public ContainmentType Contains(Vector3D point)
        {
            int seed = CreateSeed(point);

            double dist = Vector3D.Distance(point, Position);

            if (dist > Size) return ContainmentType.Disjoint;

            return ContainmentType.Contains;

            /*MyRandom inst = new MyRandom(seed);

            double baseProp = inst.NextDouble();

            double linProp = 0.7 - dist / Size;

            if(baseProp + linProp > 0.5) return ContainmentType.Contains;

            return ContainmentType.Disjoint;*/
        }

        public Vector3D GetClosestPoint(Vector3D point)
        {
            Vector3D v = point - Position;
            Vector3D norm = Vector3D.Normalize(v);

            return Position + norm * Size;
        }

        public Vector3D GetPointInShape(bool random = false)
        {
            return Position;
        }

        /// <summary>
        /// Calculates a seed for the random generator to determine whether at a given position a point is inside the cluster
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private int CreateSeed(Vector3D point)
        {
            return (int)(point.X * 7753 + point.Y * 4273 + point.Z * 6451);
        }
    }
}
