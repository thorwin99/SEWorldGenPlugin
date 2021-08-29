using System;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Simple class that can render a circle into the gameworld.
    /// </summary>
    public class RenderCircle : IRenderObject
    {
        private MatrixD m_worldMatrix;
        private float m_radius;
        private float m_lineStrength;
        private Vector4 m_color;

        /// <summary>
        /// Constructs a new renderable circle from a given world matrix, with
        /// the given radius, color and line strength
        /// </summary>
        /// <param name="worldMatrix">The world matrix specifying the location, rotation and scale of the cylinder</param>
        /// <param name="radius">The radius of the cylinder</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="lineStrength">The line strength of the circle</param>
        public RenderCircle(MatrixD worldMatrix, float radius, Vector4 color, float lineStrength = 1000f)
        {
            m_worldMatrix = worldMatrix;
            m_radius = radius;
            m_color = color;
            m_lineStrength = lineStrength;
        }

        public void Draw()
        {
            Vector3D prevVertex = Vector3D.Zero;
            Vector3D vertex;

            for(int i = 0; i <= 360; i++)
            {
                vertex = new Vector3D(m_radius * Math.Cos(MathHelper.ToRadians(i)), 0, m_radius * Math.Sin(MathHelper.ToRadians(i)));
                vertex = Vector3D.Transform(vertex, m_worldMatrix);

                if (i == 0)
                {
                    prevVertex = vertex;
                    continue;
                }

                MySimpleObjectDraw.DrawLine(prevVertex, vertex, MyStringId.GetOrCompute("GizmoDrawLine"), ref m_color, m_lineStrength);
            }
        }
    }
}
