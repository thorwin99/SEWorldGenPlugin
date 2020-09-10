
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
        private MatrixD m_worldMatrix;
        private float m_radius;
        private float m_height;
        private Vector4 m_color;

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
            this.m_worldMatrix = worldMatrix;
            this.m_radius = radius;
            this.m_height = height;
            this.m_color = color;
        }

        /// <summary>
        /// Draws the cylinder into the world as a simple, transparent object.
        /// </summary>
        public void Draw()
        {
            MySimpleObjectDraw.DrawTransparentCylinder(ref m_worldMatrix, m_radius, m_radius, m_height, ref m_color, false, 100, 0.01f * m_radius, MyStringId.GetOrCompute("GizmoDrawLine"));
        }

        /// <summary>
        /// A helper method to output the world matrix, radius and height of the cylinder as a readable string.
        /// </summary>
        /// <returns>A readable string representation of the world matrix, radius and height of the cylinder</returns>
        public override string ToString()
        {
            return "P:" + m_worldMatrix.ToString() + " R:" + m_radius + " H:" + m_height;
        }
    }
}
