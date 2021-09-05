using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using System;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// The interface for a submenu of an System object for the star system designer.
    /// </summary>
    public abstract class MyStarSystemDesignerObjectMenu
    {
        /// <summary>
        /// The system object of this menu
        /// </summary>
        protected MySystemObject m_object;

        /// <summary>
        /// Event called when properties of the edited object are changed
        /// </summary>
        public event Action<MySystemObject> OnObjectChanged;

        /// <summary>
        /// Whether a child object can be added to this object or not
        /// </summary>
        public bool CanAddChild
        {
            get;
            protected set;
        }

        /// <summary>
        /// The object this menu edits or spawns
        /// </summary>
        /// <param name="obj">The object in the system</param>
        public MyStarSystemDesignerObjectMenu(MySystemObject obj)
        {
            m_object = obj;
            CanAddChild = false;
        }

        /// <summary>
        /// Recreates the controls of this sub menu of the star system designer to edit or spawn the given system object.
        /// It should always create all controls regardless of <paramref name="isEditing"/> and only disable ones not used.
        /// The values of the controls should reflect the state of the edited object.
        /// When the controls change values, the value should be reflected in the edited object so it can be retreived at will.
        /// </summary>
        /// <param name="controlTable">The parent table the controls get added to.</param>
        /// <param name="maxWidth">The maximum width of the control elements.</param>
        /// <param name="isEditing">Whether the object is edited and not spawned newly.</param>
        public abstract void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false);

        /// <summary>
        /// Returns the object edited or spawned by this menu
        /// </summary>
        /// <returns></returns>
        public MySystemObject GetObject()
        {
            return m_object;
        }

        /// <summary>
        /// Subclasses should call this to notify the star system designer about a changed properties of the object.
        /// Each time a property is changed, this should be called.
        /// </summary>
        protected void ChangedObject()
        {
            if (OnObjectChanged == null) return;
            OnObjectChanged(m_object);
        }
    }
}
