
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    public class RenderCylinder : IRenderObject
    {
        private MatrixD m_worldMatrix;
        private float m_radius;
        private float m_height;
        private Vector4 m_color;

        public RenderCylinder(MatrixD worldMatrix, float radius, float height, Vector4 color)
        {
            this.m_worldMatrix = worldMatrix;
            this.m_radius = radius;
            this.m_height = height;
            this.m_color = color;
        }

        public void Draw()
        {
            MySimpleObjectDraw.DrawTransparentCylinder(ref m_worldMatrix, m_radius, m_radius, m_height, ref m_color, false, 100, 0.01f * m_radius, MyStringId.GetOrCompute("GizmoDrawLine"));
        }

        public override string ToString()
        {
            return "P:" + m_worldMatrix.ToString() + " R:" + m_radius + " H:" + m_height;
        }
    }
}
