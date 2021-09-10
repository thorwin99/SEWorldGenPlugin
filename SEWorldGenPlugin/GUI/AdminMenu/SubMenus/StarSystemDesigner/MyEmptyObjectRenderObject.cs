using SEWorldGenPlugin.ObjectBuilders;
using System;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// A class simply used to implement the <see cref="MyAbstractStarSystemDesignerRenderObject"/> for
    /// objects that wont be rendered, but still can be focused, such as the system center
    /// </summary>
    public class MyEmptyObjectRenderObject : MyAbstractStarSystemDesignerRenderObject
    {
        public MyEmptyObjectRenderObject(MySystemObject obj) : base(obj)
        {
        }

        public override void Draw()
        {
        }

        public override double GetObjectSize()
        {
            return 0;
        }
    }
}
