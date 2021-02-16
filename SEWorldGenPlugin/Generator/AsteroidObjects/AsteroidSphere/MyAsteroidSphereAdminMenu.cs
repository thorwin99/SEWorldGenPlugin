using Sandbox.Definitions;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// Admin menu creator class for asteroid hollow sphere
    /// </summary>
    public class MyAsteroidSphereAdminMenu : IMyAsteroidAdminMenuCreator
    {
        /// <summary>
        /// Id for the render preview
        /// </summary>
        private static int PREVIEW_RENDER_ID = 1467;

        /// <summary>
        /// The current star system existend on the server used for spawning asteroid hollow spheres around planets
        /// </summary>
        private MyObjectBuilder_SystemData m_fetchedStarSytem;

        /// <summary>
        /// The listbox containing all possible parent system objects
        /// </summary>
        private MyGuiControlListbox m_parentObjectListBox;

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

        /// <summary>
        /// The button to spawn the sphere
        /// </summary>
        private MyGuiControlButton m_spawnButton;

        /// <summary>
        /// The textbox for the asteroid sphere name in the spawn menu
        /// </summary>
        private MyGuiControlTextbox m_nameBox;

        /// <summary>
        /// The admin menu, that uses this creator.
        /// </summary>
        private MyPluginAdminMenu m_parentScreen;

        public bool CreateEditMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen, MySystemAsteroids asteroidObject)
        {
            return false;
        }

        public bool CreateSpawnMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen)
        {
            m_parentScreen = adminScreen;

            if (m_fetchedStarSytem == null)
            {
                MyGuiControlRotatingWheel loadingWheel = new MyGuiControlRotatingWheel(position: Vector2.Zero);
                loadingWheel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;

                adminScreen.Controls.Add(loadingWheel);

                MyStarSystemGenerator.Static.GetStarSystem(delegate (MyObjectBuilder_SystemData starSystem)
                {
                    m_fetchedStarSytem = starSystem;
                    adminScreen.ShouldRecreate = true;
                });
                return true;
            }

            MyGuiControlLabel label = new MyGuiControlLabel(null, null, "Parent objects");

            parentTable.AddTableRow(label);

            m_parentObjectListBox = new MyGuiControlListbox();
            m_parentObjectListBox.Add(new MyGuiControlListbox.Item(new System.Text.StringBuilder("System center"), userData: m_fetchedStarSytem.CenterObject));
            m_parentObjectListBox.VisibleRowsCount = 8;
            m_parentObjectListBox.Size = new Vector2(usableWidth, m_parentObjectListBox.Size.Y);
            m_parentObjectListBox.SelectAllVisible();
            m_parentObjectListBox.ItemsSelected += OnParentItemClicked;

            foreach (var obj in m_fetchedStarSytem.CenterObject.GetAllChildren())
            {
                if (obj.Type == MySystemObjectType.PLANET || obj.Type == MySystemObjectType.MOON)
                {
                    m_parentObjectListBox.Add(new MyGuiControlListbox.Item(new System.Text.StringBuilder(obj.DisplayName), userData: obj));
                }
            }

            parentTable.AddTableRow(m_parentObjectListBox);

            parentTable.AddTableSeparator();

            m_radiusSlider = new MyGuiControlClickableSlider(width: usableWidth - 0.1f, minValue: 0, maxValue: 1, labelSuffix: " km", showLabel: true);
            m_radiusSlider.Enabled = false;
            m_radiusSlider.ValueChanged += delegate
            {
                RenderSpherePreview();
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Radius"));
            parentTable.AddTableRow(m_radiusSlider);

            m_widthSlider = new MyGuiControlClickableSlider(null, 0, 1, usableWidth - 0.1f, 0.5f, showLabel: true, labelSuffix: " km");
            m_widthSlider.Enabled = false;
            m_widthSlider.ValueChanged += delegate
            {
                RenderSpherePreview();
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Width"));
            parentTable.AddTableRow(m_widthSlider);

            m_asteroidSizesSlider = new MyGuiControlRangedSlider(32, 1024, 32, 1024, true, width: usableWidth - 0.1f, showLabel: true);
            m_asteroidSizesSlider.Enabled = false;

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Asteroid size range"));
            parentTable.AddTableRow(m_asteroidSizesSlider);

            m_nameBox = new MyGuiControlTextbox();
            m_nameBox.Size = new Vector2(usableWidth, m_nameBox.Size.Y);

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Name"));
            parentTable.AddTableRow(m_nameBox);

            m_spawnButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Spawn sphere", delegate
            {
                StringBuilder name = new StringBuilder();
                m_nameBox.GetText(name);
                if (name.Length < 4)
                {
                    MyPluginGuiHelper.DisplayError("Name must be at least 4 letters long", "Error");
                    return;
                }

                MySystemAsteroids instance;
                MyAsteroidSphereData data;
                GenerateData(out instance, out data);

                if(instance == null || data == null)
                {
                    MyPluginGuiHelper.DisplayError("Could not generate asteroid sphere. No data found.", "Error");
                    return;
                }

                MyAsteroidSphereProvider.Static.AddInstance(instance, data, delegate (bool success)
                {
                    if (!success)
                    {
                        MyPluginGuiHelper.DisplayError("Sphere could not be added, because an object with the same id already exists. This error should not occour, so please try again.", "Error");
                    }
                    else
                    {
                        MyPluginGuiHelper.DisplayMessage("Sphere was created successfully.", "Success");
                    }
                });

            });

            parentTable.AddTableSeparator();

            parentTable.AddTableRow(m_spawnButton);

            return true;
        }

        public void OnAdminMenuClose()
        {
            m_fetchedStarSytem = null;
            MyPluginDrawSession.Static.RemoveRenderObject(PREVIEW_RENDER_ID);
        }

        private void GenerateData(out MySystemAsteroids instance, out MyAsteroidSphereData data)
        {
            if (m_parentObjectListBox.SelectedItems.Count <= 0)
            {
                instance = null;
                data = null;
                return;
            }
            var selectedParent = m_parentObjectListBox.SelectedItems[m_parentObjectListBox.SelectedItems.Count - 1];
            var parentItem = selectedParent.UserData as MySystemObject;
            StringBuilder name = new StringBuilder();
            m_nameBox.GetText(name);

            instance = new MySystemAsteroids();
            instance.AsteroidTypeName = MyAsteroidSphereProvider.TYPE_NAME;
            instance.CenterPosition = parentItem.CenterPosition;
            instance.AsteroidSize = new MySerializableMinMax((int)m_asteroidSizesSlider.CurrentMin, (int)m_asteroidSizesSlider.CurrentMax);
            instance.ParentId = parentItem.Id;
            instance.DisplayName = name.ToString();

            data = GetDataFromGui();
        }

        /// <summary>
        /// Runs on click of the parent item box. Sets the ranges for the sliders and resets the values.
        /// </summary>
        /// <param name="box">The listbox which calls this method on item clicked</param>
        private void OnParentItemClicked(MyGuiControlListbox box)
        {
            if (box.SelectedItems.Count > 0)
            {
                m_spawnButton.Enabled = true;
                m_radiusSlider.Enabled = true;
                m_widthSlider.Enabled = true;
                m_asteroidSizesSlider.Enabled = true;
                m_nameBox.Enabled = true;

                var parent = box.SelectedItems[box.SelectedItems.Count - 1].UserData as MySystemObject;
                var settings = MySettingsSession.Static.Settings.GeneratorSettings;

                if(parent == m_fetchedStarSytem.CenterObject)
                {
                    m_radiusSlider.MinValue = settings.MinMaxOrbitDistance.Min / 1000;
                    m_radiusSlider.MaxValue = settings.WorldSize < 0 ? int.MaxValue / 1000 : settings.WorldSize / 1000;
                    m_radiusSlider.Value = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;

                    m_widthSlider.MinValue = settings.MinMaxOrbitDistance.Min / 2000;
                    m_widthSlider.MaxValue = settings.MinMaxOrbitDistance.Min / 1000;
                    m_widthSlider.Value = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;

                    m_asteroidSizesSlider.SetValues(32, 1024);

                    CameraLookAt(Vector3D.Zero, new Vector3D(0, 0, m_radiusSlider.Value * 2000));

                    RenderSpherePreview();
                    return;
                }
                else
                {
                    if (parent.Type == MySystemObjectType.PLANET || parent.Type == MySystemObjectType.MOON)
                    {
                        var planet = parent as MySystemPlanet;
                        List<MyEntityList.MyEntityListInfoItem> planets = MyEntityList.GetEntityList(MyEntityList.MyEntityTypeEnum.Planets);
                        MyPlanet planetEntity = null;
                        foreach(var p in planets)
                        {
                            var e = MyEntities.GetEntityById(p.EntityId) as MyPlanet;
                            if(e.EntityId == planet.EntityId)
                            {
                                planetEntity = MyEntities.GetEntityById(p.EntityId) as MyPlanet;
                            }
                        }

                        if (planetEntity != null)
                        {
                            m_radiusSlider.MinValue = planetEntity.AverageRadius / 1000;
                            m_radiusSlider.MaxValue = (float)planet.Diameter / 1000;
                            m_radiusSlider.Value = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;

                            m_widthSlider.MinValue = (planetEntity.MaximumRadius - planetEntity.MinimumRadius) / 1000;
                            m_widthSlider.MaxValue = (float)planet.Diameter / 5000f;
                            m_widthSlider.Value = m_widthSlider.MinValue + (m_widthSlider.MaxValue - m_widthSlider.MinValue) / 2;

                            m_asteroidSizesSlider.SetValues(32, 1024);

                            CameraLookAt(planet.CenterPosition, (float)planet.Diameter * 2f);

                            RenderSpherePreview();
                            return;
                        }
                    }
                }
            }
            m_radiusSlider.Enabled = false;
            m_asteroidSizesSlider.Enabled = false;
            m_nameBox.Enabled = false;
            m_spawnButton.Enabled = false;
            m_widthSlider.Enabled = false;
        }

        /// <summary>
        /// Generates a MyAsteroidSphereData object from the gui elements of the spawn menu
        /// </summary>
        /// <returns>The generated data</returns>
        private MyAsteroidSphereData GetDataFromGui()
        {
            if (m_parentObjectListBox.SelectedItems.Count <= 0) return null;
            var selected = m_parentObjectListBox.SelectedItems[m_parentObjectListBox.SelectedItems.Count - 1];
            MySystemObject parent = selected.UserData as MySystemObject;
            MyAsteroidSphereData data = new MyAsteroidSphereData();
            data.Center = parent.CenterPosition;
            data.InnerRadius = m_radiusSlider.Value * 1000;
            data.OuterRadius = m_radiusSlider.Value * 1000 + m_widthSlider.Value * 1000;

            return data;
        }

        /// <summary>
        /// Renders a preview of the hollow sphere currently edited
        /// </summary>
        private void RenderSpherePreview()
        {
            var data = GetDataFromGui();

            MyPluginDrawSession.Static.RemoveRenderObject(PREVIEW_RENDER_ID);

            if (data != null)
            {
                MyPluginDrawSession.Static.AddRenderObject(PREVIEW_RENDER_ID, new RenderHollowSphere(data.Center, (float)data.OuterRadius, (float)data.OuterRadius, Color.LightGreen.ToVector4(), (float)data.OuterRadius / 500f));
            }
        }

        /// <summary>
        /// Makes the spectator cam look at the specific point from the given distance
        /// </summary>
        /// <param name="center">Point to look at</param>
        /// <param name="distance">Distance to look from</param>
        private void CameraLookAt(Vector3D center, float distance)
        {
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
            MySpectatorCameraController.Static.Position = center + distance;
            MySpectatorCameraController.Static.Target = center;
        }

        /// <summary>
        /// Makes the spectator cam look at the specific point from the given distance
        /// </summary>
        /// <param name="center">Point to look at</param>
        /// <param name="distance">Distance to look from</param>
        private void CameraLookAt(Vector3D center, Vector3D distance)
        {
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
            MySpectatorCameraController.Static.Position = center + distance;
            MySpectatorCameraController.Static.Target = center;
        }
    }
}
