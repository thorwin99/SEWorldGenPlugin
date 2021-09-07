using SEWorldGenPlugin.Generator.AsteroidObjects;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// StarSystemDesigner menu base for all asteroid objects.
    /// </summary>
    public abstract class MyStarSystemDesignerAsteroidMenu : MyStarSystemDesignerObjectMenu
    {
        public IMyAsteroidData Data
        {
            get;
            protected set;
        }
       
        /// <summary>
        /// Creates a new Asteroid object star system designer edit menu for the given object that edits the given
        /// asteroid data associated with it.
        /// </summary>
        /// <param name="obj">The asteroid object edited</param>
        /// <param name="data">The associated data</param>
        public MyStarSystemDesignerAsteroidMenu(MySystemAsteroids obj, IMyAsteroidData data) : base(obj)
        {
            Data = data;
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
        }
    }
}
