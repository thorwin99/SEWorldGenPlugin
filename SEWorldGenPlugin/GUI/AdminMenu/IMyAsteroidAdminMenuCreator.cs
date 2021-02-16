using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;

namespace SEWorldGenPlugin.GUI.AdminMenu
{
    /// <summary>
    /// Interface for admin menu creators for asteroid objects
    /// </summary>
    public interface IMyAsteroidAdminMenuCreator
    {
        /// <summary>
        /// Creates the gui elements nessecary to create a menu to spawn a asteroid object instance.
        /// The elements should be added to the table and the table, the admin screen only used for elements
        /// that dont fit in the table.
        /// </summary>
        /// <param name="usableWidth">Usable width of all elements.</param>
        /// <param name="parentTable">The parent table of the elements.</param>
        /// <param name="adminScreen">The admin menu screen.</param>
        /// <returns>True, if a menu was created</returns>
        bool CreateSpawnMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen);

        /// <summary>
        /// Creates the gui elements nessecary to create a menu to edit the given asteroid object instance.
        /// The elements should be added to the table and the table, the admin screen only used for elements
        /// that dont fit in the table.
        /// </summary>
        /// <param name="usableWidth">Usable width of all elements.</param>
        /// <param name="parentTable">The parent table of the elements.</param>
        /// <param name="adminScreen">The admin menu screen.</param>
        /// <param name="asteroidObject">The asteroid object this menu gets generated for</param>
        /// <returns>True, if a menu was created</returns>
        bool OnEditMenuSelectItem(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen, MySystemAsteroids asteroidObject);

        /// <summary>
        /// Called, when the admin menu gets closed.
        /// </summary>
        void OnAdminMenuClose();
    }
}
