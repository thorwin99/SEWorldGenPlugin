using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.ObjectBuilders;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    public abstract class MyAbstractStarSystemDesignerRenderObject : IRenderObject
    {
        /// <summary>
        /// The to be rendered system object instance
        /// </summary>
        public MySystemObject RenderObject
        {
            get;
            protected set;
        }

        public MyAbstractStarSystemDesignerRenderObject(MySystemObject obj)
        {
            RenderObject = obj;
        }

        /// <summary>
        /// Returns the size of the focused render object.
        /// In case of a planet, it should return the size of the planet system, the planets and its moons.
        /// In case of a ring or belt its the radius + width of the belt / ring.
        /// </summary>
        /// <returns>The size of the object in the game world</returns>
        public abstract double GetObjectRenderSize();

        public abstract void Draw();
    }
}
