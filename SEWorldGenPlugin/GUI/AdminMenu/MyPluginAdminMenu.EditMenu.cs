using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    public partial class MyPluginAdminMenu
    {
        MyObjectBuilder_SystemData m_fetchedStarSytem;

        MyGuiControlListbox m_systemObjectsBox;
        MySystemObject m_selectedObject;
        MyGuiControlParentTableLayout m_scrollTable;
        MyGuiControlScrollablePanel m_scrollPane;

        /// <summary>
        /// Forces the edit menu to refetch the star system
        /// </summary>
        public bool ForceFetchStarSystem = false;

        private void BuildEditMenu()
        {
            MyPluginLog.Debug("Adding edit menu");

            if (m_fetchedStarSytem == null || ForceFetchStarSystem)
            {
                MyPluginLog.Debug("Fetching system data");

                MyGuiControlRotatingWheel m_loadingWheel = new MyGuiControlRotatingWheel(position: Vector2.Zero);
                m_loadingWheel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;

                Controls.Add(m_loadingWheel);

                MyStarSystemGenerator.Static.GetStarSystem(delegate (MyObjectBuilder_SystemData starSystem)
                {
                    m_fetchedStarSytem = starSystem;
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

            if (m_selectedObject != null)
            {
                if (m_selectedObject.Type == MySystemObjectType.PLANET || m_selectedObject.Type == MySystemObjectType.MOON)
                {
                    BuildPlanetEditingMenu(m_scrollTable, m_selectedObject as MySystemPlanet);
                    m_scrollTable.AddTableSeparator();
                }
                else if (m_selectedObject.Type == MySystemObjectType.ASTEROIDS)
                {
                    var asteroidObject = m_selectedObject as MySystemAsteroids;

                    if (!MyAsteroidObjectsManager.Static.AsteroidObjectProviders.ContainsKey(asteroidObject.AsteroidTypeName)) return;

                    var provider = MyAsteroidObjectsManager.Static.AsteroidObjectProviders[asteroidObject.AsteroidTypeName];
                    var creator = provider.GetAdminMenuCreator();

                    if (creator != null)
                    {
                        if (!creator.CreateEditMenu(m_usableWidth, m_scrollTable, this, asteroidObject))
                        {
                            m_scrollTable.AddTableSeparator();
                        }
                    }
                }
            }

            m_scrollTable.ApplyRows();

            var topCombo = GetCombo();
            Vector2 start = m_systemObjectsBox.Position + new Vector2(-0.001f, MARGIN_VERT * 2 + m_systemObjectsBox.Size.Y);
            Vector2 end = start + new Vector2(topCombo.Size.X, 0.8f - MARGIN_VERT);

            m_scrollPane = new MyGuiControlScrollablePanel(m_scrollTable);
            m_scrollPane.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            m_scrollPane.ScrollbarVEnabled = true;
            m_scrollPane.Size = end - start;
            m_scrollPane.Size = new Vector2(0.315f, m_scrollPane.Size.Y);
            m_scrollPane.Position = new Vector2(0, start.Y);

            Controls.Add(m_scrollPane);
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

        private void OnSystemObjectSelect(MyGuiControlListbox box)
        {
            m_selectedObject = box.SelectedItems[box.SelectedItems.Count - 1].UserData as MySystemObject;
            BuildEditingSubMenu();
        }
    }
}
