using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidCluster
{
    /// <summary>
    /// Star system designer menu to create asteroid clusters.
    /// </summary>
    public class MyStarSystemDesigneAsteroidClusterMenu : MyStarSystemDesignerOrbitMenu
    {
        /// <summary>
        /// Data associated with the edited instance
        /// </summary>
        public MyAsteroidClusterData Data
        {
            get;
            private set;
        }

        /// <summary>
        /// The slider used to set the size of the cluster
        /// </summary>
        private MyGuiControlClickableSlider m_sizeSlider;

        public MyStarSystemDesigneAsteroidClusterMenu(MySystemAsteroids instance, MyAsteroidClusterData data) : base(instance)
        {
            Data = data;
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            base.RecreateControls(controlTable, maxWidth, isEditing);

            m_sizeSlider = new MyGuiControlClickableSlider(null, 0, MySettingsSession.Static.Settings.GeneratorSettings.MinMaxOrbitDistance.Min / 20, maxWidth, (float)Data.Size, labelSuffix: " m");
            m_sizeSlider.ValueChanged += delegate (MyGuiControlSlider s)
            {
                Data.Size = s.Value;
                ChangedObject();
            };

            controlTable.AddTableRow(m_sizeSlider);
        }
    }
}
