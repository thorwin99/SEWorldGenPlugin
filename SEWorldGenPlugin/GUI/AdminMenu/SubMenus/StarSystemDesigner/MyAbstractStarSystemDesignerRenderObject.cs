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
        /// Returns the size of the focused render object for the given zoom level in world units.
        /// </summary>
        /// <returns>The size of the object in the game world</returns>
        /// <param name="level">The zoom level the object should return its render size for.</param>
        public abstract double GetObjectRenderSize(ZoomLevel level);

        public abstract void Draw();
    }
}
