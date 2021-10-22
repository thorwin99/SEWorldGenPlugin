using Sandbox.Definitions;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using System.Text;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus
{
    /// <summary>
    /// A class to create an admin sub menu to spawn supersized planets.
    /// </summary>
    public class MyPlanetSpawnMenu : MyPluginAdminMenuSubMenu
    {
        /// <summary>
        /// Listbox containing all loaded planet definitions to spawn them
        /// </summary>
        private MyGuiControlListbox m_planetDefList;

        /// <summary>
        /// Textbox for the name of the planet to spawn
        /// </summary>
        private MyGuiControlTextbox m_nameBox;

        /// <summary>
        /// Slider for the planet size
        /// </summary>
        private MyGuiControlClickableSlider m_planetSizeSlider;

        /// <summary>
        /// Button to spawn the planet with currently entered parameters
        /// </summary>
        private MyGuiControlButton m_spawnPlanetButton;

        /// <summary>
        /// Button to spawn the planet at a given coordinate
        /// </summary>
        private MyGuiControlButton m_spawnAtCoordButton;

        public override void Close()
        {
        }

        public override void Draw()
        {
        }

        public override string GetTitle()
        {
            return "Supersized-Planets";
        }

        public override bool IsVisible()
        {
            return MyPluginSession.Static.ServerVersionMatch &&
                (MySession.Static.IsUserAdmin(Sync.MyId) ||
                MySession.Static.IsUserSpaceMaster(Sync.MyId)) &&
                MySettingsSession.Static.Settings.Enabled;
        }

        public override void RefreshInternals(MyGuiControlParentTableLayout parent, float maxWidth, MyAdminMenuExtension instance)
        {
            var settings = MySettingsSession.Static.Settings.GeneratorSettings;

            m_planetDefList = new MyGuiControlListbox();
            m_planetDefList.VisibleRowsCount = 8;
            m_planetDefList.MultiSelect = false;
            m_planetDefList.Size = new Vector2(maxWidth, m_planetDefList.Size.Y);
            m_planetDefList.ItemsSelected += delegate (MyGuiControlListbox box)
            {
                m_spawnAtCoordButton.Enabled = m_planetDefList.GetLastSelected() != null;
                m_spawnPlanetButton.Enabled = m_spawnAtCoordButton.Enabled;
            };

            parent.AddTableRow(new MyGuiControlLabel(text: "Type"));
            parent.AddTableRow(m_planetDefList);

            parent.AddTableSeparator();

            m_planetSizeSlider = new MyGuiControlClickableSlider(null, 1f, settings.PlanetSettings.PlanetSizeCap, maxWidth - 0.1f, intValue: true, showLabel: true, labelSuffix: " m");
            m_planetSizeSlider.DefaultValue = Math.Min(120000, settings.PlanetSettings.PlanetSizeCap);
            m_planetSizeSlider.Value = m_planetSizeSlider.DefaultValue.Value;
            m_planetSizeSlider.SetToolTip("The diameter of the planet");

            parent.AddTableRow(new MyGuiControlLabel(text: "Diameter"));
            parent.AddTableRow(m_planetSizeSlider);

            m_nameBox = new MyGuiControlTextbox();
            m_nameBox.Size = new Vector2(maxWidth, m_nameBox.Size.Y);
            m_nameBox.SetToolTip("The name of this planet");

            parent.AddTableRow(new MyGuiControlLabel(text: "Name"));
            parent.AddTableRow(m_nameBox);


            parent.AddTableSeparator();

            m_spawnPlanetButton = MyPluginGuiHelper.CreateDebugButton(maxWidth, "Spawn planet", delegate (MyGuiControlButton button)
            {
                OnSpawnPlanet();
            });
            m_spawnPlanetButton.Enabled = true;
            m_spawnPlanetButton.SetToolTip("Activates the mode to spawn the planet where you place it");

            m_spawnAtCoordButton = MyPluginGuiHelper.CreateDebugButton(maxWidth, "Spawn planet at coordinates", delegate (MyGuiControlButton button)
            {
                OnSpawnPlanetCoord();
            });

            parent.AddTableRow(m_spawnPlanetButton);

            parent.AddTableRow(m_spawnAtCoordButton);

            LoadPlanetDefs();

            m_spawnAtCoordButton.Enabled = m_planetDefList.GetLastSelected() != null;
            m_spawnPlanetButton.Enabled = m_spawnAtCoordButton.Enabled;
        }

        /// <summary>
        /// Spawns the planet at a fixed coordinate
        /// </summary>
        private void OnSpawnPlanetCoord()
        {
            StringBuilder name = new StringBuilder();
            m_nameBox.GetText(name);
            if (name.ToString().Trim().Length <= 3)
            {
                MyPluginGuiHelper.DisplayError("The name must be at least 4 letters long", "Error");
                return;
            }

            MyGuiScreenDialogCoordinate coordinateInput = new MyGuiScreenDialogCoordinate("Planet coordinate");

            coordinateInput.OnConfirmed += delegate (Vector3D coord)
            {
                MySystemPlanet p = new MySystemPlanet()
                {
                    CenterPosition = coord,
                    SubtypeId = ((MyPlanetGeneratorDefinition)m_planetDefList.GetLastSelected().UserData).Id.SubtypeId.ToString(),
                    Generated = false,
                    DisplayName = name.ToString().Trim(),
                    Diameter = m_planetSizeSlider.Value
                };

                SpawnPlanet(p, coord);
            };

            MyGuiSandbox.AddScreen(coordinateInput);
        }

        /// <summary>
        /// Action to spawn a planet
        /// </summary>
        private void OnSpawnPlanet()
        {
            StringBuilder name = new StringBuilder();
            m_nameBox.GetText(name);
            if (name.ToString().Trim().Length <= 3)
            {
                MyPluginGuiHelper.DisplayError("The name must be at least 4 letters long", "Error");
                return;
            }

            float size = m_planetSizeSlider.Value;
            MySystemPlanet planet = new MySystemPlanet()
            {
                CenterPosition = Vector3D.Zero,
                SubtypeId = ((MyPlanetGeneratorDefinition)m_planetDefList.GetLastSelected().UserData).Id.SubtypeId.ToString(),
                Generated = false,
                DisplayName = name.ToString().Trim(),
                Diameter = size
            };

            MyPluginPlanetClipboard.Static.Activate(planet, SpawnPlanet, size);

            MyAdminMenuExtension.Static.CloseScreenNow();
        }

        /// <summary>
        /// Spawns a planet in the system
        /// </summary>
        /// <param name="planet">Planet to spawn</param>
        /// <param name="position">Position to spawn at</param>
        private void SpawnPlanet(MySystemPlanet planet, Vector3D position)
        {
            if (planet.Type == MySystemObjectType.PLANET)
            {
                planet.CenterPosition = position;

                MyStarSystemGenerator.Static.AddObjectToSystem(planet);
            }
        }

        /// <summary>
        /// Puts all loaded planet definitions into planet definition box
        /// </summary>
        private void LoadPlanetDefs()
        {
            m_planetDefList.Clear();
            var definitions = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions();
            foreach (var planet in definitions)
            {
                MyGuiControlListbox.Item i = new MyGuiControlListbox.Item(new StringBuilder(planet.Id.SubtypeId.ToString()), userData: planet);
                m_planetDefList.Items.Add(i);
            }
        }

        public override void HandleInput()
        {
        }
    }
}
