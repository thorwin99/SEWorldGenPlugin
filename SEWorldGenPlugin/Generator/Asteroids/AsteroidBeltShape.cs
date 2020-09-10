using SEWorldGenPlugin.ObjectBuilders;
using System;
using VRageMath;

namespace SEWorldGenPlugin.Generator.Asteroids
{
    /// <summary>
    /// A struct that represents the shape of an asteroid belt
    /// and contains a method to check, wether a given point is in
    /// the asteroid belt shape.
    /// </summary>
    public struct AsteroidBeltShape
    {
        public Vector3D center;
        public double radius;
        public int width;
        public int height;


        /// <summary>
        /// Creates a new asteroid belt shape from an MySystemBeltItem
        /// </summary>
        /// <param name="belt">The belt it should create the shape for.</param>
        /// <returns>The shape representation of the given belt</returns>
        public static AsteroidBeltShape CreateFromBeltItem(MySystemBeltItem belt)
        {
            AsteroidBeltShape shape = new AsteroidBeltShape();
            shape.center = Vector3D.Zero;
            shape.radius = belt.Radius;
            shape.width = belt.Width;
            shape.height = belt.Height;

            return shape;
        }

        /// <summary>
        /// Checks wether a given point is inside the asteroid belts shape.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>A ContainmentType enum</returns>
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

        /// <summary>
        /// Gets the height of the belt at the given radius. Has to be inside the belts shape.
        /// </summary>
        /// <param name="rad">The radius, to get the height from</param>
        /// <returns>The height of the belt at the given radius</returns>
        private double GetHeightAtRad(double rad)
        {
            if (rad < radius || rad > radius + width) throw new ArgumentOutOfRangeException("The radius " + rad + " has to be less than " + (radius + width) + " and larger than " + radius);
            return Math.Sin((rad - radius) * Math.PI / width) * (height - 100) + 100;//Plus 100 to make asteroids on edges possible
        }
    }
}
