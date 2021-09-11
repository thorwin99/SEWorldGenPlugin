using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using VRageMath;
using VRageRender;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// A class simply used to implement the <see cref="MyAbstractStarSystemDesignerRenderObject"/> for
    /// objects that wont be rendered, but still can be focused, such as the system center
    /// </summary>
    public class MyEmptyObjectRenderObject : MyOrbitRenderObject
    {
        public MyEmptyObjectRenderObject(MySystemObject obj) : base(obj)
        {
        }

        public override void Draw()
        {
            base.Draw();

            Color color = IsFocused ? Color.LightBlue : Color.Green;
            MyRenderProxy.DebugDrawText3D(RenderObject.CenterPosition, RenderObject.DisplayName, color, 0.25f, true, align: VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
        }

        public override double GetObjectSize()
        {
            return MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.PlanetSizeCap;
        }
    }
}
