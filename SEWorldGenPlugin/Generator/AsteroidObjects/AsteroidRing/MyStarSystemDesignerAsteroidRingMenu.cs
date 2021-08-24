using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing
{
    /// <summary>
    /// Star system designer sub menu for asteroid rings
    /// </summary>
    public class MyStarSystemDesignerAsteroidRingMenu : MyStarSystemDesignerObjectMenu
    {
        public MyStarSystemDesignerAsteroidRingMenu(MySystemObject obj) : base(obj)
        {
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            
        }
    }
}
