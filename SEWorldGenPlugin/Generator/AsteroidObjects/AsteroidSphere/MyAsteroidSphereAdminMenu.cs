using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
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

        /// <summary>
        /// The currently selected asteroid
        /// </summary>
        private MySystemAsteroids m_currentSelectedAsteroid;

        public bool OnEditMenuSelectItem(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen, MySystemAsteroids asteroidObject, MyObjectBuilder_SystemData starSystem)
        {
            m_parentScreen = adminScreen;

            m_currentSelectedAsteroid = asteroidObject;

            MyGuiControlButton teleportToRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Teleport to sphere", OnTeleportToSphere);

            parentTable.AddTableRow(teleportToRingButton);

            MyGuiControlButton deleteRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Remove sphere", OnRemoveSphere);

            parentTable.AddTableRow(deleteRingButton);

            parentTable.AddTableSeparator();

            var data = MyAsteroidSphereProvider.Static.GetInstanceData(asteroidObject.Id);
            var sphere = data as MyAsteroidSphereData;

            m_parentScreen.CameraLookAt(asteroidObject.CenterPosition, (float)sphere.OuterRadius * 2f);
            RenderSpherePreview(sphere);
            return true;
        }

        public bool CreateSpawnMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen)
        {
            m_parentScreen = adminScreen;

            MyGuiControlLabel label = new MyGuiControlLabel(null, null, "Parent objects");

            parentTable.AddTableRow(label);

            m_parentObjectListBox = new MyGuiControlListbox();
            m_parentObjectListBox.Add(new MyGuiControlListbox.Item(new System.Text.StringBuilder("System center"), userData: MyStarSystemGenerator.Static.StarSystem.CenterObject));
            m_parentObjectListBox.VisibleRowsCount = 8;
            m_parentObjectListBox.Size = new Vector2(usableWidth, m_parentObjectListBox.Size.Y);
            m_parentObjectListBox.SelectAllVisible();
            m_parentObjectListBox.ItemsSelected += OnParentItemClicked;

            foreach (var obj in MyStarSystemGenerator.Static.StarSystem.CenterObject.GetAllChildren())
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
                RenderSpherePreview(GetDataFromGui());
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Radius"));
            parentTable.AddTableRow(m_radiusSlider);

            m_widthSlider = new MyGuiControlClickableSlider(null, 0, 1, usableWidth - 0.1f, 0.5f, showLabel: true, labelSuffix: " km");
            m_widthSlider.Enabled = false;
            m_widthSlider.ValueChanged += delegate
            {
                RenderSpherePreview(GetDataFromGui());
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

                MyAsteroidSphereProvider.Static.AddInstance(instance, data);

            });

            parentTable.AddTableSeparator();

            parentTable.AddTableRow(m_spawnButton);

            return true;
        }

        public void Close()
        {
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

                if(parent == MyStarSystemGenerator.Static.StarSystem.CenterObject)
                {
                    m_radiusSlider.MinValue = settings.MinMaxOrbitDistance.Min / 1000;
                    m_radiusSlider.MaxValue = settings.WorldSize < 0 ? int.MaxValue / 1000 : settings.WorldSize / 1000;
                    m_radiusSlider.Value = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;

                    m_widthSlider.MinValue = settings.MinMaxOrbitDistance.Min / 2000;
                    m_widthSlider.MaxValue = settings.MinMaxOrbitDistance.Min / 1000;
                    m_widthSlider.Value = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;

                    m_asteroidSizesSlider.SetValues(32, 1024);

                    m_parentScreen.CameraLookAt(Vector3D.Zero, new Vector3D(0, 0, m_radiusSlider.Value * 2000));

                    RenderSpherePreview(GetDataFromGui());
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

                            m_parentScreen.CameraLookAt(planet.CenterPosition, (float)planet.Diameter * 2f);

                            RenderSpherePreview(GetDataFromGui());
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
        private void RenderSpherePreview(MyAsteroidSphereData data)
        {
            MyPluginDrawSession.Static.RemoveRenderObject(PREVIEW_RENDER_ID);

            if (data != null)
            {
                MyPluginDrawSession.Static.AddRenderObject(PREVIEW_RENDER_ID, new RenderHollowSphere(data.Center, (float)data.OuterRadius, (float)data.OuterRadius, Color.LightGreen.ToVector4(), (float)data.OuterRadius / 500f));
            }
        }

        /// <summary>
        /// Action to remove the sphere for the remove sphere button
        /// </summary>
        /// <param name="button">Button that called this action</param>
        private void OnRemoveSphere(MyGuiControlButton button)
        {
            MyPluginLog.Debug("Removing sphere " + m_currentSelectedAsteroid.DisplayName);

            MyStarSystemGenerator.Static.RemoveObjectFromSystem(m_currentSelectedAsteroid.Id);
        }

        /// <summary>
        /// Teleports the player to the selected ring
        /// </summary>
        /// <param name="button">Button to call</param>
        private void OnTeleportToSphere(MyGuiControlButton button)
        {
            MyPluginLog.Debug("Teleporting player to " + m_currentSelectedAsteroid.DisplayName);

            if (MySession.Static.CameraController != MySession.Static.LocalCharacter || true)
            {
                if (m_currentSelectedAsteroid != null)
                {
                    IMyAsteroidObjectShape shape = MyAsteroidSphereProvider.Static.GetAsteroidObjectShape(m_currentSelectedAsteroid);
                    if (shape == null)
                    {
                        MyPluginGuiHelper.DisplayError("Cant teleport to asteroid sphere. It does not exist", "Error");
                        return;
                    }

                    m_parentScreen.CloseScreenNow();

                    MyMultiplayer.TeleportControlledEntity(shape.GetPointInShape());
                    MyGuiScreenGamePlay.SetCameraController();
                }
            }
        }

        public void CreateDataEditMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MySystemAsteroids selectedInstance)
        {
        }
    }
}
