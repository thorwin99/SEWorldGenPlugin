using SEWorldGenPlugin.SaveItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace SEWorldGenPlugin.Generator.Asteroids
{
    public struct AsteroidRingShape
    {
        public Vector3D center;
        public int radius;
        public int width;
        public int height;
        public Vector3D rotation;

        public static AsteroidRingShape CreateFromRingItem(PlanetRingItem ring)
        {
            AsteroidRingShape shape = new AsteroidRingShape();
            shape.center = ring.Center;
            shape.radius = ring.Radius;
            shape.width = ring.Width;
            shape.height = ring.Height;
            shape.rotation = new Vector3D(0, 1, 0);

            double angle = 2 * Math.PI / 360 * ring.AngleDegrees;
            shape.rotation.X = Math.Cos(angle);
            shape.rotation.Z = Math.Sin(angle);

            return shape;
        }

        public ContainmentType Contains(Vector3D point)
        {
            Vector3D relPosition = Vector3D.Subtract(point, center);
            Vector3D planeNormal = new Vector3D(-rotation.Z, 0, rotation.X);
            Vector3D horVector = Vector3D.ProjectOnPlane(ref relPosition, ref planeNormal);
            Vector3D vertVector = Vector3D.Subtract(relPosition, horVector);

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
            return Math.Sin((rad - radius) * Math.PI / width) * Math.Sin((rad - radius) * Math.PI / width) * 0.5 * height + 1;//Plus one to make asteroids on edges possible
        }

    }
}
