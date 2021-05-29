using Sandbox;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    /// <summary>
    /// Gui class for the world settings for the plugin
    /// </summary>
    public class MyPluginSettingsMenu : MyGuiScreenBase
    {
        /// <summary>
        /// Action to call, when the menu is closed with ok and not aborted.
        /// </summary>
        public event Action OnOkButtonClicked;

        /// <summary>
        /// The size of this screen.
        /// </summary>
        private static readonly Vector2 SIZE = new Vector2(1024, 1120) / MyGuiConstants.GUI_OPTIMAL_SIZE;

        /// <summary>
        /// Padding for the child elements to the top left border of this screen
        /// </summary>
        private static readonly Vector2 PADDING = new Vector2(50) / MyGuiConstants.GUI_OPTIMAL_SIZE;

        /// <summary>
        /// The margin between 2 child elements.
        /// </summary>
        private static readonly Vector2 CHILD_MARGINS_VERT = new Vector2(0, 32f) / MyGuiConstants.GUI_OPTIMAL_SIZE;

        /// <summary>
        /// Whether this screen is shown to edit a new worlds settings or an existing ones.
        /// </summary>
        private bool m_isNewGame = false;

        /// <summary>
        /// All gui elements used to display various settings of the world
        /// </summary>
        private MyGuiControlCombobox m_systemGeneratorCombo;
        private MyGuiControlCombobox m_asteroidGeneratorCombo;
        private MyGuiControlCheckbox m_enableVanillaPlanetsCheckbox;
        private MyGuiControlRangedSlider m_planetCountSlider;
        private MyGuiControlRangedSlider m_asteroidCountSlider;
        private MyGuiControlRangedSlider m_orbitDistancesSlider;
        private MyGuiControlClickableSlider m_systemPlaneDeviationSlider;
        private MyGuiControlClickableSlider m_asteroidDensitySlider;
        private MyGuiControlClickableSlider m_worldSizeSlider;

        private MyGuiControlClickableSlider m_planetSizeCapSlider;
        private MyGuiControlClickableSlider m_planetSizeMultSlider;
        private MyGuiControlClickableSlider m_planetSizeDeviationSlider;
        private MyGuiControlClickableSlider m_planetMoonBasePropSlider;
        private MyGuiControlClickableSlider m_planetRingBasePropSlider;
        private MyGuiControlRangedSlider m_planetMoonMinMaxSlider;

        private MyGuiControlCombobox m_planetGPSModeCombo;
        private MyGuiControlCombobox m_moonGPSModeCombo;
        private MyGuiControlCombobox m_asteroidGPSModeCombo;

        public MyPluginSettingsMenu(bool isNewGame) : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, SIZE, false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            m_isNewGame = isNewGame;
            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            var caption = AddCaption("SEWorldGenPlugin world settings");
            caption.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            caption.Position = new Vector2(0, SIZE.Y / -2 + PADDING.Y);

            MyGuiControlButton OkButton = new MyGuiControlButton(null, VRage.Game.MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, VRage.MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, OkButtonClicked);
            OkButton.Position = new Vector2(0, SIZE.Y / 2 - PADDING.Y);
            Controls.Add(OkButton);

            MyGuiControlSeparatorList separators = new MyGuiControlSeparatorList();
            separators.AddHorizontal(SIZE / -2 + PADDING + new Vector2(0, caption.Size.Y) + CHILD_MARGINS_VERT, SIZE.X - 2 * PADDING.X);
            separators.AddHorizontal(new Vector2(SIZE.X / -2 + PADDING.X, SIZE.Y / 2 - PADDING.Y - OkButton.Size.Y) - CHILD_MARGINS_VERT, SIZE.X - 2 * PADDING.X);
            Controls.Add(separators);

            MyGuiControlParentTableLayout parent = new MyGuiControlParentTableLayout(2);
            parent.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;

            //Generate rows for parent layout containing settings

            var systemGenLabel = new MyGuiControlLabel(null, null, "Generator mode");
            var asteroidGenLabel = new MyGuiControlLabel(null, null, "Asteroid generator mode");
            var enableVanillaLabel = new MyGuiControlLabel(null, null, "Use vanilla planets");
            var planetCountLabel = new MyGuiControlLabel(null, null, "Planet count");
            var asteroidCountLabel = new MyGuiControlLabel(null, null, "Asteroid object count");
            var oribtDistanceLabel = new MyGuiControlLabel(null, null, "Orbit distances");
            var planeDeviationLabel = new MyGuiControlLabel(null, null, "System plane deviation");
            var asteroidDensityLabel = new MyGuiControlLabel(null, null, "Asteroid density");
            var worldSizeLabel = new MyGuiControlLabel(null, null, "World size");
            var planetSizeCapLabel = new MyGuiControlLabel(null, null, "Planet size cap");
            var planetSizeMultLabel = new MyGuiControlLabel(null, null, "Planet size multiplier");
            var planetSizeDeviationLabel = new MyGuiControlLabel(null, null, "Planet size deviation");
            var planetMoonPropLabel = new MyGuiControlLabel(null, null, "Planet moon probability");
            var planetRingPropLabel = new MyGuiControlLabel(null, null, "Planet ring probability");
            var planetMoonCountLabel = new MyGuiControlLabel(null, null, "Planet moon count");
            var planetGpsModeLabel = new MyGuiControlLabel(null, null, "Planet gps mode");
            var moonGpsModeLabel = new MyGuiControlLabel(null, null, "Moon gps mode");
            var asteroidGpsModeLabel = new MyGuiControlLabel(null, null, "Asteroid gps mode");

            m_systemGeneratorCombo = new MyGuiControlCombobox(null, null);
            m_systemGeneratorCombo.SetToolTip(MyPluginTexts.TOOLTIPS.SYS_GEN_MODE_COMBO);
            m_systemGeneratorCombo.AddItem((long)SystemGenerationMethod.FULL_RANDOM, "Full random");
            m_systemGeneratorCombo.AddItem((long)SystemGenerationMethod.UNIQUE, "Unique");
            m_systemGeneratorCombo.AddItem((long)SystemGenerationMethod.MANDATORY_FIRST, "Mandatory first");
            m_systemGeneratorCombo.AddItem((long)SystemGenerationMethod.MANDATORY_ONLY, "Mandatory only");
            m_systemGeneratorCombo.AddItem((long)SystemGenerationMethod.MANDATORY_UNIQUE, "Mandatory only unique");

            m_systemGeneratorCombo.Size = new Vector2(0.25f, m_systemGeneratorCombo.Size.Y);

            parent.AddTableRow(systemGenLabel, m_systemGeneratorCombo);

            m_asteroidGeneratorCombo = new MyGuiControlCombobox();
            m_asteroidGeneratorCombo.SetToolTip(MyPluginTexts.TOOLTIPS.ASTEROID_GEN_MODE_COMBO);
            m_asteroidGeneratorCombo.AddItem((long)AsteroidGenerationMethod.PLUGIN, "Plugin");
            m_asteroidGeneratorCombo.AddItem((long)AsteroidGenerationMethod.VANILLA, "Vanilla");
            m_asteroidGeneratorCombo.AddItem((long)AsteroidGenerationMethod.BOTH, "Combined");
            m_asteroidGeneratorCombo.ItemSelected += delegate()
            {
                m_asteroidGPSModeCombo.Enabled = m_asteroidGeneratorCombo.GetSelectedKey() != (long)AsteroidGenerationMethod.VANILLA;
                m_asteroidDensitySlider.Enabled = m_asteroidGeneratorCombo.GetSelectedKey() != (long)AsteroidGenerationMethod.VANILLA;
            };

            m_asteroidGeneratorCombo.Size = new Vector2(0.25f, m_asteroidGeneratorCombo.Size.Y);

            parent.AddTableRow(asteroidGenLabel, m_asteroidGeneratorCombo);

            m_enableVanillaPlanetsCheckbox = new MyGuiControlCheckbox();
            m_enableVanillaPlanetsCheckbox.SetToolTip(MyPluginTexts.TOOLTIPS.VANILLA_PLANETS_CHECK);

            parent.AddTableRow(enableVanillaLabel, m_enableVanillaPlanetsCheckbox);

            m_planetCountSlider = new MyGuiControlRangedSlider(0, 50, 5, 15, true, width: 0.25f);
            m_planetCountSlider.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_COUNT_SLIDER);

            parent.AddTableRow(planetCountLabel, m_planetCountSlider);

            m_asteroidCountSlider = new MyGuiControlRangedSlider(0, 50, 5, 15, true, width: 0.25f);
            m_asteroidCountSlider.SetToolTip(MyPluginTexts.TOOLTIPS.ASTEROID_COUNT_SLIDER);

            parent.AddTableRow(asteroidCountLabel, m_asteroidCountSlider);

            m_orbitDistancesSlider = new MyGuiControlRangedSlider(100, 10000000, 40000, 1000000, width: 0.25f, useLogScale: true);
            m_orbitDistancesSlider.SetToolTip(MyPluginTexts.TOOLTIPS.ORBIT_DISTANCE_SLIDER);

            parent.AddTableRow(oribtDistanceLabel, m_orbitDistancesSlider);

            m_systemPlaneDeviationSlider = new MyGuiControlClickableSlider(minValue: 0, maxValue: 90f, defaultValue: 5f, width: 0.25f, showLabel: true);
            m_systemPlaneDeviationSlider.SetToolTip(MyPluginTexts.TOOLTIPS.SYSTEM_PLANE_DEV_SLIDER);

            parent.AddTableRow(planeDeviationLabel, m_systemPlaneDeviationSlider);

            m_asteroidDensitySlider = new MyGuiControlClickableSlider(minValue: 0, maxValue: 1, defaultValue: 0.6f, width: 0.25f, showLabel: true);
            m_asteroidDensitySlider.SetToolTip(MyPluginTexts.TOOLTIPS.ASTEROID_DENS_SLIDER);

            parent.AddTableRow(asteroidDensityLabel, m_asteroidDensitySlider);

            m_worldSizeSlider = new MyGuiControlClickableSlider(minValue: -1, maxValue: 1000000000, defaultValue: -1, width: 0.25f, showLabel: true, labelSuffix: " Km");
            m_worldSizeSlider.SetToolTip(MyPluginTexts.TOOLTIPS.WORLD_SIZE_SLIDER);
            m_worldSizeSlider.OnLabelUpdate += delegate (MyGuiControlLabel l)
            {
                if (m_worldSizeSlider.Value < 0)
                {
                    l.Text = "Infinite";
                }
            };

            parent.AddTableRow(worldSizeLabel, m_worldSizeSlider);

            m_planetSizeCapSlider = new MyGuiControlClickableSlider(minValue: 1, maxValue: 2400000, defaultValue: 1200000, intValue: true, width: 0.25f, showLabel: true, labelSuffix: " m");
            m_planetSizeCapSlider.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_SIZE_CAP_SLIDER);

            parent.AddTableRow(planetSizeCapLabel, m_planetSizeCapSlider);

            m_planetSizeMultSlider = new MyGuiControlClickableSlider(minValue: 0.1f, maxValue: 10, defaultValue: 2, width: 0.25f, showLabel: true);
            m_planetSizeMultSlider.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_SIZE_MULT);

            parent.AddTableRow(planetSizeMultLabel, m_planetSizeMultSlider);

            m_planetSizeDeviationSlider = new MyGuiControlClickableSlider(minValue: 0f, maxValue: 0.5f, defaultValue: 0f, width: 0.25f, showLabel: true);
            m_planetSizeDeviationSlider.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_SIZE_DEV);

            parent.AddTableRow(planetSizeDeviationLabel, m_planetSizeDeviationSlider);

            m_planetMoonBasePropSlider = new MyGuiControlClickableSlider(minValue: 0f, maxValue: 1f, defaultValue: 0.5f, width: 0.25f, showLabel: true);
            m_planetMoonBasePropSlider.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_MOON_PROP);

            parent.AddTableRow(planetMoonPropLabel, m_planetMoonBasePropSlider);

            m_planetRingBasePropSlider = new MyGuiControlClickableSlider(minValue: 0f, maxValue: 1f, defaultValue: 0.5f, width: 0.25f, showLabel: true);
            m_planetRingBasePropSlider.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_RING_PROP);

            parent.AddTableRow(planetRingPropLabel, m_planetRingBasePropSlider);

            m_planetMoonMinMaxSlider = new MyGuiControlRangedSlider(1, 50, 1, 25, true, showLabel: true, width: 0.25f);
            m_planetMoonMinMaxSlider.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_MOON_COUNT);

            parent.AddTableRow(planetMoonCountLabel, m_planetMoonMinMaxSlider);

            m_planetGPSModeCombo = new MyGuiControlCombobox();
            m_planetGPSModeCombo.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_GPS_COMBO);
            m_planetGPSModeCombo.AddItem((long)MyGPSGenerationMode.DISCOVERY, "Discovery");
            m_planetGPSModeCombo.AddItem((long)MyGPSGenerationMode.PERSISTENT, "Persistent");
            m_planetGPSModeCombo.AddItem((long)MyGPSGenerationMode.PERSISTENT_HIDDEN, "Persistent hidden");
            m_planetGPSModeCombo.AddItem((long)MyGPSGenerationMode.NONE, "None");

            m_planetGPSModeCombo.Size = new Vector2(0.25f, m_planetGPSModeCombo.Size.Y);

            parent.AddTableRow(planetGpsModeLabel, m_planetGPSModeCombo);

            m_moonGPSModeCombo = new MyGuiControlCombobox();
            m_moonGPSModeCombo.SetToolTip(MyPluginTexts.TOOLTIPS.MOON_GPS_COMBO);
            m_moonGPSModeCombo.AddItem((long)MyGPSGenerationMode.DISCOVERY, "Discovery");
            m_moonGPSModeCombo.AddItem((long)MyGPSGenerationMode.PERSISTENT, "Persistent");
            m_moonGPSModeCombo.AddItem((long)MyGPSGenerationMode.PERSISTENT_HIDDEN, "Persistent hidden");
            m_moonGPSModeCombo.AddItem((long)MyGPSGenerationMode.NONE, "None");

            m_moonGPSModeCombo.Size = new Vector2(0.25f, m_moonGPSModeCombo.Size.Y);

            parent.AddTableRow(moonGpsModeLabel, m_moonGPSModeCombo);

            m_asteroidGPSModeCombo = new MyGuiControlCombobox();
            m_asteroidGPSModeCombo.SetToolTip(MyPluginTexts.TOOLTIPS.ASTEROID_GPS_COMBO);
            m_asteroidGPSModeCombo.AddItem((long)MyGPSGenerationMode.DISCOVERY, "Discovery");
            m_asteroidGPSModeCombo.AddItem((long)MyGPSGenerationMode.PERSISTENT, "Persistent");
            m_asteroidGPSModeCombo.AddItem((long)MyGPSGenerationMode.PERSISTENT_HIDDEN, "Persistent hidden");
            m_asteroidGPSModeCombo.AddItem((long)MyGPSGenerationMode.NONE, "None");

            m_asteroidGPSModeCombo.Size = new Vector2(0.25f, m_asteroidGPSModeCombo.Size.Y);

            parent.AddTableRow(asteroidGpsModeLabel, m_asteroidGPSModeCombo);

            parent.ApplyRows();

            Vector2 start = SIZE / -2 + PADDING + new Vector2(0, caption.Size.Y) + CHILD_MARGINS_VERT * 2;
            Vector2 end = new Vector2(SIZE.X / 2 - PADDING.X, SIZE.Y / 2 - PADDING.Y - OkButton.Size.Y) - CHILD_MARGINS_VERT * 2;

            MyGuiControlScrollablePanel scrollPane = new MyGuiControlScrollablePanel(parent);
            scrollPane.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            scrollPane.ScrollbarVEnabled = true;
            scrollPane.Size = end - start;
            scrollPane.Position = start;

            Controls.Add(scrollPane);
        }

        /// <summary>
        /// Called when the ok button was clicked
        /// </summary>
        /// <param name="obj"></param>
        private void OkButtonClicked(MyGuiControlButton obj)
        {
            OnOkButtonClicked?.Invoke();
            CloseScreen();
        }

        /// <summary>
        /// Sets the settings for this menu screen to display.
        /// </summary>
        /// <param name="worldSettings">The world settings to display</param>
        public void SetSettings(MyObjectBuilder_WorldSettings worldSettings)
        {
            m_systemGeneratorCombo.SelectItemByKey((long)worldSettings.GeneratorSettings.SystemGenerator);
            m_asteroidGeneratorCombo.SelectItemByKey((long)worldSettings.GeneratorSettings.AsteroidGenerator);
            m_enableVanillaPlanetsCheckbox.IsChecked = worldSettings.GeneratorSettings.AllowVanillaPlanets;
            m_planetCountSlider.SetValues(worldSettings.GeneratorSettings.MinMaxPlanets.Min, worldSettings.GeneratorSettings.MinMaxPlanets.Max);
            m_asteroidCountSlider.SetValues(worldSettings.GeneratorSettings.MinMaxAsteroidObjects.Min, worldSettings.GeneratorSettings.MinMaxAsteroidObjects.Max);
            m_orbitDistancesSlider.SetValues(worldSettings.GeneratorSettings.MinMaxOrbitDistance.Min / 1000, worldSettings.GeneratorSettings.MinMaxOrbitDistance.Max / 1000);
            m_systemPlaneDeviationSlider.Value = worldSettings.GeneratorSettings.SystemPlaneDeviation;
            m_asteroidDensitySlider.Value = worldSettings.GeneratorSettings.AsteroidDensity;

            if(worldSettings.GeneratorSettings.WorldSize >= 0)
            {
                m_worldSizeSlider.Value = worldSettings.GeneratorSettings.WorldSize / 1000;
            }
            else
            {
                m_worldSizeSlider.Value = -1;
            }

            m_planetSizeCapSlider.Value = worldSettings.GeneratorSettings.PlanetSettings.PlanetSizeCap;
            m_planetSizeMultSlider.Value = worldSettings.GeneratorSettings.PlanetSettings.PlanetSizeMultiplier;
            m_planetSizeDeviationSlider.Value = worldSettings.GeneratorSettings.PlanetSettings.PlanetSizeDeviation;
            m_planetMoonBasePropSlider.Value = worldSettings.GeneratorSettings.PlanetSettings.BaseMoonProbability;
            m_planetRingBasePropSlider.Value = worldSettings.GeneratorSettings.PlanetSettings.BaseRingProbability;
            m_planetMoonMinMaxSlider.SetValues(worldSettings.GeneratorSettings.PlanetSettings.MinMaxMoons.Min, worldSettings.GeneratorSettings.PlanetSettings.MinMaxMoons.Max);

            m_planetGPSModeCombo.SelectItemByKey((long)worldSettings.GeneratorSettings.GPSSettings.PlanetGPSMode);
            m_asteroidGPSModeCombo.SelectItemByKey((long)worldSettings.GeneratorSettings.GPSSettings.AsteroidGPSMode);
            m_moonGPSModeCombo.SelectItemByKey((long)worldSettings.GeneratorSettings.GPSSettings.MoonGPSMode);
        }

        /// <summary>
        /// Retreives the world settings set in this gui screen for the plugin
        /// </summary>
        /// <returns>An object builder containing the currently set world settings</returns>
        public MyObjectBuilder_WorldSettings GetSettings()
        {
            MyObjectBuilder_WorldSettings settings = new MyObjectBuilder_WorldSettings();
            settings.GeneratorSettings.SystemGenerator = (SystemGenerationMethod)m_systemGeneratorCombo.GetSelectedKey();
            settings.GeneratorSettings.AsteroidGenerator = (AsteroidGenerationMethod)m_asteroidGeneratorCombo.GetSelectedKey();
            settings.GeneratorSettings.AllowVanillaPlanets = m_enableVanillaPlanetsCheckbox.IsChecked;
            settings.GeneratorSettings.MinMaxPlanets = new MySerializableMinMax((long)m_planetCountSlider.CurrentMin, (long)m_planetCountSlider.CurrentMax);
            settings.GeneratorSettings.MinMaxAsteroidObjects = new MySerializableMinMax((long)m_asteroidCountSlider.CurrentMin, (long)m_asteroidCountSlider.CurrentMax);
            settings.GeneratorSettings.MinMaxOrbitDistance = new MySerializableMinMax((long)(m_orbitDistancesSlider.CurrentMin * 1000), (long)(m_orbitDistancesSlider.CurrentMax * 1000));
            settings.GeneratorSettings.SystemPlaneDeviation = m_systemPlaneDeviationSlider.Value;
            settings.GeneratorSettings.AsteroidDensity = m_asteroidDensitySlider.Value;
            settings.GeneratorSettings.WorldSize = m_worldSizeSlider.Value >= 0 ? (long)(m_worldSizeSlider.Value * 1000) : -1;

            settings.GeneratorSettings.PlanetSettings.PlanetSizeCap = (int)m_planetSizeCapSlider.Value;
            settings.GeneratorSettings.PlanetSettings.PlanetSizeMultiplier = m_planetSizeMultSlider.Value;
            settings.GeneratorSettings.PlanetSettings.PlanetSizeDeviation = m_planetSizeDeviationSlider.Value;
            settings.GeneratorSettings.PlanetSettings.BaseMoonProbability = m_planetMoonBasePropSlider.Value;
            settings.GeneratorSettings.PlanetSettings.BaseRingProbability = m_planetRingBasePropSlider.Value;
            settings.GeneratorSettings.PlanetSettings.MinMaxMoons = new MySerializableMinMax((long)m_planetMoonMinMaxSlider.CurrentMin, (long)m_planetMoonMinMaxSlider.CurrentMax);

            settings.GeneratorSettings.GPSSettings.PlanetGPSMode = (MyGPSGenerationMode)m_planetGPSModeCombo.GetSelectedKey();
            settings.GeneratorSettings.GPSSettings.MoonGPSMode = (MyGPSGenerationMode)m_moonGPSModeCombo.GetSelectedKey();
            settings.GeneratorSettings.GPSSettings.AsteroidGPSMode = (MyGPSGenerationMode)m_asteroidGPSModeCombo.GetSelectedKey();

            return settings;
        }

        public override string GetFriendlyName()
        {
            return "MyPluginSettingsMenu";
        }
    }
}
