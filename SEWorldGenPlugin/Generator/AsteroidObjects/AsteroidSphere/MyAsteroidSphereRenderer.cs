using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.ObjectBuilders;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// A class to visually render an asteroid sphere.
    /// </summary>
    public class MyAsteroidSphereRenderer : MyAbstractStarSystemDesignerRenderObject
    {
        /// <summary>
        /// The data of the asteroid sphere to render
        /// </summary>
        private MyAsteroidSphereData m_data;

        /// <summary>
        /// A hollow sphere used to render the asteroid sphere
        /// </summary>
        private RenderHollowSphere m_render;

        public MyAsteroidSphereRenderer(MySystemAsteroids instance, MyAsteroidSphereData data) : base(instance)
        {
            m_data = data;
            m_render = new RenderHollowSphere(instance.CenterPosition, (float)data.InnerRadius, (float)data.OuterRadius, Color.Green, (float)data.OuterRadius / 200f);
        }

        public override void Draw()
        {
            m_render.Draw();
        }

        public override double GetObjectRenderSize()
        {
            return 0;
        }
    }
}
