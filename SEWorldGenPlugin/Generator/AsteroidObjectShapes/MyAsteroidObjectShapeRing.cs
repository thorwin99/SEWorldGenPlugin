using SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing;
using System;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjectShapes
{
    /// <summary>
    /// A struct that represents the shape of an asteroid ring,
    /// and contains a method to check relations of points to the ring.
    /// </summary>
    public struct MyAsteroidObjectShapeRing : IMyAsteroidObjectShape
    {
        public Vector3D center;
        public double radius;
        public double width;
        public double height;
        public Vector3D rotation;
        public Vector3D normal;
        public MatrixD worldMatrix;

        /// <summary>
        /// Creates a bew asteroid ring shape from a MyPlanetRingItem.
        /// A ring has a radius, a rotation and a width.
        /// </summary>
        /// <param name="ring">The ring to create a shape representation for.</param>
        /// <returns>An AsteroidRingShape representing the given ring in worldspace</returns>
        public static MyAsteroidObjectShapeRing CreateFromRingItem(MyAsteroidRingData ring)
        {
            MyAsteroidObjectShapeRing shape = new MyAsteroidObjectShapeRing();
            shape.center = ring.CenterPosition;
            shape.radius = ring.Radius;
            shape.width = ring.Width;
            shape.height = ring.Height;
            shape.rotation = new Vector3D(1, 0, 0);
            shape.normal = new Vector3D(0, 0, 1);

            double angleZ = 2 * Math.PI / 360 * (ring.AngleDegrees.Z);
            double angleY = 2 * Math.PI / 360 * (ring.AngleDegrees.Y);
            double angleX = 2 * Math.PI / 360 * (ring.AngleDegrees.X);

            //shape.rotation.X = Math.Cos(angleZ);
            //shape.rotation.Y = Math.Sin(angleZ); 
            MatrixD mx = MatrixD.CreateRotationX(angleX);
            MatrixD my = MatrixD.CreateRotationY(angleY);
            MatrixD mz = MatrixD.CreateRotationZ(angleZ);

            MatrixD.Multiply(ref mx, ref my, out MatrixD mxy);
            MatrixD.Multiply(ref mxy, ref mz, out MatrixD mxyz);

            Vector3D.Rotate(ref shape.rotation, ref mxyz, out Vector3D newRotation);
            Vector3D.Rotate(ref shape.normal, ref mxyz, out Vector3D newNormal);
            shape.rotation = Vector3D.Normalize(newRotation);
            shape.normal = Vector3D.Normalize(newNormal);

            shape.worldMatrix = MatrixD.CreateWorld(shape.center, shape.rotation, shape.normal);

            return shape;
        }

        /// <summary>
        /// Checks whether a given point is inside the asteroid rings shape or outside.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>An ContainmentType enum</returns>
        public ContainmentType Contains(Vector3D point)
        {
            Vector3D relPosition = Vector3D.Subtract(point, center);
            Vector3D planeNormal = normal;
            Vector3D horVector = Vector3D.ProjectOnPlane(ref relPosition, ref planeNormal);
            Vector3D vertVector = Vector3D.Subtract(relPosition, horVector);

            double length = horVector.Length();

            if (length > radius + width || length < radius) return ContainmentType.Disjoint;

            double vertLength = vertVector.Length();
            double ringHeight = GetHeightAtRad(length);

            if (vertLength <= ringHeight) return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Gets the Closest point at the center of the rings torus to the given position.
        /// </summary>
        /// <param name="position">The position to get the closest point to</param>
        /// <returns>The closest position to position inside the ring</returns>
        public Vector3D ClosestPointAtRingCenter(Vector3D position)
        {
            if (Contains(position) == ContainmentType.Contains) return position;
            Vector3D relPosition = Vector3D.Subtract(position, center);
            Vector3D planeNormal = normal;
            Vector3D horVector = Vector3D.ProjectOnPlane(ref relPosition, ref planeNormal);
            double length = horVector.Length();

            if (length < radius + width && length > radius) return Vector3D.Add(horVector, center);
            Vector3D horVectorNorm = Vector3D.Normalize(horVector);

            return Vector3D.Add(Vector3D.Multiply(horVectorNorm, radius + width / 2), center);
        }

        /// <summary>
        /// Will get a location in the ring at the given angle of the ring
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <returns>A location in the ring at the given angle</returns>
        public Vector3D LocationInRing(int angle)
        {
            Vector3D pos = Vector3D.Zero;
            pos.X = (radius + width / 2) * Math.Cos(MathHelper.ToRadians(angle));
            pos.Y = 0;
            pos.Z = (radius + width / 2) * Math.Sin(MathHelper.ToRadians(angle));
            return Vector3D.Transform(pos, worldMatrix);
        }

        /// <summary>
        /// Gets the height of the ring at the given radius
        /// </summary>
        /// <param name="rad">The radius</param>
        /// <returns>The height</returns>
        private double GetHeightAtRad(double rad)
        {
            if (rad < radius || rad > radius + width) throw new ArgumentOutOfRangeException("The radius " + rad + " has to be less than " + (radius + width) + " and larger than " + radius);
            return Math.Sin((rad - radius) * Math.PI / width) * Math.Sin((rad - radius) * Math.PI / width) * 0.5 * height + 100;//Plus 100 to make asteroids on edges possible
        }

        public Vector3D GetClosestPoint(Vector3D point)
        {
            //Is not really the closest point
            return ClosestPointAtRingCenter(point);
        }

        public Vector3D GetPointInShape()
        {
            Vector3D pos = Vector3D.Zero;
            pos.X = (radius + width / 2) * Math.Cos(MathHelper.ToRadians(0));
            pos.Y = 0;
            pos.Z = (radius + width / 2) * Math.Sin(MathHelper.ToRadians(0));
            return Vector3D.Transform(pos, worldMatrix);
        }
    }
}
