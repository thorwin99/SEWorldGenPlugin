using System;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Simple class that can render a circle into the gameworld.
    /// </summary>
    public class RenderCircle : AbstractWireframeRenderObject
    {
        public MatrixD WorldMatrix;
        public float Radius;
        public Vector4 Color;

        /// <summary>
        /// Constructs a new renderable circle from a given world matrix, with
        /// the given radius, color and line strength
        /// </summary>
        /// <param name="worldMatrix">The world matrix specifying the location, rotation and scale of the cylinder</param>
        /// <param name="radius">The radius of the cylinder</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="lineStrength">The line strength of the circle</param>
        public RenderCircle(MatrixD worldMatrix, float radius, Vector4 color, float lineStrength = 1000f) : base(lineStrength)
        {
            WorldMatrix = worldMatrix;
            Radius = radius;
            Color = color;
        }

        public override void Draw()
        {
            Vector3D prevVertex = Vector3D.Zero;
            Vector3D vertex;

            for(int i = 0; i <= 360; i++)
            {
                vertex = new Vector3D(Radius * Math.Cos(MathHelper.ToRadians(i)), 0, Radius * Math.Sin(MathHelper.ToRadians(i)));
                vertex = Vector3D.Transform(vertex, WorldMatrix);

                if (i == 0)
                {
                    prevVertex = vertex;
                    continue;
                }

                MySimpleObjectDraw.DrawLine(prevVertex, vertex, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);
            }
        }
    }
}
