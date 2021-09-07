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
    public class MyAsteroidRingRenderer : MyAbstractStarSystemDesignerRenderObject
    {
        /// <summary>
        /// The data to render
        /// </summary>
        private MyAsteroidRingData m_data;

        /// <summary>
        /// A hollow cylinder used to represent the ring
        /// </summary>
        private RenderHollowCylinder m_render;

        public MyAsteroidRingRenderer(MySystemAsteroids instance, MyAsteroidRingData data) : base(instance)
        {
            m_data = data;

            if(data == null)
            {
                m_data = new MyAsteroidRingData();
            }
            var shape = MyAsteroidObjectShapeRing.CreateFromRingItem(data);

            m_render = new RenderHollowCylinder(shape.worldMatrix, (float)(shape.radius + shape.width), (float)shape.radius, (float)shape.height, Color.Brown.ToVector4(), (float)shape.radius / 100f);
        }

        public override void Draw()
        {
            m_render.Color = IsFocused ? Color.Green.ToVector4() : Color.Brown.ToVector4();

            var shape = MyAsteroidObjectShapeRing.CreateFromRingItem(m_data);

            m_render.WorldMatrix = shape.worldMatrix;
            m_render.Radius = (float)(shape.radius + shape.width);
            m_render.InnerRadius = (float)shape.radius;
            m_render.Height = (float)shape.height;
            m_render.LineThickness = m_render.Radius / 200f;

            m_render.Draw();
        }

        public override double GetObjectRenderSize(ZoomLevel level)
        {

            if(level == ZoomLevel.OBJECT_SYSTEM)
            {
                return (m_data.Radius + m_data.Width) * 2f;
            }

            return m_data.Radius + m_data.Width;
        }
    }
}
