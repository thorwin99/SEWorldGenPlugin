using Sandbox.Game.Screens;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities.SEWorldGenPlugin.Utilities;
using System;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    public class PluginWorldSettings : MyGuiScreenWorldSettings
    {

        public MyObjectBuilder_PluginSettings PluginSettings;

        private MyGuiControlCheckbox m_enablePlugin;
        private MyGuiControlLabel m_enablePluginLabel;
        private MyGuiControlButton m_pluginSettingsButton;

        private float MARGIN_TOP = 0.22f;

        private float MARGIN_BOTTOM = 50f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;

        private float MARGIN_LEFT_INFO = 29.5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;

        private float MARGIN_RIGHT = 81f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;

        private float MARGIN_LEFT_LIST = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;

        internal PluginSettingsGui SettingsGui;

        public MyObjectBuilder_PluginSettings PlSettings;
        public bool UseGlobal;

        public PluginWorldSettings() : this(null, null)
        {
            MyLog.Default.WriteLine("Create custom settings screen");
        }

        public PluginWorldSettings(MyObjectBuilder_Checkpoint checkpoint, string path) : base(checkpoint, path)
        {
            MyLog.Default.WriteLine("Create custom settings screen");
            Static = this;
            PlSettings = null;
        }

        protected override void BuildControls()
        {
            base.BuildControls();

            Vector2 vector = -m_size.Value / 2f + new Vector2(m_isNewGame ? (MARGIN_LEFT_LIST + MyGuiConstants.LISTBOX_WIDTH + MARGIN_LEFT_INFO) : MARGIN_LEFT_LIST, m_isNewGame ? (MARGIN_TOP + 0.015f) : (MARGIN_TOP - 0.105f));
            Vector2 value = new Vector2(0f, 0.052f);
            Vector2 vector2 = m_size.Value / 2f - vector;
            vector2.X -= MARGIN_RIGHT + 0.005f;
            vector2.Y -= MARGIN_BOTTOM;
            Vector2 vector3 = vector2 * (m_isNewGame ? 0.339f : 0.329f);
            Vector2 vector4 = vector2 - vector3;

            m_enablePluginLabel = new MyGuiControlLabel(null, null, "Enable SEWorldGenPlugin");

            m_enablePlugin = new MyGuiControlCheckbox();
            m_enablePlugin.SetToolTip("Enable the SEWorldGenPlugin for this world");
            m_enablePlugin.IsCheckedChanged = (Action<MyGuiControlCheckbox>)Delegate.Combine(m_enablePlugin.IsCheckedChanged, (Action<MyGuiControlCheckbox>)delegate (MyGuiControlCheckbox s)
            {
                if (PlSettings != null)
                {
                    PlSettings.Enable = s.IsChecked;
                }
            });

            m_pluginSettingsButton = new MyGuiControlButton(null, MyGuiControlButtonStyleEnum.Small, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Plugin settings"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, OnSettingsClick);
            Controls.Add(m_enablePluginLabel);
            Controls.Add(m_enablePlugin);
            Controls.Add(m_pluginSettingsButton);

            m_enablePluginLabel.Position = vector + value * 5;
            m_enablePluginLabel.PositionY += MyGuiConstants.BACK_BUTTON_SIZE.Y * 2;
            m_enablePlugin.Position = m_enablePluginLabel.Position;
            m_enablePlugin.PositionX += m_enablePluginLabel.Size.X + m_enablePlugin.Size.X - 0.009f;
            m_pluginSettingsButton.Position = m_enablePlugin.Position;
            m_pluginSettingsButton.PositionX += m_enablePlugin.Size.X + m_pluginSettingsButton.Size.X / 2;
        }

        private void OnSettingsClick(object sender)
        {
            SettingsGui = new PluginSettingsGui(this);
            SettingsGui.OnOkButtonClicked += Settings_OnOkButtonClick;
            SettingsGui.SetSettings(PlSettings, UseGlobal);
            MyGuiSandbox.AddScreen(SettingsGui);
        }

        private void Settings_OnOkButtonClick()
        {
            UseGlobal = SettingsGui.GetSettings(ref PlSettings);
        }
    }
}
