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
            set;
        }

        public MyAbstractStarSystemDesignerRenderObject(MySystemObject obj)
        {
            RenderObject = obj;
            IsFocused = false;
        }

        /// <summary>
        /// Returns the size of the object itself. For example for a planet, it is the diameter
        /// </summary>
        /// <returns></returns>
        public abstract double GetObjectSize();

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
