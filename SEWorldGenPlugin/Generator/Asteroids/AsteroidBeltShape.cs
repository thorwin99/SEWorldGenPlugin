using SEWorldGenPlugin.ObjectBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace SEWorldGenPlugin.Generator.Asteroids
{
    public struct AsteroidBeltShape
    {
        public Vector3D center;
        public long radius;
        public int width;
        public int height;

        public static AsteroidBeltShape CreateFromRingItem(MySystemBeltItem ring)
        {
            AsteroidBeltShape shape = new AsteroidBeltShape();
            shape.center = Vector3D.Zero;
            shape.radius = ring.Radius;
            shape.width = ring.Width;
            shape.height = ring.Height;

            return shape;
        }

        public ContainmentType Contains(Vector3D point)
        {
            Vector3D vertVector = new Vector3D(0, 0, point.Z);
            Vector3D horVector = new Vector3D(point.X, point.Y, 0);

            double length = horVector.Length();

            if (length > radius + width || length < radius) return ContainmentType.Disjoint;

            double vertLength = vertVector.Length();
            double ringHeight = GetHeightAtRad(length);

            if (vertLength <= ringHeight) return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }

        private double GetHeightAtRad(double rad)
        {
            if (rad < radius || rad > radius + width) throw new ArgumentOutOfRangeException("The radius " + rad + " has to be less than " + (radius + width) + " and larger than " + radius);
            return Math.Sin((rad - radius) * Math.PI / width) * (height - 100) + 100;//Plus 100 to make asteroids on edges possible
        }
    }
}
