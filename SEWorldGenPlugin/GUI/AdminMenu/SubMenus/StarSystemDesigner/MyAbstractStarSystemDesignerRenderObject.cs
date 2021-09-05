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

        /// <summary>
        /// Whether this object is currently in focus in the Star System designer
        /// </summary>
        public bool IsFocused
        {
            get;
            private set;
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

        /// <summary>
        /// Enables or disables focus for this object
        /// </summary>
        /// <param name="isFocused">Whether it is focused or not</param>
        public void SetFocus(bool isFocused)
        {
            IsFocused = isFocused;
        }

        public abstract void Draw();
    }
}
