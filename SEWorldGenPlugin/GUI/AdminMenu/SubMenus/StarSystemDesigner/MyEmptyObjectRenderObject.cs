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

        public override double GetObjectRenderSize(ZoomLevel level)
        {
            if(level == ZoomLevel.OBJECT)
            {
                return 10;
            }
            else
            {
                double radius = 0;
                foreach(var child in RenderObject.ChildObjects)
                {
                    if(child is MySystemPlanet)
                    {
                        radius = Math.Max(radius, Vector3D.Distance(RenderObject.CenterPosition, child.CenterPosition));
                    }
                    else if(child is MySystemAsteroids)
                    {
                        radius = Math.Max(radius, 0);
                    }
                }
                return radius;
            }
        }
    }
}
