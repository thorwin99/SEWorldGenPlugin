

using HarmonyLib;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System.Text;
using System;
using VRage.Game;
using VRage.Utils;
using VRage;
using VRageMath;

namespace SEWorldGenPlugin.Patches
{
    /// <summary>
    /// Patch class to patch the world settings screen to add the SEWG settings button.
    /// </summary>
    public class WorldSettingsMenuPatch : HarmonyPatchBase
    {
        private static MyGuiControlCheckbox m_enablePlugin;
        private static string m_path;

        private static float MARGIN_TOP = 0.22f;
        private static float MARGIN_BOTTOM = 50f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
        private static float MARGIN_LEFT_INFO = 29.5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
        private static float MARGIN_RIGHT = 81f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
        private static float MARGIN_LEFT_LIST = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;

        private static bool m_isNewGame;

        internal static MyPluginSettingsMenu SettingsGui;
        public static MyObjectBuilder_WorldSettings PlSettings;

        public WorldSettingsMenuPatch() : base("World settings menu patch")
        {
        }

        /// <summary>
        /// Postfix patch for the BuildControls method in MyGuiScreenWorldSettings to add the plugin settings to that settings screen.
        /// </summary>
        /// <param name="___m_isNewGame">private member of MyGuiScreenWorldSettings indicating, whether the screen is a new game screen or edits an existing one.</param>
        /// <param name="___m_size">private member of MyGuiScreenWorldSettings indicating its size.</param>
        /// <param name="__instance">Instance of MyGuiScreenWorldSettings, that this patches.</param>
        /// <param name="___m_sessionPath">private member of MyGuiScreenWorldSettings containing the path to the session save folder.</param>
        public static void Postfix(bool ___m_isNewGame, Vector2? ___m_size, MyGuiScreenWorldSettings __instance, string ___m_sessionPath)
        {
            m_isNewGame = ___m_isNewGame;
            m_path = ___m_sessionPath;
            Vector2? m_size = ___m_size;
            MyGuiControls Controls = __instance.Controls;

            Vector2 vector = -m_size.Value / 2f + new Vector2(m_isNewGame ? (MARGIN_LEFT_LIST + MyGuiConstants.LISTBOX_WIDTH + MARGIN_LEFT_INFO) : MARGIN_LEFT_LIST, m_isNewGame ? (MARGIN_TOP + 0.015f) : (MARGIN_TOP - 0.105f));
            Vector2 value = new Vector2(0f, 0.052f);
            Vector2 vector2 = m_size.Value / 2f - vector;
            vector2.X -= MARGIN_RIGHT + 0.005f;
            vector2.Y -= MARGIN_BOTTOM;
            Vector2 vector3 = vector2 * (m_isNewGame ? 0.339f : 0.329f);
            Vector2 vector4 = vector2 - vector3;

            MyGuiControlLabel m_enablePluginLabel = new MyGuiControlLabel(null, null, "Enable SEWorldGenPlugin");
            MyGuiControlButton m_pluginSettingsButton = new MyGuiControlButton(null, MyGuiControlButtonStyleEnum.Small, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Plugin settings"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, OnSettingsClick);

            m_enablePlugin = new MyGuiControlCheckbox();
            m_enablePlugin.SetToolTip("Enable the SEWorldGenPlugin for this world");
            m_enablePlugin.IsCheckedChanged = (Action<MyGuiControlCheckbox>)Delegate.Combine(m_enablePlugin.IsCheckedChanged, (Action<MyGuiControlCheckbox>)delegate (MyGuiControlCheckbox s)
            {
                if (PlSettings != null)
                {
                    PlSettings.Enabled = s.IsChecked;
                    m_pluginSettingsButton.Enabled = m_enablePlugin.IsChecked;
                }
            });

            m_enablePlugin.IsChecked = false;

            Controls.Add(m_enablePluginLabel);
            Controls.Add(m_enablePlugin);
            Controls.Add(m_pluginSettingsButton);

            m_enablePluginLabel.Position = vector + value * 6;
            m_enablePluginLabel.PositionY += MyGuiConstants.BACK_BUTTON_SIZE.Y * 2;
            m_pluginSettingsButton.Enabled = m_enablePlugin.IsChecked;
            m_enablePlugin.Position = GetControlPosition(Controls);
            m_enablePlugin.PositionY = m_enablePluginLabel.PositionY;
            m_enablePlugin.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_pluginSettingsButton.Position = m_enablePlugin.Position;
            m_pluginSettingsButton.PositionX += m_enablePlugin.Size.X + 0.009f;
            m_pluginSettingsButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            foreach (var c in Controls)
            {
                if (c is MyGuiControlButton)
                {
                    MyGuiControlButton b = (MyGuiControlButton)c;
                    if (b.Text == MyTexts.GetString(MyCommonTexts.Start) || b.Text == MyTexts.GetString(MyCommonTexts.Ok))
                    {
                        if (!m_enablePlugin.IsChecked)
                        {
                            PlSettings = new MyObjectBuilder_WorldSettings();
                            PlSettings.Enabled = m_enablePlugin.IsChecked;
                        }

                        if (m_isNewGame)
                        {
                            b.ButtonClicked += delegate
                            {
                                MySettings.Static.SessionSettings = PlSettings;
                                MyPluginLog.Log("Settings: " + PlSettings.ToString());
                            };
                        }
                        else
                        {
                            b.ButtonClicked += delegate
                            {
                                MyFileUtils.WriteXmlFileToPath(PlSettings, m_path, MySettingsSession.FILE_NAME);
                            };
                        }
                    }
                }
            }

            if (m_isNewGame)
            {
                PlSettings = new MyObjectBuilder_WorldSettings();
            }
            else
            {
                LoadValues();
            }
        }

        /// <summary>
        /// Gets the position of the first checkbox found in the gui control list.
        /// </summary>
        /// <returns>The position</returns>
        private static Vector2 GetControlPosition(MyGuiControls controls)
        {
            foreach (var c in controls)
            {
                if (c is MyGuiControlCheckbox)
                    return c.Position;
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Loads values from the folder of the world, which is currently edited. Used for editing world settings.
        /// </summary>
        private static void LoadValues()
        {
            if (m_path != null && MyFileUtils.FileExistsInPath(m_path, MySettingsSession.FILE_NAME))
            {
                PlSettings = MyFileUtils.ReadXmlFileFromPath<MyObjectBuilder_WorldSettings>(m_path, MySettingsSession.FILE_NAME);
            }
            else
            {
                PlSettings = new MyObjectBuilder_WorldSettings();
            }
            m_enablePlugin.IsChecked = PlSettings.Enabled;
        }

        /// <summary>
        /// Button callback for the plugin settigns button. Opens the plugin settings window
        /// and provides it the current plugin settings set.
        /// </summary>
        /// <param name="sender"></param>
        private static void OnSettingsClick(object sender)
        {
            SettingsGui = new MyPluginSettingsMenu(m_isNewGame);
            SettingsGui.OnOkButtonClicked += Settings_OnOkButtonClick;
            SettingsGui.SetSettings(PlSettings);
            MyGuiSandbox.AddScreen(SettingsGui);
        }

        /// <summary>
        /// Callback for the confirm button in the plugin settings menu,
        /// to get the new settings back.
        /// </summary>
        private static void Settings_OnOkButtonClick()
        {
            PlSettings = SettingsGui.GetSettings();
            PlSettings.Enabled = m_enablePlugin.IsChecked;
        }

        public override void ApplyPatch(Harmony harmony)
        {
            base.ApplyPatch(harmony);

            var baseMethod = typeof(MyGuiScreenWorldSettings).GetMethod("BuildControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var postfix = typeof(WorldSettingsMenuPatch).GetMethod("Postfix");

            harmony.Patch(baseMethod, postfix: new HarmonyMethod(postfix));
        }
    }
}
