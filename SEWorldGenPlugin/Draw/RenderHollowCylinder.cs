using System;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Simple class that can render a hollow cylinder into the gameworld.
    /// </summary>
    public class RenderHollowCylinder : AbstractWireframeRenderObject
    {
        public MatrixD WorldMatrix;
        public float Radius;
        public float Height;
        public Vector4 Color;
        public float InnerRadius;

        /// <summary>
        /// Constructs a new renderable hollow cylinder from a given world matrix, with
        /// the given radius, inner radius and height, aswell as a given color.
        /// </summary>
        /// <param name="worldMatrix">The world matrix specifying the location, rotation and scale of the cylinder</param>
        /// <param name="radius">The radius of the cylinder</param>
        /// <param name="innerRadius">The inner radius of the cylinder, which defines the hollow part of the cylinder</param>
        /// <param name="height">The height of the cylinder</param>
        /// <param name="color">The color of the cylinder</param>
        public RenderHollowCylinder(MatrixD worldMatrix, float radius, float innerRadius, float height, Vector4 color, float lineThickness = 1000f) : base(lineThickness)
        {
            WorldMatrix = worldMatrix;
            Radius = radius;
            Height = height;
            Color = color;
            InnerRadius = innerRadius;
            LineThickness = lineThickness;
        }

        /// <summary>
        /// Draws the hollow cylinder into the gameworld by creating a wireframe mesh of the cylinder.
        /// </summary>
        public override void Draw()
        {
            Vector3D outVertexUp = Vector3D.Zero;
            Vector3D inVertexUp = Vector3D.Zero;
            Vector3D outVertexLow = Vector3D.Zero;
            Vector3D inVertexLow = Vector3D.Zero;

            Vector3D outPrevUp = Vector3D.Zero;
            Vector3D inPrevUp = Vector3D.Zero;
            Vector3D outPrevLow = Vector3D.Zero;
            Vector3D inPrevLow = Vector3D.Zero;

            for(int i = 0; i <= 360; i += 2)
            {
                outVertexUp.X = (float)(Radius * Math.Cos(MathHelper.ToRadians(i)));
                outVertexUp.Y = Height;
                outVertexUp.Z = (float)(Radius * Math.Sin(MathHelper.ToRadians(i)));

                outVertexLow.X = (float)(Radius * Math.Cos(MathHelper.ToRadians(i)));
                outVertexLow.Y = -Height;
                outVertexLow.Z = (float)(Radius * Math.Sin(MathHelper.ToRadians(i)));

                inVertexUp.X = (float)(InnerRadius * Math.Cos(MathHelper.ToRadians(i)));
                inVertexUp.Y = Height;
                inVertexUp.Z = (float)(InnerRadius * Math.Sin(MathHelper.ToRadians(i)));

                inVertexLow.X = (float)(InnerRadius * Math.Cos(MathHelper.ToRadians(i)));
                inVertexLow.Y = -Height;
                inVertexLow.Z = (float)(InnerRadius * Math.Sin(MathHelper.ToRadians(i)));

                outVertexUp = Vector3D.Transform(outVertexUp, WorldMatrix);
                outVertexLow = Vector3D.Transform(outVertexLow, WorldMatrix);
                inVertexUp = Vector3D.Transform(inVertexUp, WorldMatrix);
                inVertexLow = Vector3D.Transform(inVertexLow, WorldMatrix);

                if (i > 0)
                {
                    MySimpleObjectDraw.DrawLine(outPrevUp, outVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);
                    MySimpleObjectDraw.DrawLine(outPrevLow, outVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);
                    MySimpleObjectDraw.DrawLine(inPrevUp, inVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);
                    MySimpleObjectDraw.DrawLine(inPrevLow, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);
                }

                MySimpleObjectDraw.DrawLine(outVertexUp, inVertexUp, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);
                MySimpleObjectDraw.DrawLine(outVertexLow, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);
                //MySimpleObjectDraw.DrawLine(outVertexUp, outVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);
                //MySimpleObjectDraw.DrawLine(inVertexUp, inVertexLow, MyStringId.GetOrCompute("GizmoDrawLine"), ref Color, LineThickness);

                outPrevUp = outVertexUp;
                outPrevLow = outVertexLow;
                inPrevUp = inVertexUp;
                inPrevLow = inVertexLow;
            }
        }
    }
}
