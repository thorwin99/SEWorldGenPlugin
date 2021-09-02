using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.ObjectBuilders;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// A class to visually render an asteroid sphere.
    /// </summary>
    public class MyAsteroidSphereRenderer : IMyStarSystemDesignerRenderObject
    {
        /// <summary>
        /// The instance of the asteroid sphere to render
        /// </summary>
        private MySystemAsteroids m_instance;

        /// <summary>
        /// The data of the asteroid sphere to render
        /// </summary>
        private MyAsteroidSphereData m_data;

        /// <summary>
        /// A hollow sphere used to render the asteroid sphere
        /// </summary>
        private RenderHollowSphere m_render;

        public MyAsteroidSphereRenderer(MySystemAsteroids instance, MyAsteroidSphereData data)
        {
            m_instance = instance;
            m_data = data;
            m_render = new RenderHollowSphere(instance.CenterPosition, (float)data.InnerRadius, (float)data.OuterRadius, Color.Green, (float)data.OuterRadius);
        }

        public void Draw()
        {
            m_render.Draw();
        }

        public void Update(double CameraFocusLength)
        {
            
        }
    }
}
