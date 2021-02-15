using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    public class RenderHollowSphere : IRenderObject
    {
        private Vector3D m_position;
        private float m_innerRadius;
        private float m_outerRadius;
        private Color m_color;
        private Color m_innerColor;

        /// <summary>
        /// Creates a new sphere at the given position with given radius and color.
        /// </summary>
        /// <param name="position">The position of the sphere in the world space</param>
        /// <param name="innerRadius">The inner radius of the sphere</param>
        /// <param name="outerRadius">The outer radius of the sphere</param>
        /// <param name="color">The sphere color</param>
        public RenderHollowSphere(Vector3D position, float innerRadius, float outerRadius, Color color)
        {
            m_position = position;
            m_innerRadius = innerRadius;
            m_outerRadius = outerRadius;
            m_color = color;
            m_innerColor = m_color.ToGray();
        }

        /// <summary>
        /// Draws the sphere in the game world as a solid mesh
        /// with a visible wireframe.
        /// </summary>
        public void Draw()
        {
            MatrixD wm = MatrixD.CreateWorld(m_position);
            MySimpleObjectDraw.DrawTransparentSphere(ref wm, m_innerRadius, ref m_innerColor, MySimpleObjectRasterizer.Wireframe, 100, lineMaterial: MyStringId.GetOrCompute("GizmoDrawLine"), lineThickness: 1000);
            MySimpleObjectDraw.DrawTransparentSphere(ref wm, m_outerRadius, ref m_color, MySimpleObjectRasterizer.Wireframe, 100, lineMaterial: MyStringId.GetOrCompute("GizmoDrawLine"), lineThickness: 1000);
        }
    }
}
