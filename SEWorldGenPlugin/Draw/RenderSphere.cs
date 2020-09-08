using VRage.Game;
using VRage.Utils;
using VRageMath;
using VRageRender;

namespace SEWorldGenPlugin.Draw
{
    public class RenderSphere : IRenderObject
    {
        private Vector3D m_position;
        private float m_radius;
        private Color m_color;

        public RenderSphere(Vector3D position, float radius, Color color)
        {
            m_position = position;
            m_radius = radius;
            m_color = color;
        }

        public void Draw()
        {
            MatrixD wm = MatrixD.CreateWorld(m_position);
            MySimpleObjectDraw.DrawTransparentSphere(ref wm, m_radius, ref m_color, MySimpleObjectRasterizer.SolidAndWireframe, 100, lineMaterial: MyStringId.GetOrCompute("GizmoDrawLine"), lineThickness: 1000);
        }
    }
}
