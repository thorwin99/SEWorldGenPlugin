using System;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    public class RenderHollowCylinder : IRenderObject
    {
        private MatrixD m_worldMatrix;
        private float m_radius;
        private float m_height;
        private Vector4 m_color;
        private float m_innerRadius;

        public RenderHollowCylinder(MatrixD worldMatrix, float radius, float innerRadius, float height, Vector4 color)
        {
            m_worldMatrix = worldMatrix;
            m_radius = radius;
            m_height = height;
            m_color = color;
            m_innerRadius = innerRadius;
        }

        public void Draw()
        {
            Vector3D outVertexUp = Vector3D.Zero;
            Vector3D inVertexUp = Vector3D.Zero;
            Vector3D outVertexLow = Vector3D.Zero;
            Vector3D inVertexLow = Vector3D.Zero;

            Vector3D outPrevUp = Vector3D.Zero;
            Vector3D inPrevUp = Vector3D.Zero;
            Vector3D outPrevLow = Vector3D.Zero;
            Vector3D inPrevLow = Vector3D.Zero;

            for(int i = 0; i <= 360; i++)
            {
                outVertexUp.X = (float)(m_radius * Math.Cos(MathHelper.ToRadians(i)));
                outVertexUp.Y = m_height;
                outVertexUp.Z = (float)(m_radius * Math.Sin(MathHelper.ToRadians(i)));

                outVertexLow.X = (float)(m_radius * Math.Cos(MathHelper.ToRadians(i)));
                outVertexLow.Y = -m_height;
                outVertexLow.Z = (float)(m_radius * Math.Sin(MathHelper.ToRadians(i)));

                inVertexUp.X = (float)(m_innerRadius * Math.Cos(MathHelper.ToRadians(i)));
                inVertexUp.Y = m_height;
                inVertexUp.Z = (float)(m_innerRadius * Math.Sin(MathHelper.ToRadians(i)));

                inVertexLow.X = (float)(m_innerRadius * Math.Cos(MathHelper.ToRadians(i)));
                inVertexLow.Y = -m_height;
                inVertexLow.Z = (float)(m_innerRadius * Math.Sin(MathHelper.ToRadians(i)));

                outVertexUp = Vector3D.Transform(outVertexUp, m_worldMatrix);
                outVertexLow = Vector3D.Transform(outVertexLow, m_worldMatrix);
                inVertexUp = Vector3D.Transform(inVertexUp, m_worldMatrix);
                inVertexLow = Vector3D.Transform(inVertexLow, m_worldMatrix);

                if (i > 0)
                {
                    MySimpleObjectDraw.DrawLine(outPrevUp, outVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_height);
                    MySimpleObjectDraw.DrawLine(outPrevLow, outVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_height);
                    MySimpleObjectDraw.DrawLine(inPrevUp, inVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_height);
                    MySimpleObjectDraw.DrawLine(inPrevLow, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_height);
                }

                MySimpleObjectDraw.DrawLine(outVertexUp, inVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_height);
                MySimpleObjectDraw.DrawLine(outVertexLow, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_height);
                MySimpleObjectDraw.DrawLine(outVertexUp, outVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_height);
                MySimpleObjectDraw.DrawLine(inVertexUp, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_height);

                outPrevUp = outVertexUp;
                outPrevLow = outVertexLow;
                inPrevUp = inVertexUp;
                inPrevLow = inVertexLow;
            }
        }
    }
}
