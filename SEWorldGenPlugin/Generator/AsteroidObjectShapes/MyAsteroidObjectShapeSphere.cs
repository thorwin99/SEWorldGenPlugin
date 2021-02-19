using SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjectShapes
{
    /// <summary>
    /// A shape for the hollow asteroid sphere object
    /// </summary>
    public struct MyAsteroidObjectShapeSphere : IMyAsteroidObjectShape
    {
        /// <summary>
        /// Center of the shape
        /// </summary>
        public Vector3D Center;

        /// <summary>
        /// Inner radius of the hollow sphere
        /// </summary>
        public double InnerRadius;

        /// <summary>
        /// Outer radius of the hollow sphere
        /// </summary>
        public double OuterRadius;

        public static MyAsteroidObjectShapeSphere CreateFromAsteroidSphereData(MyAsteroidSphereData data)
        {
            MyAsteroidObjectShapeSphere shape = new MyAsteroidObjectShapeSphere();

            shape.Center = data.Center;
            shape.InnerRadius = data.InnerRadius;
            shape.OuterRadius = data.OuterRadius;

            return shape;
        }

        public ContainmentType Contains(Vector3D point)
        {
            BoundingSphereD outer = new BoundingSphereD(Center, OuterRadius);
            BoundingSphereD inner = new BoundingSphereD(Center, InnerRadius);

            if (inner.Contains(point) == ContainmentType.Contains) return ContainmentType.Disjoint;

            if (outer.Contains(point) != ContainmentType.Disjoint) return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }

        public Vector3D GetClosestPoint(Vector3D point)
        {
            Vector3D direction = point - Center;
            double lenght = direction.Length();
            direction.Normalize();

            if (lenght <= InnerRadius)
            {
                return direction * InnerRadius + Center;
            }
            else if(lenght <= OuterRadius && lenght >= InnerRadius)
            {
                return point;
            }
            else
            {
                return direction * OuterRadius + Center;
            }


        }

        public Vector3D GetPointInShape()
        {
            return Center + new Vector3D(InnerRadius + ((OuterRadius - InnerRadius) / 2), 0, 0);
        }
    }
}
