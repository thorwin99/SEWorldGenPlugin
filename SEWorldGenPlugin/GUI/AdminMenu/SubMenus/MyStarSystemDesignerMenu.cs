using SEWorldGenPlugin.GUI.Controls;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus
{
    public class MyStarSystemDesignerMenu : MyPluginAdminMenuSubMenu
    {
        public override void Close()
        {
            
        }

        public override string GetTitle()
        {
            return "Star System Designer";
        }

        public override bool IsVisible()
        {
            return true;
        }

        public override void RefreshInternals(MyGuiControlParentTableLayout parent, float maxWidth, MyAdminMenuExtension instance)
        {
            
        }
    }
}
