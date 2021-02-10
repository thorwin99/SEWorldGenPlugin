using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjectShapes
{
    public interface IMyAsteroidObjectShape
    {
        /// <summary>
        /// Checks, if the given point is inside or outside of the Asteroid Object Shape
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <returns>A ContainmentType stating, if the point is inside, or disjoint</returns>
        ContainmentType Contains(Vector3D point);

        /// <summary>
        /// Tries to get the closest point in / on the asteroid object shape to the given point.
        /// </summary>
        /// <param name="point">The point in space</param>
        /// <returns>The closest point in this shape</returns>
        Vector3D GetClosestPoint(Vector3D point);

        /// <summary>
        /// Returns a point inside of the shape area
        /// </summary>
        /// <returns>A random point in the shape</returns>
        Vector3D GetPointInShape();
    }
}
