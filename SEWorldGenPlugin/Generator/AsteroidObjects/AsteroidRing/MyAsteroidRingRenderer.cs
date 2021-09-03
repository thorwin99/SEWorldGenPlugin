using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.ObjectBuilders;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing
{
    /// <summary>
    /// A class to visually render an asteroid ring.
    /// </summary>
    public class MyAsteroidRingRenderer : IMyStarSystemDesignerRenderObject
    {
        /// <summary>
        /// The instance to render
        /// </summary>
        private MySystemAsteroids m_instance;

        /// <summary>
        /// The data to render
        /// </summary>
        private MyAsteroidRingData m_data;

        /// <summary>
        /// A hollow cylinder used to represent the ring
        /// </summary>
        private RenderHollowCylinder m_render;

        public MyAsteroidRingRenderer(MySystemAsteroids instance, MyAsteroidRingData data)
        {
            m_instance = instance;
            m_data = data;

            if(data == null)
            {
                m_data = new MyAsteroidRingData();
            }
            var shape = MyAsteroidObjectShapeRing.CreateFromRingItem(data);

            m_render = new RenderHollowCylinder(shape.worldMatrix, (float)(shape.radius + shape.width), (float)shape.radius, (float)shape.height, Color.Green.ToVector4(), (float)shape.radius / 200f);
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
