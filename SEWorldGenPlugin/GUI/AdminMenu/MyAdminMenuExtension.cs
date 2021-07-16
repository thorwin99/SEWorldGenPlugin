using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using System.Collections.Generic;

namespace SEWorldGenPlugin.GUI.AdminMenu
{
    /// <summary>
    /// Class used to extend the vanilla admin menu.
    /// Submenus can be registered to the admin menu.
    /// An instance of this class only exists, when the admin menu is open.
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
        /// The custom sub menus of the admin menu. This static variable is used
        /// by each instance of the MyAdminMenuExtension and is static, since a new instance
        /// is created each time the admin menu is opened, but sub menus should only need to be registered once.
        /// </summary>
        private static readonly List<MyPluginAdminMenuSubMenu> m_subMenus = new List<MyPluginAdminMenuSubMenu>();

        /// <summary>
        /// Registers a new sub menu to be used with the AdminMenuExtension
        /// </summary>
        /// <param name="subMenu">The sub menu instance</param>
        public static void RegisterSubMenu(MyPluginAdminMenuSubMenu subMenu)
        {
            if (!m_subMenus.Contains(subMenu))
            {
                m_subMenus.Add(subMenu);
            }
        }

        public MyAdminMenuExtension() : base()
        {
            m_requestRecreate = false;
            m_isRecreating = false;
        }

        /// <summary>
        /// Recreates the GUI elements of the admin menu
        /// </summary>
        /// <param name="constructor"></param>
        public override void RecreateControls(bool constructor)
        {
            if (m_isRecreating) return;
            m_isRecreating = true;

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

            if(m_selectedMenuIndex >= m_vanillaSubMenuCount)
            {
                ClearControls();
                int index = m_selectedMenuIndex - m_vanillaSubMenuCount + 1;
                m_subMenus[index].RefreshInternals(null, m_usableWidth);//TODO: Add parent item table
            }

            m_isRecreating = false;
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
                m_vanillaComboBox.SelectItemByKey(m_selectedMenuIndex);
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
            MyGuiControlCombobox newCombo = AddCombo();

            for (int i = 0; i < comboBox.GetItemsCount(); i++)
            {
                newCombo.AddItem(comboBox.GetItemByIndex(i).Key, comboBox.GetItemByIndex(i).Value);
            }

            newCombo.Position = comboBox.Position;
            newCombo.Size = comboBox.Size;
            newCombo.OriginAlign = comboBox.OriginAlign;
            newCombo.SelectItemByIndex(m_selectedMenuIndex);

            Controls[Controls.IndexOf(comboBox)] = newCombo;
            Controls.Remove(comboBox);

            newCombo.ItemSelected += OnAdminSubSelected;
        }
    }
}
