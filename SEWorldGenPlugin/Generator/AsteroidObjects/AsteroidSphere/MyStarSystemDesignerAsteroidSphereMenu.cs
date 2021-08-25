using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// Star system designer edit menu for asteroid spheres
    /// </summary>
    public class MyStarSystemDesignerAsteroidSphereMenu : MyStarSystemDesignerAsteroidMenu
    {
        public MyStarSystemDesignerAsteroidSphereMenu(MySystemAsteroids obj, MyAsteroidSphereData data) : base(obj, data)
        {
            if (obj == null)
            {
                m_object = new MySystemAsteroids();
                var roid = m_object as MySystemAsteroids;
                roid.AsteroidTypeName = MyAsteroidSphereProvider.Static.GetTypeName();
            }
            if (data == null)
            {
                m_data = new MyAsteroidSphereData();
            }
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
        }
    }
}
