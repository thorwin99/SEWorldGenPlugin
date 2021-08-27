using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing
{
    /// <summary>
    /// Star system designer sub menu for asteroid rings
    /// </summary>
    public class MyStarSystemDesignerAsteroidRingMenu : MyStarSystemDesignerAsteroidMenu
    {
        /// <summary>
        /// The slider for the asteroid ring radius in the spawn menu
        /// </summary>
        private MyGuiControlClickableSlider m_radiusSlider;

        /// <summary>
        /// The slider for the asteroid ring width in the spawn menu
        /// </summary>
        private MyGuiControlClickableSlider m_widthSlider;

        /// <summary>
        /// The slider for the asteroid ring height in the spawn menu
        /// </summary>
        private MyGuiControlClickableSlider m_heightSlider;

        /// <summary>
        /// The slider for the asteroid ring asteroid size range in the spawn menu
        /// </summary>
        private MyGuiControlRangedSlider m_asteroidSizesSlider;

        /// <summary>
        /// The slider for the asteroid ring x axis angle in the spawn menu
        /// </summary>
        private MyGuiControlClickableSlider m_angleXSlider;

        /// <summary>
        /// The slider for the asteroid ring y axis angle in the spawn menu
        /// </summary>
        private MyGuiControlClickableSlider m_angleYSlider;

        /// <summary>
        /// The slider for the asteroid ring z axis angle in the spawn menu
        /// </summary>
        private MyGuiControlClickableSlider m_angleZSlider;

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
            m_radiusSlider = new MyGuiControlClickableSlider(width: maxWidth - 0.1f, minValue: 0, maxValue: 1, labelSuffix: " km", showLabel: true);
            m_radiusSlider.ValueChanged += OnValueChanged;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Radius"));
            controlTable.AddTableRow(m_radiusSlider);

            m_widthSlider = new MyGuiControlClickableSlider(null, 0, 1, maxWidth - 0.1f, 0.5f, showLabel: true, labelSuffix: " km");
            m_widthSlider.ValueChanged += OnValueChanged;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Width"));
            controlTable.AddTableRow(m_widthSlider);

            m_heightSlider = new MyGuiControlClickableSlider(null, 0, 1, maxWidth - 0.1f, 0.5f, showLabel: true, labelSuffix: " km");
            m_heightSlider.ValueChanged += OnValueChanged;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Height"));
            controlTable.AddTableRow(m_heightSlider);

            m_asteroidSizesSlider = new MyGuiControlRangedSlider(32, 1024, 32, 1024, true, width: maxWidth - 0.1f, showLabel: true);
            m_asteroidSizesSlider.ValueChanged += OnValueChanged;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Asteroid sizes"));
            controlTable.AddTableRow(m_asteroidSizesSlider);

            m_angleXSlider = new MyGuiControlClickableSlider(null, -90, 90, maxWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "°");
            m_angleXSlider.ValueChanged += OnValueChanged;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Angle X"));
            controlTable.AddTableRow(m_angleXSlider);

            m_angleYSlider = new MyGuiControlClickableSlider(null, -90, 90, maxWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "°");
            m_angleYSlider.ValueChanged += OnValueChanged;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Angle Y"));
            controlTable.AddTableRow(m_angleYSlider);

            m_angleZSlider = new MyGuiControlClickableSlider(null, -90, 90, maxWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "°");
            m_angleZSlider.ValueChanged += OnValueChanged;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Angle Z"));
            controlTable.AddTableRow(m_angleZSlider);

            GetControlsFromRoid();
        }

        /// <summary>
        /// Retreives the control values from the edited asteroid
        /// </summary>
        private void GetControlsFromRoid()
        {
            var data = m_data as MyAsteroidRingData;
            var roid = m_object as MySystemAsteroids;
            var planet = MyStarSystemGenerator.Static.StarSystem.GetById(m_object.ParentId) as MySystemPlanet;

            if (planet == null)
            {
                var settings = MySettingsSession.Static.Settings.GeneratorSettings;

                m_radiusSlider.MinValue = settings.MinMaxOrbitDistance.Min / 1000;
                m_radiusSlider.MaxValue = settings.WorldSize < 0 ? int.MaxValue / 1000 : settings.WorldSize / 1000;
                m_radiusSlider.Value = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;
                m_radiusSlider.Enabled = true;

                m_widthSlider.MinValue = settings.MinMaxOrbitDistance.Min / 2000;
                m_widthSlider.MaxValue = settings.MinMaxOrbitDistance.Max / 1000;
                m_widthSlider.Value = m_widthSlider.MinValue + (m_widthSlider.MaxValue - m_widthSlider.MinValue) / 2;
                m_widthSlider.Enabled = true;

                m_heightSlider.MinValue = m_widthSlider.MinValue / 10;
                m_heightSlider.MaxValue = m_widthSlider.MaxValue / 10;
                m_heightSlider.Value = m_heightSlider.MinValue + (m_heightSlider.MaxValue - m_heightSlider.MinValue) / 2;
                m_heightSlider.Enabled = true;

                m_asteroidSizesSlider.Enabled = true;
                m_asteroidSizesSlider.SetValues(32, 1024);

                m_angleXSlider.Enabled = true;
                m_angleXSlider.Value = 0;
                m_angleYSlider.Enabled = true;
                m_angleYSlider.Value = 0;
                m_angleZSlider.Enabled = true;
                m_angleZSlider.Value = 0;

                return;
            }

            m_radiusSlider.MinValue = (int)planet.Diameter / 1000 * 0.75f;
            m_radiusSlider.MaxValue = (int)planet.Diameter / 1000 * 2f;
            m_radiusSlider.Value = (float)data.Radius / 1000;
            m_radiusSlider.Enabled = true;

            m_widthSlider.MinValue = (int)planet.Diameter / 1000 / 20f;
            m_widthSlider.MaxValue = (int)planet.Diameter / 1000 / 1.25f;
            m_widthSlider.Value = (float)data.Width / 1000;
            m_widthSlider.Enabled = true;

            m_heightSlider.MinValue = m_widthSlider.MinValue / 10;
            m_heightSlider.MaxValue = m_widthSlider.MaxValue / 10;
            m_heightSlider.Value = (float)data.Height / 1000;
            m_heightSlider.Enabled = true;

            m_asteroidSizesSlider.Enabled = true;
            m_asteroidSizesSlider.SetValues(roid.AsteroidSize.Min, roid.AsteroidSize.Max);

            m_angleXSlider.Enabled = true;
            m_angleXSlider.Value = (float)data.AngleDegrees.x;
            m_angleYSlider.Enabled = true;
            m_angleYSlider.Value = (float)data.AngleDegrees.y;
            m_angleZSlider.Enabled = true;
            m_angleZSlider.Value = (float)data.AngleDegrees.z;
        }

        /// <summary>
        /// Called when the given <paramref name="control"/> changes value
        /// </summary>
        /// <param name="control">Control that changed a value for a property of the asteroid ring</param>
        private void OnValueChanged(MyGuiControlBase control)
        {
            var data = m_data as MyAsteroidRingData;
            var roid = m_object as MySystemAsteroids;

            data.Radius = m_radiusSlider.Value * 1000f;
            data.Width = m_widthSlider.Value * 1000f;
            data.Height = m_heightSlider.Value * 1000f;
            data.AngleDegrees = new Vector3D(m_angleXSlider.Value, m_angleYSlider.Value, m_angleZSlider.Value);
            data.CenterPosition = roid.CenterPosition;

            roid.AsteroidSize.Min = (long)m_asteroidSizesSlider.CurrentMin;
            roid.AsteroidSize.Max = (long)m_asteroidSizesSlider.CurrentMax;

            ChangedObject();
        }
    }
}
