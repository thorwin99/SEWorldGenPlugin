using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing
{
    /// <summary>
    /// Star system designer sub menu for asteroid rings
    /// </summary>
    public class MyStarSystemDesignerAsteroidRingMenu : MyStarSystemDesignerAsteroidMenu
    {
        public MyStarSystemDesignerAsteroidRingMenu(MySystemAsteroids obj, MyAsteroidRingData data) : base(obj, data)
        {
            if(obj == null)
            {
                m_object = new MySystemAsteroids();
                var roid = m_object as MySystemAsteroids;
                roid.AsteroidTypeName = MyAsteroidRingProvider.Static.GetTypeName();
            }
            if(data == null)
            {
                m_data = new MyAsteroidRingData();
            }
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            
        }
    }
}
