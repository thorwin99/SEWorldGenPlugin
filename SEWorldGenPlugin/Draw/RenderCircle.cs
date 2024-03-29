﻿using System;
using VRageMath;
using VRageRender;

namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Simple class that can render a circle into the gameworld.
    /// </summary>
    public class RenderCircle : IRenderObject
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
        public RenderCircle(MatrixD worldMatrix, float radius, Vector4 color)
        {
            WorldMatrix = worldMatrix;
            Radius = radius;
            Color = color;
        }

        public void Draw()
        {
            Vector3D prevVertex = Vector3D.Zero;
            Vector3D vertex;

            for(int i = 0; i <= 360; i += 2)
            {
                vertex = new Vector3D(Radius * Math.Cos(MathHelper.ToRadians(i)), 0, Radius * Math.Sin(MathHelper.ToRadians(i)));
                vertex = Vector3D.Transform(vertex, WorldMatrix);

                if (i == 0)
                {
                    prevVertex = vertex;
                    continue;
                }

                MyRenderProxy.DebugDrawLine3D(prevVertex, vertex, Color, Color, true, false);
                prevVertex = vertex;
            }
        }
    }
}
