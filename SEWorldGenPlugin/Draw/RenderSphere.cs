using VRageMath;
using VRageRender;

namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Simple class that can render a sphere into the gameworld.
    /// </summary>
    public class RenderSphere : AbstractWireframeRenderObject
    {
        public Vector3D Position;
        public float Radius;
        public Color Color;

        /// <summary>
        /// Creates a new sphere at the given position with given radius and color.
        /// </summary>
        /// <param name="position">The position of the sphere in the world space</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="color">The sphere color</param>
        public RenderSphere(Vector3D position, float radius, Color color) : base(1000)
        {
            Position = position;
            Radius = radius;
            Color = color;
        }

        /// <summary>
        /// Draws the sphere in the game world as a solid mesh
        /// with a visible wireframe.
        /// </summary>
        public override void Draw()
        {
            MyRenderProxy.DebugDrawSphere(Position, Radius, Color, 0.75f, true, true, true, false);
        }
    }
}
