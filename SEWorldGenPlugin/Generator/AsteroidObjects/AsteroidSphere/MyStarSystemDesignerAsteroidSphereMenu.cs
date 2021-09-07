using Sandbox.Game.Entities;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// Star system designer edit menu for asteroid spheres
    /// </summary>
    public class MyStarSystemDesignerAsteroidSphereMenu : MyStarSystemDesignerAsteroidMenu
    {
        /// <summary>
        /// The slider for the asteroid sphere radius in the spawn menu
        /// </summary>
        private MyGuiControlClickableSlider m_radiusSlider;

        /// <summary>
        /// The slider for the asteroid sphere width in the spawn menu
        /// </summary>
        private MyGuiControlClickableSlider m_widthSlider;

        /// <summary>
        /// The slider for the asteroid ring asteroid size range in the spawn menu
        /// </summary>
        private MyGuiControlRangedSlider m_asteroidSizesSlider;

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
                Data = new MyAsteroidSphereData();
            }
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            m_radiusSlider = new MyGuiControlClickableSlider(width: maxWidth - 0.1f, minValue: 0, maxValue: 1, labelSuffix: " km", showLabel: true);

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Radius"));
            controlTable.AddTableRow(m_radiusSlider);

            m_widthSlider = new MyGuiControlClickableSlider(null, 0, 1, maxWidth - 0.1f, 0.5f, showLabel: true, labelSuffix: " km");

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Width"));
            controlTable.AddTableRow(m_widthSlider);

            m_asteroidSizesSlider = new MyGuiControlRangedSlider(32, 1024, 32, 1024, true, width: maxWidth - 0.1f, showLabel: true);

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Asteroid sizes"));
            controlTable.AddTableRow(m_asteroidSizesSlider);

            SetSliderValues();

            m_radiusSlider.ValueChanged += UpdateObjectData;
            m_widthSlider.ValueChanged += UpdateObjectData;
            m_asteroidSizesSlider.ValueChanged += UpdateObjectData;
        }

        private void UpdateObjectData(MyGuiControlBase control)
        {
            var data = Data as MyAsteroidSphereData;
            var roid = m_object as MySystemAsteroids;

            data.InnerRadius = m_radiusSlider.Value * 1000.0;
            data.OuterRadius = data.InnerRadius + m_widthSlider.Value * 1000.0;
            data.Center = roid.CenterPosition;

            roid.AsteroidSize.Min = (long)m_asteroidSizesSlider.CurrentMin;
            roid.AsteroidSize.Max = (long)m_asteroidSizesSlider.CurrentMax;

            ChangedObject();
        }

        private void SetSliderValues()
        {
            MySystemObject parent = MyStarSystemGenerator.Static.StarSystem.GetById(m_object.ParentId);
            var settings = MySettingsSession.Static.Settings.GeneratorSettings;
            var data = Data as MyAsteroidSphereData;
            var roid = m_object as MySystemAsteroids;

            if (parent == MyStarSystemGenerator.Static.StarSystem.CenterObject)
            {
                m_radiusSlider.MinValue = settings.MinMaxOrbitDistance.Min / 1000;
                m_radiusSlider.MaxValue = settings.WorldSize < 0 ? float.MaxValue / 1000 : settings.WorldSize / 1000;
                m_radiusSlider.Value = (float)(data.InnerRadius / 1000f);

                m_widthSlider.MinValue = settings.MinMaxOrbitDistance.Min / 2000;
                m_widthSlider.MaxValue = settings.MinMaxOrbitDistance.Min / 1000;
                m_widthSlider.Value = (float)((data.OuterRadius - data.InnerRadius) / 1000f);

                m_asteroidSizesSlider.SetValues(roid.AsteroidSize.Min, roid.AsteroidSize.Max);
                return;
            }
            else
            {
                if (parent.Type == MySystemObjectType.PLANET || parent.Type == MySystemObjectType.MOON)
                {
                    var planet = parent as MySystemPlanet;
                    var planetEntity = MyEntities.GetEntityById(planet.EntityId) as MyPlanet;

                    if (planetEntity != null)
                    {
                        m_radiusSlider.MinValue = planetEntity.AverageRadius / 1000;
                        m_radiusSlider.MaxValue = (float)planet.Diameter / 1000;
                        m_radiusSlider.Value = (float)(data.InnerRadius / 1000f);

                        m_widthSlider.MinValue = (planetEntity.MaximumRadius - planetEntity.MinimumRadius) / 1000;
                        m_widthSlider.MaxValue = (float)planet.Diameter / 5000f;
                        m_widthSlider.Value = (float)((data.OuterRadius - data.InnerRadius) / 1000f);

                        m_asteroidSizesSlider.SetValues(roid.AsteroidSize.Min, roid.AsteroidSize.Max);
                        return;
                    }
                }
            }
        }
    }
}
