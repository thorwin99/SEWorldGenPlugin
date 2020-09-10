using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Simple class that can render a sphere into the gameworld.
    /// </summary>
    public class RenderSphere : IRenderObject
    {
        private Vector3D m_position;
        private float m_radius;
        private Color m_color;

        /// <summary>
        /// Creates a new sphere at the given position with given radius and color.
        /// </summary>
        /// <param name="position">The position of the sphere in the world space</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="color">The sphere color</param>
        public RenderSphere(Vector3D position, float radius, Color color)
        {
            m_position = position;
            m_radius = radius;
            m_color = color;
        }

        /// <summary>
        /// Draws the sphere in the game world as a solid mesh
        /// with a visible wireframe.
        /// </summary>
        public void Draw()
        {
            MatrixD wm = MatrixD.CreateWorld(m_position);
            MySimpleObjectDraw.DrawTransparentSphere(ref wm, m_radius, ref m_color, MySimpleObjectRasterizer.SolidAndWireframe, 100, lineMaterial: MyStringId.GetOrCompute("GizmoDrawLine"), lineThickness: 1000);
        }
    }
}
