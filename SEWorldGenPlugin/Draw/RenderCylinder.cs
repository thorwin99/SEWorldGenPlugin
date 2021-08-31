
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Simple class that can render a cylinder into the gameworld.
    /// </summary>
    public class RenderCylinder : IRenderObject
    {
        public MatrixD WorldMatrix;
        public float Radius;
        public float Height;
        public Vector4 Color;

        /// <summary>
        /// Constructs a new renderable cylinder from a given world matrix, with
        /// the given radius and height, aswell as a given color.
        /// </summary>
        /// <param name="worldMatrix">The world matrix specifying the location, rotation and scale of the cylinder</param>
        /// <param name="radius">The radius of the cylinder</param>
        /// <param name="height">The height of the cylinder</param>
        /// <param name="color">The color of the cylinder</param>
        public RenderCylinder(MatrixD worldMatrix, float radius, float height, Vector4 color)
        {
            this.WorldMatrix = worldMatrix;
            this.Radius = radius;
            this.Height = height;
            this.Color = color;
        }

        /// <summary>
        /// Draws the cylinder into the world as a simple, transparent object.
        /// </summary>
        public void Draw()
        {
            MySimpleObjectDraw.DrawTransparentCylinder(ref WorldMatrix, Radius, Radius, Height, ref Color, false, 100, 0.01f * Radius, MyStringId.GetOrCompute("GizmoDrawLine"));
        }

        /// <summary>
        /// A helper method to output the world matrix, radius and height of the cylinder as a readable string.
        /// </summary>
        /// <returns>A readable string representation of the world matrix, radius and height of the cylinder</returns>
        public override string ToString()
        {
            return "P:" + WorldMatrix.ToString() + " R:" + Radius + " H:" + Height;
        }
    }
}
