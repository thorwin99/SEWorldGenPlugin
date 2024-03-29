﻿using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.Utilities;
using System.Collections.Generic;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu
{
    /// <summary>
    /// Class used to extend the vanilla admin menu.
    /// Submenus can be registered to the admin menu.
    /// An instance of this class only exists, when the admin menu is open.
    /// Registered sub menus persist for the whole lifetime of the plugin.
    /// </summary>
    public class MyAdminMenuExtension : MyGuiScreenAdminMenu
    {
        /// <summary>
        /// Vertical margin between objects of the admin menu
        /// </summary>
        private static readonly float MARGIN_VERT = 25f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;

        /// <summary>
        /// Usable width for elements in admin menu to preserve padding
        /// </summary>
        private float m_usableWidth;

        /// <summary>
        /// Whether the menu is currently recreating
        /// </summary>
        private bool m_isRecreating = false;

        /// <summary>
        /// Whether a recreate is requested from outside
        /// </summary>
        private bool m_requestRecreate = false;

        /// <summary>
        /// The count of vanilla admin sub menus
        /// </summary>
        private int m_vanillaSubMenuCount;

        /// <summary>
        /// The current selected menu index
        /// </summary>
        private int m_selectedMenuIndex = 0;

        /// <summary>
        /// The vanilla combo box to select admin sub menu
        /// </summary>
        private MyGuiControlCombobox m_vanillaComboBox;

        /// <summary>
        /// The plugins replacement combo box to select admin sub menu
        /// </summary>
        private MyGuiControlCombobox m_pluginComboBox;

        /// <summary>
        /// The scrollpane used to scroll in the admin menu
        /// </summary>
        MyGuiControlScrollablePanel m_scrollPane;

        /// <summary>
        /// The table used to add controls to, visible in the admin menu
        /// </summary>
        MyGuiControlParentTableLayout m_contentTable;

        /// <summary>
        /// The custom sub menus of the admin menu. This static variable is used
        /// by each instance of the MyAdminMenuExtension and is static, since a new instance
        /// is created each time the admin menu is opened, but sub menus should only need to be registered once per session.
        /// </summary>
        private static readonly List<MyPluginAdminMenuSubMenu> m_subMenus = new List<MyPluginAdminMenuSubMenu>();

        /// <summary>
        /// Registers a new sub menu to be used with the AdminMenuExtension.
        /// A registered sub menu persists for the whole lifetime of the plugin.
        /// That means a sub menu should only be registered once at initialization of the plugin.
        /// </summary>
        /// <param name="subMenu">The sub menu instance</param>
        public static void RegisterSubMenu(MyPluginAdminMenuSubMenu subMenu)
        {
            if (!m_subMenus.Contains(subMenu))
            {
                m_subMenus.Add(subMenu);
            }
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);

            int index = m_selectedMenuIndex - m_vanillaSubMenuCount;

            if (index >= 0 && index < m_subMenus.Count)
                m_subMenus[index].HandleInput();
        }

        /// <summary>
        /// Unregisters all sub menu for admin menus.
        /// </summary>
        public static void UnregisterAllSubMenus()
        {
            m_subMenus.Clear();
        }

        /// <summary>
        /// Static instance of the admin menu
        /// </summary>
        public static MyAdminMenuExtension Static
        {
            get;
            private set;
        }

        public MyAdminMenuExtension() : base()
        {
            m_requestRecreate = false;
            m_isRecreating = false;
            Static = this;
        }

        /// <summary>
        /// Recreates the GUI elements of the admin menu
        /// </summary>
        /// <param name="constructor"></param>
        public override void RecreateControls(bool constructor)
        {
            if (m_isRecreating) return;
            m_isRecreating = true;

            MyPluginLog.Debug("Build admin menu - Start");

            base.RecreateControls(constructor);

            m_usableWidth = Size.Value.X * 0.75f;

            m_vanillaComboBox = GetCombo();
            m_vanillaSubMenuCount = m_vanillaComboBox.GetItemsCount();

            for(int i = 0,j = 0; i < m_subMenus.Count; i++)
            {
                var subMenu = m_subMenus[i];
                if (subMenu == null || !subMenu.IsVisible()) continue;
                m_vanillaComboBox.AddItem(m_vanillaSubMenuCount + j++, subMenu.GetTitle());
            }

            ReplaceComboBox(m_vanillaComboBox);

            if (m_selectedMenuIndex >= m_vanillaSubMenuCount)///Build sub menu
            {
                ClearControls();

                Vector2 start = m_pluginComboBox.Position + new Vector2(0, MARGIN_VERT * 2 + GetCombo().Size.Y);
                Vector2 end = start + new Vector2(m_pluginComboBox.Size.X, 0.8f - MARGIN_VERT);

                m_contentTable = new MyGuiControlParentTableLayout(1, false, Vector2.Zero);
                m_contentTable.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;

                m_contentTable.AddTableSeparator();

                m_scrollPane = new MyGuiControlScrollablePanel(m_contentTable);
                m_scrollPane.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
                m_scrollPane.ScrollbarVEnabled = true;
                m_scrollPane.Size = end - start;
                m_scrollPane.Size = new Vector2(0.315f, m_scrollPane.Size.Y);
                m_scrollPane.Position = new Vector2(0, start.Y);

                Controls.Add(m_scrollPane);

                MyGuiControlSeparatorList sep = new MyGuiControlSeparatorList();
                sep.AddHorizontal(new Vector2(m_scrollPane.Position.X - m_scrollPane.Size.X / 2, m_scrollPane.Position.Y + m_scrollPane.Size.Y), m_usableWidth);

                Controls.Add(sep);

                int index = m_selectedMenuIndex - m_vanillaSubMenuCount;
                m_subMenus[index].RefreshInternals(m_contentTable, m_usableWidth, this);

                RequestResize();
            }

            m_isRecreating = false;

            MyPluginLog.Debug("Build admin menu - End");
        }

        /// <summary>
        /// Runs every frame, recreates the admin menu, when
        /// Recreation was requested.
        /// </summary>
        /// <returns></returns>
        public override bool Draw()
        {
            bool ret =  base.Draw();
            if (m_requestRecreate)
            {
                m_requestRecreate = false;
                RecreateControls(false);
            }

            int index = m_selectedMenuIndex - m_vanillaSubMenuCount;

            if(index >= 0 && index < m_subMenus.Count)
                m_subMenus[index].Draw();

            return ret;
        }

        /// <summary>
        /// Requests a recreation of the admin menu.
        /// This should be preferred to be used, since it is threadsafe other than
        /// <see cref="RecreateControls(bool)"/>
        /// </summary>
        public void RequestRecreate()
        {
            m_requestRecreate = true;
        }

        public void RequestResize()
        {
            m_contentTable.RefreshInternals();
            m_scrollPane.RefreshInternals();
        }

        /// <summary>
        /// Gets the admin menu combo box from the list of controls
        /// </summary>
        /// <returns>The combo box or null if it does not exist</returns>
        private MyGuiControlCombobox GetCombo()
        {
            foreach (var c in Controls)
            {
                if (c is MyGuiControlCombobox)
                {
                    return (MyGuiControlCombobox)c;
                }
            }
            return null;
        }

        /// <summary>
        /// Clears all controls, except those before the combo box to select the current menu from.
        /// Used to rebuild the whole admin menu
        /// </summary>
        private void ClearControls()
        {
            List<MyGuiControlBase> keep = new List<MyGuiControlBase>();
            foreach (var c in Controls)
            {
                keep.Add(c);
                if (c is MyGuiControlCombobox) break;
            }
            Controls.Clear();
            foreach (var c in keep)
            {
                Controls.Add(c);
            }

        }

        /// <summary>
        /// Event handler for admin sub menu selection
        /// </summary>
        private void OnAdminSubSelected()
        {
            m_selectedMenuIndex = GetCombo().GetSelectedIndex();

            if(m_selectedMenuIndex >= m_vanillaSubMenuCount)
            {
                RecreateControls(false);
            }
            else
            {
                m_vanillaComboBox.SelectItemByIndex(m_selectedMenuIndex);
                RecreateControls(false);
            }
        }

        /// <summary>
        /// Replaces the top combo box of the admin menu (<paramref name="comboBox"/>)
        /// so that a new onItemSelected event can be used.
        /// </summary>
        /// <param name="comboBox"></param>
        private void ReplaceComboBox(MyGuiControlCombobox comboBox)
        {
            m_pluginComboBox = AddCombo();

            for (int i = 0; i < comboBox.GetItemsCount(); i++)
            {
                m_pluginComboBox.AddItem(comboBox.GetItemByIndex(i).Key, comboBox.GetItemByIndex(i).Value);
            }

            m_pluginComboBox.Position = comboBox.Position;
            m_pluginComboBox.Size = comboBox.Size;
            m_pluginComboBox.OriginAlign = comboBox.OriginAlign;
            m_pluginComboBox.SelectItemByIndex(m_selectedMenuIndex);

            Controls[Controls.IndexOf(comboBox)] = m_pluginComboBox;
            Controls.Remove(comboBox);

            m_pluginComboBox.ItemSelected += OnAdminSubSelected;
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            foreach(var sub in m_subMenus)
            {
                sub.Close();
            }

            Static = null;
        }
    }
}
