using System;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Simple class that can render a hollow cylinder into the gameworld.
    /// </summary>
    public class RenderHollowCylinder : IRenderObject
    {
        private MatrixD m_worldMatrix;
        private float m_radius;
        private float m_height;
        private Vector4 m_color;
        private float m_innerRadius;
        private float m_thickness;

        /// <summary>
        /// Constructs a new renderable hollow cylinder from a given world matrix, with
        /// the given radius, inner radius and height, aswell as a given color.
        /// </summary>
        /// <param name="worldMatrix">The world matrix specifying the location, rotation and scale of the cylinder</param>
        /// <param name="radius">The radius of the cylinder</param>
        /// <param name="innerRadius">The inner radius of the cylinder, which defines the hollow part of the cylinder</param>
        /// <param name="height">The height of the cylinder</param>
        /// <param name="color">The color of the cylinder</param>
        public RenderHollowCylinder(MatrixD worldMatrix, float radius, float innerRadius, float height, Vector4 color, float lineThickness = 1000f)
        {
            m_worldMatrix = worldMatrix;
            m_radius = radius;
            m_height = height;
            m_color = color;
            m_innerRadius = innerRadius;
            m_thickness = lineThickness;
        }

        /// <summary>
        /// Draws the hollow cylinder into the gameworld by creating a wireframe mesh of the cylinder.
        /// </summary>
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
                    MySimpleObjectDraw.DrawLine(outPrevUp, outVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_thickness);
                    MySimpleObjectDraw.DrawLine(outPrevLow, outVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_thickness);
                    MySimpleObjectDraw.DrawLine(inPrevUp, inVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_thickness);
                    MySimpleObjectDraw.DrawLine(inPrevLow, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_thickness);
                }

                MySimpleObjectDraw.DrawLine(outVertexUp, inVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_thickness);
                MySimpleObjectDraw.DrawLine(outVertexLow, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_thickness);
                MySimpleObjectDraw.DrawLine(outVertexUp, outVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_thickness);
                MySimpleObjectDraw.DrawLine(inVertexUp, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_thickness);

                outPrevUp = outVertexUp;
                outPrevLow = outVertexLow;
                inPrevUp = inVertexUp;
                inPrevLow = inVertexLow;
            }
        }
    }
}
