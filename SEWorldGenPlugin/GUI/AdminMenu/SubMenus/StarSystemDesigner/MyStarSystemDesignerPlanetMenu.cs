using Sandbox.Definitions;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// Sub menu for the star system designer to edit or spawn planets
    /// </summary>
    public class MyStarSystemDesignerPlanetMenu : MyStarSystemDesignerOrbitMenu
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
        /// A list of planet types to spawn
        /// </summary>
        List<MyPlanetGeneratorDefinition> m_planetTypes;

        public MyStarSystemDesignerPlanetMenu(MySystemPlanet obj) : base(obj)
        {
            CanAddChild = true;
            CanBeRemoved = false;
            m_planetTypes = new List<MyPlanetGeneratorDefinition>();
            if(obj == null)
            {
                MyPluginLog.Debug("Trying to edit a non planet with the planet editing menu.", LogLevel.ERROR);
                m_object = new MySystemPlanet();
            }
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            CanBeRemoved = !isEditing;

            var settings = MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings;
            m_planetTypeCombobox = new MyGuiControlCombobox();
            m_planetTypeCombobox.Size = new Vector2(maxWidth - 0.01f, m_planetTypeCombobox.Size.Y);
            m_planetTypeCombobox.SetToolTip(new MyToolTips("The type of the planet."));

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Planet type"));
            controlTable.AddTableRow(m_planetTypeCombobox);

            m_sizeSlider = new MyGuiControlClickableSlider(null, 1f, settings.PlanetSizeCap / 1000f, maxWidth - 0.1f, intValue: true, labelSuffix: " km", showLabel: true);
            m_sizeSlider.SetToolTip(new MyToolTips("The size of the planet in meters."));

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Planet size"));
            controlTable.AddTableRow(m_sizeSlider);

            base.RecreateControls(controlTable, maxWidth, isEditing);

            LoadPlanetList();
            LoadPlanetProperties();

            m_sizeSlider.ValueChanged += OnSizeChanged;
            m_planetTypeCombobox.ItemSelected += OnTypeSelected;

            if (isEditing)
            {
                m_planetTypeCombobox.Enabled = false;
                m_sizeSlider.Enabled = false;
            }

            OnSizeChanged(m_sizeSlider);
            OnTypeSelected();
            ChangedObject();
        }

        /// <summary>
        /// Loads the properties from the edited object and sets the sliders accordingly.
        /// </summary>
        private void LoadPlanetProperties()
        {
            if (m_object == null) return;
            SetPlanetType();
            SetPlanetSize();
        }

        /// <summary>
        /// Gets the planet type from the edited object and sets the selected item in the combobox.
        /// </summary>
        private void SetPlanetType()
        {
            MySystemPlanet planet = m_object as MySystemPlanet;

            string type = planet.SubtypeId;

            foreach (var planetDef in m_planetTypes)
            {
                if (planetDef.Id.SubtypeId.ToString() == type)
                {
                    m_planetTypeCombobox.SelectItemByKey(m_planetTypes.IndexOf(planetDef));
                    return;
                }
            }

            m_planetTypeCombobox.SelectItemByIndex(0);
        }

        /// <summary>
        /// Action when planet type got selected.
        /// </summary>
        private void OnTypeSelected()
        {
            int typeIndex = (int)m_planetTypeCombobox.GetSelectedKey();
            MySystemPlanet p = m_object as MySystemPlanet;
            p.SubtypeId = m_planetTypes[typeIndex].Id.SubtypeId.ToString();

            ChangedObject();
        }

        /// <summary>
        /// Sets the slider for planet size to the size of the edited planet
        /// </summary>
        private void SetPlanetSize()
        {
            MySystemPlanet planet = m_object as MySystemPlanet;
            m_sizeSlider.Value = (float)planet.Diameter / 1000f;
        }

        /// <summary>
        /// Action when the size of the object is changed
        /// </summary>
        /// <param name="s">Slider for size</param>
        private void OnSizeChanged(MyGuiControlSlider s)
        {
            MySystemPlanet planet = m_object as MySystemPlanet;
            planet.Diameter = s.Value * 1000f;

            ChangedObject();
        }

        /// <summary>
        /// Loads all planet types and puts them into the PlanetTypeCombobox aswell as the planetTypes list for index association.
        /// </summary>
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
