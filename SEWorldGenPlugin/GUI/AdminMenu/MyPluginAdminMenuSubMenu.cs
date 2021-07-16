using SEWorldGenPlugin.GUI.Controls;

namespace SEWorldGenPlugin.GUI.AdminMenu
{
    /// <summary>
    /// Class representing a sub menu for the admin menu
    /// </summary>
    public abstract class MyPluginAdminMenuSubMenu
    {
        /// <summary>
        /// The admin menu this sub menu belongs to
        /// </summary>
        protected readonly MyAdminMenuExtension m_adminMenu;

        public MyPluginAdminMenuSubMenu(MyAdminMenuExtension adminMenu)
        {
            m_adminMenu = adminMenu;
        }

        /// <summary>
        /// Adds its GUI elements as children to the parent element given by the admin menu.
        /// Builds the sub menus GUI elements. The elements of each row of the table should not exceed
        /// the <paramref name="maxWidth"/> in width.
        /// </summary>
        /// <param name="parent">Admin menu panel for sub menu GUI elements</param>
        /// <param name="maxWidth">THe maximum width usable in the parent item table</param>
        public abstract void RefreshInternals(MyGuiControlParentTableLayout parent, float maxWidth);

        /// <summary>
        /// Whether this sub menu is visible or not
        /// </summary>
        /// <returns>True if it is visible in the admin menu, false otherwise</returns>
        public abstract bool IsVisible();

        /// <summary>
        /// Returns the sub menus title, visible in the Admin menu dropdown box.
        /// Should be unique
        /// </summary>
        /// <returns>The title of this sub menu</returns>
        public abstract string GetTitle();
    }
}
