﻿using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.Generator.AsteroidObjects;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus
{
    /// <summary>
    /// The star system designer admin menu, used to create and edit star systems generated by the plugin.
    /// </summary>
    public class MyStarSystemDesignerMenu : MyPluginAdminMenuSubMenu
    {
        /// <summary>
        /// An enum indicating which zoom level the star system designer currently is on.
        /// </summary>
        private enum ZoomLevel
        {
            ORBIT,
            OBJECT
        }

        /// <summary>
        /// A list box containing all system objects
        /// </summary>
        private MyGuiControlListbox m_systemObjectsBox;

        /// <summary>
        /// Button used to refresh the gui elements to contain the current system.
        /// </summary>
        private MyGuiControlButton m_refreshSystemButton;

        /// <summary>
        /// Button used to add a new Object to the system.
        /// </summary>
        private MyGuiControlButton m_addObjectButton;

        /// <summary>
        /// Button to apply the changes done to a system object.
        /// </summary>
        private MyGuiControlButton m_applyChangesButton;

        /// <summary>
        /// Button used to zoom into the current selected object.
        /// </summary>
        private MyGuiControlButton m_zoomInButton;

        /// <summary>
        /// Button used to zoom out of the current selected object.
        /// </summary>
        private MyGuiControlButton m_zoomOutButton;

        /// <summary>
        /// Table that holds the controls to set object specific settings.
        /// </summary>
        private MyGuiControlParentTableLayout m_subMenuControlTable;

        /// <summary>
        /// Current instance of the admin menu
        /// </summary>
        private MyAdminMenuExtension m_adminMenuInst;

        /// <summary>
        /// A dictionary that stores all currently changed system objects, that have not yet been applied
        /// </summary>
        private Dictionary<Guid, MySystemObject> m_pendingSystemObjects;

        /// <summary>
        /// The id of the currently selected system object.
        /// </summary>
        private Guid m_selectedObjectId;

        /// <summary>
        /// The current zoom level of the spectator cam
        /// </summary>
        private ZoomLevel m_zoomLevel;

        /// <summary>
        /// The usable gui width
        /// </summary>
        private float m_usableWidth;

        /// <summary>
        /// The menu currently used to edit the selected object
        /// </summary>
        private MyStarSystemDesignerObjectMenu m_currentObjectMenu;

        public MyStarSystemDesignerMenu()
        {
            m_pendingSystemObjects = new Dictionary<Guid, MySystemObject>(); //Needs to be cleaned on session close
            m_selectedObjectId = Guid.Empty;
            m_zoomLevel = ZoomLevel.ORBIT;
        }

        public override void Close()
        {
            MyPluginLog.Debug("Close");
            m_selectedObjectId = Guid.Empty;
            m_adminMenuInst = null;
            m_systemObjectsBox = null;
            m_zoomLevel = ZoomLevel.ORBIT;
        }

        public override string GetTitle()
        {
            return "Star System Designer";
        }

        public override bool IsVisible()
        {
            return MyPluginSession.Static.ServerVersionMatch;//Star system designer is only visible, IFF server and client version match up
        }

        public override void RefreshInternals(MyGuiControlParentTableLayout parent, float maxWidth, MyAdminMenuExtension instance)
        {
            MyPluginLog.Debug("Building Star system designer admin menu");

            m_adminMenuInst = instance;
            m_usableWidth = maxWidth;

            MyGuiControlLabel systemBoxLabel = new MyGuiControlLabel(null, null, "System Objects");
            parent.AddTableRow(systemBoxLabel);

            m_systemObjectsBox = new MyGuiControlListbox();
            m_systemObjectsBox.VisibleRowsCount = 8;
            m_systemObjectsBox.Size = new Vector2(maxWidth, m_systemObjectsBox.Size.Y);
            RefreshSystemList();
            m_systemObjectsBox.SelectByUserData(m_selectedObjectId);
            m_systemObjectsBox.ItemsSelected += OnSystemObjectSelected;

            parent.AddTableRow(m_systemObjectsBox);

            m_refreshSystemButton = MyPluginGuiHelper.CreateDebugButton("Refresh", RefreshSystem, true);
            m_refreshSystemButton.Size = new Vector2(maxWidth, m_refreshSystemButton.Size.Y);

            parent.AddTableRow(m_refreshSystemButton);

            m_addObjectButton = MyPluginGuiHelper.CreateDebugButton("Add new object", AddNewSystemObject, false);
            m_addObjectButton.Size = new Vector2(maxWidth, m_addObjectButton.Size.Y);

            parent.AddTableRow(m_addObjectButton);
            parent.AddTableSeparator();

            var row = new MyGuiControlParentTableLayout(3, false, Vector2.Zero);

            m_zoomInButton = new MyGuiControlButton(null, VRage.Game.MyGuiControlButtonStyleEnum.Increase, onButtonClick: OnZoomLevelChange, toolTip: "Zoom onto the selected object");
            m_zoomOutButton = new MyGuiControlButton(null, VRage.Game.MyGuiControlButtonStyleEnum.Decrease, onButtonClick: OnZoomLevelChange, toolTip: "Zoom out of the selected object");

            m_zoomInButton.Enabled = m_zoomLevel != ZoomLevel.OBJECT;
            m_zoomOutButton.Enabled = m_zoomLevel != ZoomLevel.ORBIT;

            row.AddTableRow(m_zoomInButton, m_zoomOutButton, new MyGuiControlLabel(text: "Zoom in / out"));

            parent.AddTableRow(row);
            parent.AddTableSeparator();

            m_subMenuControlTable = new MyGuiControlParentTableLayout(1, false, Vector2.Zero);

            if(m_selectedObjectId != Guid.Empty)
            {
                //Fill with selected object specific controls
                SetSubMenuControls();
            }

            parent.AddTableRow(m_subMenuControlTable);
            parent.AddTableSeparator();

            m_applyChangesButton = MyPluginGuiHelper.CreateDebugButton("Apply", AddNewSystemObject, false, "Apply settings of this object and spawn it if it isnt spawned yet.");
            m_applyChangesButton.Size = new Vector2(maxWidth, m_applyChangesButton.Size.Y);

            parent.AddTableRow(m_applyChangesButton);
        }

        /// <summary>
        /// Creates the sub menu controls, based on the type of selected object and whether it already exists or not.
        /// </summary>
        private void SetSubMenuControls()
        {
            m_subMenuControlTable.ClearTable();

            var StarSystem = MyStarSystemGenerator.Static.StarSystem;
            bool exists = StarSystem.Contains(m_selectedObjectId);
            MySystemObject obj;
            if (m_pendingSystemObjects.ContainsKey(m_selectedObjectId))
            {
                obj = m_pendingSystemObjects[m_selectedObjectId];
            }
            else if (exists)
            {
                obj = StarSystem.GetById(m_selectedObjectId);
            }
            else return;

            if(obj.Type == MySystemObjectType.PLANET || obj.Type == MySystemObjectType.MOON)
            {
                BuildPlanetMenuControls(exists, obj as MySystemPlanet);
            }
            else if(obj.Type == MySystemObjectType.ASTEROIDS)
            {
                MySystemAsteroids asteroid = obj as MySystemAsteroids;
                MyAbstractAsteroidObjectProvider prov;
                if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(asteroid.AsteroidTypeName, out prov))
                {
                    var adminMenu = prov.GetAdminMenuCreator();
                    if(adminMenu != null)
                    {
                        adminMenu.CreateDataEditMenu(m_usableWidth, m_subMenuControlTable, asteroid);
                    }
                }
            }
            m_adminMenuInst.RequestResize();
        }

        /// <summary>
        /// Creates the controls to edit or spawn a planet.
        /// </summary>
        /// <param name="exists">If the planet already exists in the world.</param>
        /// <param name="planet">The planet object itself</param>
        private void BuildPlanetMenuControls(bool exists, MySystemPlanet planet)
        {
            m_currentObjectMenu = new MyStarSystemDesignerPlanetMenu(planet);
            m_currentObjectMenu.RecreateControls(m_subMenuControlTable, m_usableWidth, exists);
        }

        /// <summary>
        /// When a new System object is selected, update GUI
        /// </summary>
        /// <param name="box"></param>
        private void OnSystemObjectSelected(MyGuiControlListbox box)
        {
            if (box.SelectedItems.Count < 1) return;
            Guid newId = (Guid)box.SelectedItems[box.SelectedItems.Count - 1].UserData;
            MyPluginLog.Debug("On selecet " + newId);
            m_selectedObjectId = newId;
            SetSubMenuControls();
        }

        /// <summary>
        /// The event called when one of the zoom buttons is clicked.
        /// </summary>
        /// <param name="btn"></param>
        private void OnZoomLevelChange(MyGuiControlButton btn)
        {
            if(btn == m_zoomInButton)
            {
                m_zoomLevel++;
            }
            else
            {
                m_zoomLevel--;
            }

            m_zoomInButton.Enabled = m_zoomLevel != ZoomLevel.OBJECT;
            m_zoomOutButton.Enabled = m_zoomLevel != ZoomLevel.ORBIT;
        }

        /// <summary>
        /// Opens window to create new System object in the system.
        /// </summary>
        /// <param name="btn"></param>
        private void AddNewSystemObject(MyGuiControlButton btn)
        {

        }

        /// <summary>
        /// Applies the changes of the currently selected system object, if it has changes done to it.
        /// </summary>
        /// <param name="btn"></param>
        private void ApplyChanges(MyGuiControlButton btn)
        {

        }

        /// <summary>
        /// Action called to refresh the GUI representing the current system.
        /// </summary>
        /// <param name="btn"></param>
        private void RefreshSystem(MyGuiControlButton btn)
        {
            RefreshSystemList();

            if (m_selectedObjectId != Guid.Empty)
            {
                m_systemObjectsBox.SelectByUserData(m_selectedObjectId);
            }
        }

        private void RefreshSystemList()
        {
            MyPluginLog.Debug("Refresh");
            m_systemObjectsBox.ClearItems();
            var system = MyStarSystemGenerator.Static.StarSystem;

            system.Foreach((int depth, MySystemObject obj) =>
            {
                var text = new System.Text.StringBuilder("");
                if (depth > 0)
                {
                    for (int i = 0; i < depth; i++)
                        text.Append("   ");
                }

                text.Append(obj.DisplayName);

                m_systemObjectsBox.Add(new MyGuiControlListbox.Item(text, userData: obj.Id));

                //Add pending system object that have this parent.
                foreach (var pending in m_pendingSystemObjects)
                {
                    if (pending.Value.ParentId == obj.ParentId)
                    {
                        var text2 = new System.Text.StringBuilder("");
                        if (depth + 1 > 0)
                        {
                            for (int i = 0; i < depth + 1; i++)
                                text.Append("   ");
                        }

                        text2.Append(pending.Value.DisplayName);
                        text2.Append(" *");
                        m_systemObjectsBox.Add(new MyGuiControlListbox.Item(text2, userData: pending.Value.Id));
                    }
                }
            });
        }
    }
}
