using VRage.Game;
using VRage.Utils;
using VRageMath;
using VRageRender;

namespace SEWorldGenPlugin.Draw
{
    public class RenderHollowSphere : AbstractWireframeRenderObject
    {
        public Vector3D Position;
        public float InnerRadius;
        public float OuterRadius;
        public Color Color;

        /// <summary>
        /// Creates a new sphere at the given position with given radius and color.
        /// </summary>
        /// <param name="position">The position of the sphere in the world space</param>
        /// <param name="innerRadius">The inner radius of the sphere</param>
        /// <param name="outerRadius">The outer radius of the sphere</param>
        /// <param name="color">The sphere color</param>
        public RenderHollowSphere(Vector3D position, float innerRadius, float outerRadius, Color color, float lineThickness) : base(lineThickness)
        {
            Position = position;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            Color = color;
            LineThickness = lineThickness;
        }

        /// <summary>
        /// Draws the sphere in the game world as a solid mesh
        /// with a visible wireframe.
        /// </summary>
        public override void Draw()
        {
            MyRenderProxy.DebugDrawSphere(Position, InnerRadius, Color, 0.5f, true, false, true, false);
            MyRenderProxy.DebugDrawSphere(Position, OuterRadius, Color, 0.5f, true, false, true, false);
        }
    }
}
