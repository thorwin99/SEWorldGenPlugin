using SEWorldGenPlugin.GUI.Controls;

namespace SEWorldGenPlugin.GUI.AdminMenu
{
    /// <summary>
    /// Class representing a sub menu for the admin menu.
    /// Should only be registered ONCE in the <see cref="MyAdminMenuExtension"/> per game session,
    /// i.e. on startup or initilazion of the plugin.
    /// </summary>
    public abstract class MyPluginAdminMenuSubMenu
    {
        /// <summary>
        /// Adds its GUI elements as children to the parent element given by the admin menu.
        /// Builds the sub menus GUI elements. The elements of each row of the table should not exceed
        /// the <paramref name="maxWidth"/> in width.
        /// </summary>
        /// <param name="parent">Admin menu panel for sub menu GUI elements</param>
        /// <param name="maxWidth">THe maximum width usable in the parent item table</param>
        /// <param name="instance">The instance of the admin menu, that refreshed the internals</param>
        public abstract void RefreshInternals(MyGuiControlParentTableLayout parent, float maxWidth, MyAdminMenuExtension instance);

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

        /// <summary>
        /// Called when the currently opened admin menu gets closed. Can be
        /// used to clean up data that should not persist between admin menu
        /// instances, since an instance of a sub menu is persistent for the
        /// entire game session.
        /// </summary>
        public abstract void Close();
    }
}
