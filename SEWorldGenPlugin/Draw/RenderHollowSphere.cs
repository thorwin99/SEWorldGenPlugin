using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Draw
{
    public class RenderHollowSphere : AbstractWireframeRenderObject
    {
        public Vector3D Position;
        public float InnerRadius;
        public float OuterRadius;
        public Color Color;
        public Color InnerColor;

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
            InnerColor = Color.Red;
            LineThickness = lineThickness;
        }

        /// <summary>
        /// Draws the sphere in the game world as a solid mesh
        /// with a visible wireframe.
        /// </summary>
        public override void Draw()
        {
            MatrixD wm = MatrixD.CreateWorld(Position);
            MySimpleObjectDraw.DrawTransparentSphere(ref wm, InnerRadius, ref InnerColor, MySimpleObjectRasterizer.Wireframe, 50, lineMaterial: MyStringId.GetOrCompute("GizmoDrawLine"), lineThickness: LineThickness);
            MySimpleObjectDraw.DrawTransparentSphere(ref wm, OuterRadius, ref Color, MySimpleObjectRasterizer.Wireframe, 150, lineMaterial: MyStringId.GetOrCompute("GizmoDrawLine"), lineThickness: LineThickness);
        }
    }
}
