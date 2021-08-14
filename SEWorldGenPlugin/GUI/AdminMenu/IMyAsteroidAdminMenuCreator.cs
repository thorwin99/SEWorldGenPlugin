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
        bool OnEditMenuSelectItem(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen, MySystemAsteroids asteroidObject, MyObjectBuilder_SystemData starSystem);

        /// <summary>
        /// Creates the gui menu to spawn or edit an Asteroid object instance data.
        /// The GUI elements are added to the <paramref name="parentTable"/>.
        /// Incase a new instance is spawned, <paramref name="selectedInstance"/> is null, if an existing one is edited, it has a value.
        /// </summary>
        /// <param name="usableWidth">The max width of the GUI elements in the table.</param>
        /// <param name="parentTable">The parent table the GUI elements should be put in</param>
        /// <param name="selectedInstance">The selected instance to edit or null if a new one is spawned.</param>
        void CreateDataEditMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MySystemAsteroids selectedInstance);

        /// <summary>
        /// Called, when menus created by this get closed
        /// </summary>
        void Close();
    }
}
