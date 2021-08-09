using Sandbox.Engine.Utils;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    public partial class MyPluginAdminMenu
    {
        /// <summary>
        /// The current fetched star system of the server
        /// </summary>
        MyObjectBuilder_SystemData m_fetchedStarSytem;

        /// <summary>
        /// A list box containing all system objects
        /// </summary>
        MyGuiControlListbox m_systemObjectsBox;

        /// <summary>
        /// The currently selected object in the edit menu
        /// </summary>
        MySystemObject m_selectedObject;

        /// <summary>
        /// Table for the scrollpane containing the editing gui elements for the currently
        /// selected system object
        /// </summary>
        MyGuiControlParentTableLayout m_scrollTable;

        /// <summary>
        /// The scrollpane used to scroll editing menu elements
        /// </summary>
        MyGuiControlScrollablePanel m_scrollPane;

        /// <summary>
        /// Forces the edit menu to refetch the star system
        /// </summary>
        public bool ForceFetchStarSystem = false;

        /// <summary>
        /// The current asteroid admin menu creator used to create the editing elements
        /// for the currently selected asteroid object, if one is selected.
        /// </summary>
        private IMyAsteroidAdminMenuCreator m_currentAsteroidAdminMenu = null;

        /// <summary>
        /// Builds the edit menu
        /// </summary>
        private void BuildEditMenu()
        {
            MyPluginLog.Debug("Adding edit menu");

            if (m_fetchedStarSytem == null || ForceFetchStarSystem)
            {
                MyPluginLog.Debug("Fetching system data");

                MyGuiControlRotatingWheel m_loadingWheel = new MyGuiControlRotatingWheel(position: Vector2.Zero);
                m_loadingWheel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;

                Controls.Add(m_loadingWheel);

                MyStarSystemGenerator.Static.GetStarSystemFromServer(delegate (MyObjectBuilder_SystemData starSystem)
                {
                    m_fetchedStarSytem = starSystem;
                    m_selectedObject = null;
                    ShouldRecreate = true;
                    ForceFetchStarSystem = false;
                });
                return;
            }

            var topCombo = GetCombo();
            Vector2 start = topCombo.Position + new Vector2(0, MARGIN_VERT * 2 + GetCombo().Size.Y);
            Vector2 end = start + new Vector2(topCombo.Size.X, 0.8f - MARGIN_VERT);

            MyGuiControlLabel systemObjsLabel = new MyGuiControlLabel(null, null, "System Objects");
            systemObjsLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            systemObjsLabel.Position = start;

            Controls.Add(systemObjsLabel);

            m_systemObjectsBox = new MyGuiControlListbox();
            m_systemObjectsBox.VisibleRowsCount = 8;
            m_systemObjectsBox.Size = new Vector2(m_usableWidth, m_systemObjectsBox.Size.Y);
            m_systemObjectsBox.Position = start;
            m_systemObjectsBox.PositionY += systemObjsLabel.Size.Y + MARGIN_VERT;
            m_systemObjectsBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;

            foreach (var obj in m_fetchedStarSytem.GetAllObjects())
            {
                if (obj.Type == MySystemObjectType.EMPTY) continue;
                m_systemObjectsBox.Add(new MyGuiControlListbox.Item(new System.Text.StringBuilder(obj.DisplayName), userData: obj));
            }

            if (m_selectedObject != null)
            {
                m_systemObjectsBox.SelectByUserData(m_selectedObject);
            }
            m_systemObjectsBox.ItemsSelected += OnSystemObjectSelect;


            Controls.Add(m_systemObjectsBox);

            MyGuiControlSeparatorList sep = new MyGuiControlSeparatorList();
            sep.AddHorizontal(new Vector2(m_systemObjectsBox.Position.X, m_systemObjectsBox.Position.Y + m_systemObjectsBox.Size.Y + MARGIN_VERT), m_usableWidth);

            BuildEditingSubMenu();

            sep.AddHorizontal(new Vector2(m_scrollPane.Position.X - m_scrollPane.Size.X / 2, m_scrollPane.Position.Y + m_scrollPane.Size.Y), m_usableWidth);

            Controls.Add(sep);

            MyPluginLog.Debug("Added edit menu");
        }

        /// <summary>
        /// Creates the sub menu for the currently selected system object, that contains the 
        /// elements to edit it.
        /// </summary>
        private void BuildEditingSubMenu()
        {
            if(m_scrollTable != null)
                Controls.Remove(m_scrollPane);

            m_scrollTable = new MyGuiControlParentTableLayout(1, false, Vector2.Zero, m_usableWidth);
            m_scrollTable.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;

            m_scrollTable.AddTableSeparator();

            if (m_scrollTable == null)
            {
                m_scrollTable = new MyGuiControlParentTableLayout(1, false, Vector2.Zero, m_usableWidth);
            }
            else
            {
                m_scrollTable.ClearTable();
            }

            m_currentAsteroidAdminMenu = null;

            if (m_selectedObject != null)
            {
                if (m_selectedObject.Type == MySystemObjectType.PLANET || m_selectedObject.Type == MySystemObjectType.MOON)
                {
                    BuildPlanetEditingMenu(m_scrollTable, m_selectedObject as MySystemPlanet);
                    m_scrollTable.AddTableSeparator();

                    CameraLookAt(m_selectedObject.CenterPosition, (float)(m_selectedObject as MySystemPlanet).Diameter * 1.5f);
                }
                else if (m_selectedObject.Type == MySystemObjectType.ASTEROIDS)
                {
                    var asteroidObject = m_selectedObject as MySystemAsteroids;

                    if (!MyAsteroidObjectsManager.Static.AsteroidObjectProviders.ContainsKey(asteroidObject.AsteroidTypeName)) return;

                    var provider = MyAsteroidObjectsManager.Static.AsteroidObjectProviders[asteroidObject.AsteroidTypeName];
                    var creator = provider.GetAdminMenuCreator();

                    if (creator != null)
                    {
                        if (!creator.OnEditMenuSelectItem(m_usableWidth, m_scrollTable, this, asteroidObject, m_fetchedStarSytem))
                        {
                            m_scrollTable.AddTableRow(new MyGuiControlLabel(null, null, "This object cant be edited.", font: "Red"));

                            m_scrollTable.AddTableSeparator();
                        }

                        m_currentAsteroidAdminMenu = creator;
                    }
                }
            }

            m_scrollTable.ApplyRows();

            var topCombo = GetCombo();
            Vector2 start = m_systemObjectsBox.Position + new Vector2(-0.001f, MARGIN_VERT * 2 + m_systemObjectsBox.Size.Y);
            Vector2 end = new Vector2(topCombo.Size.X, 0.5f - MARGIN_VERT);

            m_scrollPane = new MyGuiControlScrollablePanel(m_scrollTable);
            m_scrollPane.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            m_scrollPane.ScrollbarVEnabled = true;
            m_scrollPane.Size = end - start;
            m_scrollPane.Size = new Vector2(0.315f, m_scrollPane.Size.Y);
            m_scrollPane.Position = new Vector2(0, start.Y);

            Controls.Add(m_scrollPane);
        }

        /// <summary>
        /// Makes the spectator cam look at the specific point from the given distance
        /// </summary>
        /// <param name="center">Point to look at</param>
        /// <param name="distance">Distance to look from</param>
        public void CameraLookAt(Vector3D center, float distance)
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
        public void CameraLookAt(Vector3D center, Vector3D distance)
        {
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
            MySpectatorCameraController.Static.Position = center + distance;
            MySpectatorCameraController.Static.Target = center;
        }

        /// <summary>
        /// Builds all elements to edit planets
        /// </summary>
        /// <param name="table">Table to add the elements to</param>
        /// <param name="planet">Planet item to build the edit menu for</param>
        private void BuildPlanetEditingMenu(MyGuiControlParentTableLayout table, MySystemPlanet planet)
        {
            table.AddTableRow(new MyGuiControlLabel(null, null, "This object cant be edited.", font: "Red"));
        }

        /// <summary>
        /// Action when system object is clicked in the system object list box.
        /// Sets the currently selected system object
        /// </summary>
        /// <param name="box"></param>
        private void OnSystemObjectSelect(MyGuiControlListbox box)
        {
            if(m_currentAsteroidAdminMenu != null)
            {
                m_currentAsteroidAdminMenu.Close();
            }
            m_selectedObject = box.SelectedItems[box.SelectedItems.Count - 1].UserData as MySystemObject;
            BuildEditingSubMenu();
        }
    }
}
