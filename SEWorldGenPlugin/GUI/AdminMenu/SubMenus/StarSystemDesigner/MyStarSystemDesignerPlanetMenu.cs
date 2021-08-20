using Sandbox.Definitions;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System.Collections.Generic;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// Sub menu for the star system designer to edit or spawn planets
    /// </summary>
    public class MyStarSystemDesignerPlanetMenu : MyStarSystemDesignerObjectMenu
    {
        /// <summary>
        /// A combobox for the type of planet
        /// </summary>
        MyGuiControlCombobox m_planetTypeCombobox;

        /// <summary>
        /// A slider for the size of the planet in m
        /// </summary>
        MyGuiControlClickableSlider m_sizeSlider;

        /// <summary>
        /// A textbox to enter the orbit radius in km
        /// </summary>
        MyGuiControlTextbox m_orbitRadiusTextbox;

        /// <summary>
        /// A slider for the position of the planet on the orbit, between 0 and 360 degrees.
        /// </summary>
        MyGuiControlClickableSlider m_orbitPosSlider;

        /// <summary>
        /// The slider for the elevation of the planet over the system plane, between -90 and 90 degrees.
        /// </summary>
        MyGuiControlClickableSlider m_elevationSldier;

        /// <summary>
        /// A list of planet types to spawn
        /// </summary>
        List<MyPlanetGeneratorDefinition> m_planetTypes;

        public MyStarSystemDesignerPlanetMenu(MySystemObject obj) : base(obj)
        {
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            var settings = MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings;
            m_planetTypeCombobox = new MyGuiControlCombobox();
            m_planetTypeCombobox.Size = new VRageMath.Vector2(maxWidth, m_planetTypeCombobox.Size.Y);

            m_sizeSlider = new MyGuiControlClickableSlider(null, 1f, settings.PlanetSizeCap, maxWidth - 0.1f, intValue: true, labelSuffix: " m");

            m_orbitRadiusTextbox = new MyGuiControlTextbox(type: MyGuiControlTextboxType.DigitsOnly);
            m_orbitRadiusTextbox.Size = new VRageMath.Vector2(maxWidth - 0.1f, m_orbitRadiusTextbox.Size.Y);

            m_orbitPosSlider = new MyGuiControlClickableSlider(null, 0f, 360f, maxWidth - 0.1f, 0, showLabel: true);

            m_elevationSldier = new MyGuiControlClickableSlider(null, -90f, 90f, maxWidth - 0.1f, 0f);

            LoadPlanetList();
        }

        private void LoadPlanetList()
        {
            m_planetTypeCombobox.Clear();
            m_planetTypes.Clear();
            var planets = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions();

            foreach(var planet in planets)
            {
                m_planetTypes.Add(planet);
                m_planetTypeCombobox.AddItem(m_planetTypes.IndexOf(planet), planet.Id.SubtypeId.ToString());
            }
        }
    }
}
