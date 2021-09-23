using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    public class MyStarSystemDesignerOrbitMenu : MyStarSystemDesignerObjectMenu
    {
        /// <summary>
        /// A textbox to enter the orbit radius in km
        /// </summary>
        protected MyGuiControlClickableSlider m_orbitRadiusSlider;

        /// <summary>
        /// A slider for the position of the planet on the orbit, between 0 and 360 degrees.
        /// </summary>
        protected MyGuiControlClickableSlider m_orbitPosSlider;

        /// <summary>
        /// The slider for the elevation of the planet over the system plane, between -90 and 90 degrees.
        /// </summary>
        protected MyGuiControlClickableSlider m_elevationSldier;

        public MyStarSystemDesignerOrbitMenu(MySystemObject obj) : base(obj)
        {
            CanAddChild = true;
            CanBeRemoved = true;
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            m_orbitRadiusSlider = new MyGuiControlClickableSlider(width: maxWidth - 0.1f, minValue: (float)CalculateMinOrbitRadius(), maxValue: (float)CalculateMaxOrbitRadius(), labelSuffix: " km", showLabel: true);
            m_orbitRadiusSlider.SetToolTip(new MyToolTips("The radius of the orbit. Its the distance of the objects center to the system center."));

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Orbit radius"));
            controlTable.AddTableRow(m_orbitRadiusSlider);

            m_orbitPosSlider = new MyGuiControlClickableSlider(null, 0f, 360f, maxWidth - 0.1f, 0f, showLabel: true, labelSuffix: "°");
            m_orbitPosSlider.SetToolTip(new MyToolTips("The rotation of the orbit itself around the parent."));

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Orbit Rotation"));
            controlTable.AddTableRow(m_orbitPosSlider);

            m_elevationSldier = new MyGuiControlClickableSlider(null, -90f, 90f, maxWidth - 0.1f, 0f, showLabel: true, labelSuffix: "°");
            m_elevationSldier.SetToolTip(new MyToolTips("The elevation of the orbit above the system plane."));

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Elevation degrees"));
            controlTable.AddTableRow(m_elevationSldier);

            SetOrbitProperties();

            m_elevationSldier.ValueChanged += (s) =>
            {
                GetPropertiesFromOrbit();
                ChangedObject();
            };

            m_orbitPosSlider.ValueChanged += (s) =>
            {
                GetPropertiesFromOrbit();
                ChangedObject();
            };

            m_orbitRadiusSlider.ValueChanged += (s) =>
            {
                GetPropertiesFromOrbit();
                ChangedObject();
            };

            if (isEditing)
            {
                m_orbitRadiusSlider.Enabled = false;
                m_orbitPosSlider.Enabled = false;
                m_elevationSldier.Enabled = false;
            }


            var parent = MyStarSystemGenerator.Static.StarSystem.GetById(m_object.ParentId);
            if (parent == null)
            {
                m_orbitRadiusSlider.Enabled = false;
                m_elevationSldier.Enabled = false;
                m_orbitPosSlider.Enabled = false;
            }

            GetPropertiesFromOrbit();
        }

        /// <summary>
        /// Calculates the largest orbit radius possible for this planet
        /// </summary>
        private double CalculateMaxOrbitRadius()
        {
            var center = MyStarSystemGenerator.Static.StarSystem.CenterObject;
            if (center != null && m_object.ParentId == center.Id)
            {
                int count = center.ChildObjects.Count + 1;
                return count * MySettingsSession.Static.Settings.GeneratorSettings.MinMaxOrbitDistance.Max / 1000;
            }
            else
            {
                MySystemPlanet p = MyStarSystemGenerator.Static.StarSystem.GetById(m_object.ParentId) as MySystemPlanet;

                if(p == null)
                {
                    return MySettingsSession.Static.Settings.GeneratorSettings.MinMaxOrbitDistance.Min / 1000.0;
                }
                else
                {
                    return p.Diameter / 2000.0 + MySettingsSession.Static.Settings.GeneratorSettings.MinMaxOrbitDistance.Min / 1000.0;
                }
            }
        }

        /// <summary>
        /// Calculates the min orbit radius of the object around its parent
        /// </summary>
        /// <returns>The min orbit radius</returns>
        private double CalculateMinOrbitRadius()
        {
            var center = MyStarSystemGenerator.Static.StarSystem.CenterObject;
            if (center != null && m_object.ParentId == center.Id)
            {
                return 0.0;
            }
            else
            {
                MySystemPlanet p = MyStarSystemGenerator.Static.StarSystem.GetById(m_object.ParentId) as MySystemPlanet;

                if (p == null)
                {
                    return 0.0;
                }
                else
                {
                    return p.Diameter / 2000.0;
                }
            }
        }

        /// <summary>
        /// Sets the sliders for orbit radius, orbit position and elevation from the edited object.
        /// </summary>
        protected void SetOrbitProperties()
        {
            MySystemObject parent = MyStarSystemGenerator.Static.StarSystem.GetById(m_object.ParentId);
            double radius = 0;
            Vector3D parentRel = Vector3D.Zero;

            if (parent != null)
            {
                parentRel = new Vector3D(m_object.CenterPosition) - new Vector3D(parent.CenterPosition);
                radius = parentRel.Length();
            }

            if (radius == 0)
            {
                m_orbitRadiusSlider.Value = 0;
                m_elevationSldier.Value = 0;
                m_orbitPosSlider.Value = 0;
                return;
            }

            double elevation = MathHelperD.ToDegrees(Math.Asin(parentRel.Z / radius));
            double orbitPos = MathHelperD.ToDegrees(Math.Acos(parentRel.X / Math.Cos(MathHelperD.ToRadians(elevation)) / radius));
            if (parentRel.Y < 0)
            {
                orbitPos = 360 - orbitPos;
            }

            m_orbitRadiusSlider.Value = (float)radius / 1000f;
            m_elevationSldier.Value = (float)elevation;
            m_orbitPosSlider.Value = (float)orbitPos;
        }

        /// <summary>
        /// Sets the m_object properties based on the current values for the orbit radius, orbit position and elevation controls
        /// </summary>
        protected void GetPropertiesFromOrbit()
        {
            MySystemObject parent = MyStarSystemGenerator.Static.StarSystem.GetById(m_object.ParentId);

            double radius = m_orbitRadiusSlider.Value * 1000f;
            double elevation = MathHelperD.ToRadians(m_elevationSldier.Value);
            double orbitPos = MathHelperD.ToRadians(m_orbitPosSlider.Value);

            if (radius == 0)
            {
                if (parent == null)
                {
                    m_object.CenterPosition = Vector3D.Zero;
                }
                else
                {
                    m_object.CenterPosition = parent.CenterPosition;
                }

                return;
            }

            Vector3D pos = new Vector3D(radius * Math.Cos(orbitPos) * Math.Cos(elevation), radius * Math.Sin(orbitPos) * Math.Cos(elevation), radius * Math.Sin(elevation));

            if (parent != null)
                pos += parent.CenterPosition;

            m_object.CenterPosition = pos;
        }
    }
}
