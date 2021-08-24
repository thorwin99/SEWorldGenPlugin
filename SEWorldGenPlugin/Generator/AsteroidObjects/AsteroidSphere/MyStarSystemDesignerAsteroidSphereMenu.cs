using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// Star system designer edit menu for asteroid spheres
    /// </summary>
    public class MyStarSystemDesignerAsteroidSphereMenu : MyStarSystemDesignerObjectMenu
    {
        public MyStarSystemDesignerAsteroidSphereMenu(MySystemObject obj) : base(obj)
        {
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
        }
    }
}
