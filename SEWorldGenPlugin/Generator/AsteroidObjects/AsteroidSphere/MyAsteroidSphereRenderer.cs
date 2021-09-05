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
            m_render = new RenderHollowSphere(instance.CenterPosition, (float)data.InnerRadius, (float)data.OuterRadius, Color.Brown, (float)data.OuterRadius / 200f);
        }

        public override void Draw()
        {
            m_render.Color = IsFocused ? Color.Green.ToVector4() : Color.Brown.ToVector4();
            m_render.Draw();
        }

        public override double GetObjectRenderSize(ZoomLevel level)
        {
            if(level == ZoomLevel.OBJECT_SYSTEM)
            {
                return m_data.OuterRadius * 2;
            }

            return m_data.OuterRadius;
        }
    }
}
