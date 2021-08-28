/*using Sandbox.Engine.Multiplayer;
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
using System.Text;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing
{
    public class MyAsteroidRingAdminMenu : IMyAsteroidAdminMenuCreator
    {
        /// <summary>
        /// Id for the render preview
        /// </summary>
        private static int PREVIEW_RENDER_ID = 1425;

        /// <summary>
        /// The listbox containing all possible parent system objects
        /// </summary>
        private MyGuiControlListbox m_parentObjectListBox;

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

        /// <summary>
        /// The textbox for the asteroid ring name in the spawn menu
        /// </summary>
        private MyGuiControlTextbox m_nameBox;

        /// <summary>
        /// Button to spawn the ring
        /// </summary>
        private MyGuiControlButton m_spawnRingButton;

        /// <summary>
        /// Checkbox, to disable or enable snap to parent
        /// </summary>
        private MyGuiControlCheckbox m_snapToParentCheck;

        /// <summary>
        /// The currently selected asteroid ring / belt in the edit menu
        /// </summary>
        private MySystemAsteroids m_currentSelectedAsteroid;

        /// <summary>
        /// Open the offset dialog in spawn menu
        /// </summary>
        private MyGuiControlButton m_offsetToCoordButton;

        /// <summary>
        /// Button to zoom onto the ring
        /// </summary>
        private MyGuiControlButton m_zoomInButton;

        /// <summary>
        /// The offset of the center when spawning a ring
        /// </summary>
        private Vector3D m_offset = Vector3D.Zero;

        /// <summary>
        /// If the camera should snap to the parent object in the spawn
        /// menu
        /// </summary>
        private bool m_snapToParent = true;

        /// <summary>
        /// The admin menu instance, that used this class to generate the menu
        /// </summary>
        private MyPluginAdminMenu m_parentScreen;

        public bool OnEditMenuSelectItem(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen, MySystemAsteroids asteroidObject, MyObjectBuilder_SystemData starSystem)
        {
            m_parentScreen = adminScreen;

            m_currentSelectedAsteroid = asteroidObject;

            m_offset = Vector3D.Zero;

            GenerateRingSettingElements(usableWidth, parentTable);
            SetSliderValues(m_currentSelectedAsteroid);

            MyGuiControlButton teleportToRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Teleport to ring", OnTeleportToRing);

            parentTable.AddTableRow(teleportToRingButton);

            MyGuiControlButton deleteRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Remove ring", OnRemoveRing);

            parentTable.AddTableRow(deleteRingButton);

            MyGuiControlButton editRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Edit ring", OnEditRing);
            parentTable.AddTableRow(editRingButton);

            parentTable.AddTableSeparator();

            var data = MyAsteroidRingProvider.Static.GetInstanceData(asteroidObject.Id);
            var ring = data as MyAsteroidRingData;

            m_parentScreen.CameraLookAt(asteroidObject.CenterPosition, (float)ring.Radius * 1.5f);

            UpdateRingVisual(ring);

            return true;
        }

        public bool CreateSpawnMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen)
        {
            m_parentScreen = adminScreen;
            m_offset = Vector3D.Zero;
            m_currentSelectedAsteroid = null;

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

            var row = new MyGuiControlParentTableLayout(2, false, Vector2.Zero);

            m_snapToParentCheck = new MyGuiControlCheckbox();
            m_snapToParentCheck.IsChecked = m_snapToParent;
            m_snapToParentCheck.IsCheckedChanged += delegate
            {
                m_snapToParent = m_snapToParentCheck.IsChecked;
                m_zoomInButton.Enabled = m_snapToParent;
            };

            row.AddTableRow(m_snapToParentCheck, new MyGuiControlLabel(null, null, "Snap camera to parent"));

            parentTable.AddTableRow(row);

            parentTable.AddTableSeparator();

            GenerateRingSettingElements(usableWidth, parentTable);

            m_nameBox = new MyGuiControlTextbox();
            m_nameBox.Size = new Vector2(usableWidth, m_nameBox.Size.Y);

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Name"));
            parentTable.AddTableRow(m_nameBox);

            m_zoomInButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Zoom to ring", delegate
            {
                if (m_snapToParent)
                    m_parentScreen.CameraLookAt(GenerateAsteroidRing().CenterPosition, new Vector3D(0, 0, (m_radiusSlider.Value + m_widthSlider.Value) * 2000));
            });

            parentTable.AddTableRow(m_zoomInButton);

            m_offsetToCoordButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Offset to coordinate", delegate
            {
                var coordMessage = new MyGuiScreenDialogCoordinate("Enter coordinate to offset the center of the ring from its parent");
                coordMessage.OnConfirmed += delegate (Vector3D coord)
                {
                    m_offset = coord;
                    UpdateRingVisual(GenerateAsteroidRing());
                };
                MyGuiSandbox.AddScreen(coordMessage);
            });

            parentTable.AddTableRow(m_offsetToCoordButton);

            m_spawnRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Add ring", delegate
            {
                StringBuilder name = new StringBuilder();
                m_nameBox.GetText(name);
                if (name.Length < 4)
                {
                    MyPluginGuiHelper.DisplayError("Name must be at least 4 letters long", "Error");
                    return;
                }

                MySystemAsteroids instance;
                MyAsteroidRingData ring;
                GenerateAsteroidData(out ring, out instance);

                if (ring == null || instance == null)
                {
                    MyPluginGuiHelper.DisplayError("Could not generate asteroid ring. No data found.", "Error");
                    return;
                }

                MyAsteroidRingProvider.Static.AddInstance(instance, ring);
            });
            parentTable.AddTableRow(m_spawnRingButton);

            return true;
        }

        /// <summary>
        /// Callback when edit ring button is pressed
        /// </summary>
        /// <param name="button">Button to press</param>
        private void OnEditRing(MyGuiControlButton button)
        {
            var data = GetAsteroidDataFromGui();

            MyAsteroidRingProvider.Static.SetInstanceData(m_currentSelectedAsteroid.Id, data);

            MyPluginGuiHelper.DisplayMessage("The ring was updated", "Message");
        }

        /// <summary>
        /// Generates the specific gui elements to set ring data and puts them into the parent table
        /// </summary>
        /// <param name="usableWidth">Usable width for gui elements</param>
        /// <param name="parentTable">Parent table</param>
        private void GenerateRingSettingElements(float usableWidth, MyGuiControlParentTableLayout parentTable)
        {
            m_radiusSlider = new MyGuiControlClickableSlider(width: usableWidth - 0.1f, minValue: 0, maxValue: 1, labelSuffix: " km", showLabel: true);
            m_radiusSlider.Enabled = false;
            m_radiusSlider.ValueChanged += delegate
            {
                UpdateRingVisual(GetAsteroidDataFromGui());
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Radius"));
            parentTable.AddTableRow(m_radiusSlider);

            m_widthSlider = new MyGuiControlClickableSlider(null, 0, 1, usableWidth - 0.1f, 0.5f, showLabel: true, labelSuffix: " km");
            m_widthSlider.Enabled = false;
            m_widthSlider.ValueChanged += delegate
            {
                UpdateRingVisual(GetAsteroidDataFromGui());
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Width"));
            parentTable.AddTableRow(m_widthSlider);

            m_heightSlider = new MyGuiControlClickableSlider(null, 0, 1, usableWidth - 0.1f, 0.5f, showLabel: true, labelSuffix: " km");
            m_heightSlider.Enabled = false;
            m_heightSlider.ValueChanged += delegate
            {
                UpdateRingVisual(GetAsteroidDataFromGui());
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Height"));
            parentTable.AddTableRow(m_heightSlider);

            m_asteroidSizesSlider = new MyGuiControlRangedSlider(32, 1024, 32, 1024, true, width: usableWidth - 0.1f, showLabel: true);
            m_asteroidSizesSlider.Enabled = false;

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Asteroid size range"));
            parentTable.AddTableRow(m_asteroidSizesSlider);

            m_angleXSlider = new MyGuiControlClickableSlider(null, -90, 90, usableWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "°");
            m_angleXSlider.Enabled = false;
            m_angleXSlider.ValueChanged += delegate
            {
                UpdateRingVisual(GetAsteroidDataFromGui());
            };

            m_angleYSlider = new MyGuiControlClickableSlider(null, -90, 90, usableWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "°");
            m_angleYSlider.Enabled = false;
            m_angleYSlider.ValueChanged += delegate
            {
                UpdateRingVisual(GetAsteroidDataFromGui());
            };

            m_angleZSlider = new MyGuiControlClickableSlider(null, -90, 90, usableWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "°");
            m_angleZSlider.Enabled = false;
            m_angleZSlider.ValueChanged += delegate
            {
                UpdateRingVisual(GetAsteroidDataFromGui());
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Angle X Axis"));
            parentTable.AddTableRow(m_angleXSlider);

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Angle Y Axis"));
            parentTable.AddTableRow(m_angleYSlider);

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Angle Z Axis"));
            parentTable.AddTableRow(m_angleZSlider);
        }

        public void Close()
        {
            m_parentScreen = null;
            m_currentSelectedAsteroid = null;
            MyPluginDrawSession.Static.RemoveRenderObject(PREVIEW_RENDER_ID);
        }

        /// <summary>
        /// Updates the visual representaion of the current ring edited in the spawn menu
        /// </summary>
        private void UpdateRingVisual(MyAsteroidRingData ring)
        {
            if (ring == null) return;

            var shape = MyAsteroidObjectShapeRing.CreateFromRingItem(ring);

            MyPluginDrawSession.Static.RemoveRenderObject(PREVIEW_RENDER_ID);

            MyPluginDrawSession.Static.AddRenderObject(PREVIEW_RENDER_ID, new RenderHollowCylinder(shape.worldMatrix, (float)shape.radius + (float)shape.width, (float)shape.radius, (float)shape.height, Color.LightGreen.ToVector4(), (float)shape.radius / 200f));
        }

        /// <summary>
        /// Action to remove the ring for the remove ring button
        /// </summary>
        /// <param name="button">Button that called this action</param>
        private void OnRemoveRing(MyGuiControlButton button)
        {
            MyPluginLog.Debug("Removing ring " + m_currentSelectedAsteroid.DisplayName);

            MyStarSystemGenerator.Static.RemoveObjectFromSystem(m_currentSelectedAsteroid.Id);
        }

        /// <summary>
        /// Teleports the player to the selected ring
        /// </summary>
        /// <param name="button">Button to call</param>
        private void OnTeleportToRing(MyGuiControlButton button)
        {
            MyPluginLog.Debug("Teleporting player to " + m_currentSelectedAsteroid.DisplayName);

            if (MySession.Static.CameraController != MySession.Static.LocalCharacter || true)
            {
                if (m_currentSelectedAsteroid != null)
                {
                    IMyAsteroidObjectShape shape = MyAsteroidRingProvider.Static.GetAsteroidObjectShape(m_currentSelectedAsteroid);
                    if(shape == null) 
                    {
                        MyPluginGuiHelper.DisplayError("Cant teleport to asteroid ring. It does not exist", "Error");
                        return;
                    }

                    m_parentScreen.CloseScreenNow();

                    MyMultiplayer.TeleportControlledEntity(shape.GetPointInShape());
                    MyGuiScreenGamePlay.SetCameraController();
                }
            }
        }

        /// <summary>
        /// Generates the whole data for the currently edited asteroid ring from the values in the spawn menu
        /// </summary>
        /// <param name="ringData">The out value for the ring data</param>
        /// <param name="systemObject">The out value for the system object</param>
        private void GenerateAsteroidData(out MyAsteroidRingData ringData, out MySystemAsteroids systemObject)
        {
            if (m_parentObjectListBox.SelectedItems.Count <= 0)
            {
                ringData = null;
                systemObject = null;
                return;
            }
            var selectedParent = m_parentObjectListBox.SelectedItems[m_parentObjectListBox.SelectedItems.Count - 1];
            var parentItem = selectedParent.UserData as MySystemObject;
            StringBuilder name = new StringBuilder();
            m_nameBox.GetText(name);

            systemObject = new MySystemAsteroids();
            systemObject.AsteroidTypeName = MyAsteroidRingProvider.Static.GetTypeName();
            systemObject.CenterPosition = parentItem.CenterPosition + m_offset;
            systemObject.AsteroidSize = new MySerializableMinMax((int)m_asteroidSizesSlider.CurrentMin, (int)m_asteroidSizesSlider.CurrentMax);
            systemObject.DisplayName = name.ToString();
            systemObject.ParentId = parentItem.Id;

            ringData = GenerateAsteroidRing();
        }

        /// <summary>
        /// Returns the Asteroid data generated from the gui elements.
        /// </summary>
        /// <returns>The asteroid ring data generated</returns>
        private MyAsteroidRingData GetAsteroidDataFromGui()
        {
            Vector3D center;

            if(m_currentSelectedAsteroid == null)
            {
                if (m_parentObjectListBox.SelectedItems.Count <= 0) return null;
                var selected = m_parentObjectListBox.SelectedItems[m_parentObjectListBox.SelectedItems.Count - 1];
                center = (selected.UserData as MySystemObject).CenterPosition + m_offset;
            }
            else
            {
                center = m_currentSelectedAsteroid.CenterPosition;
            }

            MyAsteroidRingData ring = new MyAsteroidRingData();
            ring.CenterPosition = center;
            ring.Width = m_widthSlider.Value * 1000;
            ring.Height = m_heightSlider.Value * 1000;
            ring.Radius = m_radiusSlider.Value * 1000;
            ring.AngleDegrees = new Vector3D(m_angleXSlider.Value, m_angleYSlider.Value, m_angleZSlider.Value);
            return ring;

        }

        /// <summary>
        /// Generates an asteroid ring from the current slider values in the spawn menu
        /// </summary>
        /// <returns>The generated system ring data</returns>
        private MyAsteroidRingData GenerateAsteroidRing()
        {
            if (m_parentObjectListBox.SelectedItems.Count <= 0) return null;
            var selected = m_parentObjectListBox.SelectedItems[m_parentObjectListBox.SelectedItems.Count - 1];
            MySystemObject parent = selected.UserData as MySystemObject;

            MyAsteroidRingData ring = new MyAsteroidRingData();
            ring.CenterPosition = parent.CenterPosition + m_offset;
            ring.Width = m_widthSlider.Value * 1000;
            ring.Height = m_heightSlider.Value * 1000;
            ring.Radius = m_radiusSlider.Value * 1000;
            ring.AngleDegrees = new Vector3D(m_angleXSlider.Value, m_angleYSlider.Value, m_angleZSlider.Value);
            return ring;
        }

        /// <summary>
        /// Sets all slider values to reflect the instance asteroid object
        /// </summary>
        /// <param name="instance">The instance of the asteroid object</param>
        private void SetSliderValues(MySystemAsteroids instance)
        {
            if (instance.AsteroidTypeName != MyAsteroidRingProvider.TYPE_NAME) return;
            MyAsteroidRingData data = MyAsteroidRingProvider.Static.GetInstanceData(instance.Id) as MyAsteroidRingData;
            var planet  = MyStarSystemGenerator.Static.StarSystem.GetById(instance.ParentId) as MySystemPlanet;

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
            m_asteroidSizesSlider.SetValues(instance.AsteroidSize.Min, instance.AsteroidSize.Max);

            m_angleXSlider.Enabled = true;
            m_angleXSlider.Value = (float)data.AngleDegrees.x;
            m_angleYSlider.Enabled = true;
            m_angleYSlider.Value = (float)data.AngleDegrees.y;
            m_angleZSlider.Enabled = true;
            m_angleZSlider.Value = (float)data.AngleDegrees.z;
        }

        /// <summary>
        /// Runs on click of the parent item box. Sets the ranges for the sliders and resets the values.
        /// </summary>
        /// <param name="box">The listbox which calls this method on item clicked</param>
        private void OnParentItemClicked(MyGuiControlListbox box)
        {
            if(box.SelectedItems.Count > 0)
            {
                m_spawnRingButton.Enabled = true;
                m_zoomInButton.Enabled = m_snapToParent;
                m_offsetToCoordButton.Enabled = false;
                m_offset = Vector3D.Zero;

                var parent = box.SelectedItems[box.SelectedItems.Count - 1].UserData as MySystemObject;
                var settings = MySettingsSession.Static.Settings.GeneratorSettings;

                if(parent == MyStarSystemGenerator.Static.StarSystem.CenterObject)
                {
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

                    m_offsetToCoordButton.Enabled = true;

                    if (m_snapToParent)
                        m_parentScreen.CameraLookAt(Vector3D.Zero, new Vector3D(0, 0, m_radiusSlider.Value * 2000));
                    UpdateRingVisual(GenerateAsteroidRing());
                    return;
                }

                if (parent.Type != MySystemObjectType.PLANET && parent.Type != MySystemObjectType.MOON) return;
                var planet = parent as MySystemPlanet;

                m_radiusSlider.MinValue = (int)planet.Diameter / 1000 * 0.75f;
                m_radiusSlider.MaxValue = (int)planet.Diameter / 1000 * 2f;
                m_radiusSlider.Value = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;
                m_radiusSlider.Enabled = true;

                m_widthSlider.MinValue = (int)planet.Diameter / 1000 / 20f;
                m_widthSlider.MaxValue = (int)planet.Diameter / 1000 / 1.25f;
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

                m_nameBox.SetText(new StringBuilder(""));

                if (m_snapToParent)
                    m_parentScreen.CameraLookAt(planet.CenterPosition, (float)planet.Diameter * 2f);
                UpdateRingVisual(GenerateAsteroidRing());
            }
            else
            {
                m_radiusSlider.Enabled = false;
                m_widthSlider.Enabled = false;
                m_heightSlider.Enabled = false;
                m_asteroidSizesSlider.Enabled = false;
                m_angleXSlider.Enabled = false;
                m_angleYSlider.Enabled = false;
                m_angleZSlider.Enabled = false;
                m_spawnRingButton.Enabled = false;
            }
        }

        public void CreateDataEditMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MySystemAsteroids selectedInstance)
        {
            return;
        }
    }
}
*/