
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
using VRageMath;

namespace SEWorldGenPlugin.Patches
{
    /// <summary>
    /// Patch class to patch the scenario customization screen to add the SEWG settings button.
    /// </summary>
    public class ScenarioCustomizationPatch : HarmonyPatchBase
    {
        private static MyGuiControlCheckbox m_enablePlugin;

        private static Vector2 m_defaultStartingPosition;

        internal static MyPluginSettingsMenu SettingsGui;

        public static MyObjectBuilder_WorldSettings PlSettings;

        public ScenarioCustomizationPatch() : base("Scenario customization plugin settings patch")
        {
        }

        /// <summary>
        /// Postfix patch for the CreateModButton method in MyGuiScreenScenarioSelectionCustomizeScreen, to add the plugin settings to the customization screen.
        /// </summary>
        /// <param name="__instance">Instance of MyGuiScreenScenarioSelectionCustomizeScreen that is patched.</param>
        /// <param name="___m_currentPosition">Member of MyGuiScreenScenarioSelectionCustomizeScreen, current position of the UI elements while building the UI.</param>
        /// <param name="___m_mainSettingsParent">Member of MyGuiScreenScenarioSelectionCustomizeScreen, parent of all settings elements.</param>
        /// <param name="___m_defaultStartingPosition">Member of MyGuiScreenScenarioSelectionCustomizeScreen, the default starting position for the UI elements.</param>
        /// <param name="___m_startButton">Member of MyGuiScreenScenarioSelectionCustomizeScreen, the button that starts the game.</param>
        public static void CreateModButton(MyGuiScreenScenarioSelectionCustomizeScreen __instance, ref Vector2 ___m_currentPosition, MyGuiControlParent ___m_mainSettingsParent, Vector2 ___m_defaultStartingPosition, MyGuiControlButton ___m_startButton)
        {
            m_defaultStartingPosition = ___m_defaultStartingPosition;
            float previousX = ___m_currentPosition.X;

            MyGuiControlParent m_mainSettingsParent = ___m_mainSettingsParent;
            MyGuiControlButton m_startButton = ___m_startButton;

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

            m_mainSettingsParent.Controls.Add(m_enablePluginLabel);
            m_mainSettingsParent.Controls.Add(m_enablePlugin);
            m_mainSettingsParent.Controls.Add(m_pluginSettingsButton);

            GoToSettings(ref ___m_currentPosition);
            ___m_currentPosition.X += 0.025f;
            m_enablePluginLabel.Position = ___m_currentPosition;
            m_pluginSettingsButton.Enabled = m_enablePlugin.IsChecked;

            GoToLabels(ref ___m_currentPosition);

            m_enablePlugin.Position = ___m_currentPosition;
            m_enablePlugin.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;

            m_pluginSettingsButton.Position = m_enablePlugin.Position;
            m_pluginSettingsButton.PositionX += m_enablePlugin.Size.X + 0.009f;
            m_pluginSettingsButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;

            ___m_currentPosition.Y = ___m_currentPosition.Y += m_enablePlugin.Size.Y;
            GoToNextLine(ref ___m_currentPosition);

            m_startButton.ButtonClicked += delegate
            {
                MySettings.Static.SessionSettings = PlSettings;
                MyPluginLog.Log("Settings: " + PlSettings.ToString());
            };

            PlSettings = new MyObjectBuilder_WorldSettings();

            ___m_currentPosition.X = previousX;

        }

        /// <summary>
        /// Go to the next line in the settings list.
        /// </summary>
        /// <param name="currentPos">Current UI element position</param>
        private static void GoToNextLine(ref Vector2 currentPos)
        {
            currentPos.Y = currentPos.Y + 0.0125f;
        }

        /// <summary>
        /// Go to the left side of the settings UI, where the labels are ordered.
        /// </summary>
        /// <param name="currentPos">Current UI element position</param>
        private static void GoToLabels(ref Vector2 currentPos)
        {
            currentPos.X = m_defaultStartingPosition.X + 0.025f + 0.22f + MyGuiConstants.GENERIC_BUTTON_SPACING.X;
        }

        /// <summary>
        /// Go to the right side of the settings UI, where the settings controls are ordered.
        /// </summary>
        /// <param name="currentPos">Current UI element position</param>
        private static void GoToSettings(ref Vector2 currentPos)
        {
            currentPos.X = m_defaultStartingPosition.X;
        }

        /// <summary>
        /// Button callback for the plugin settigns button. Opens the plugin settings window
        /// and provides it the current plugin settings set.
        /// </summary>
        /// <param name="sender"></param>
        private static void OnSettingsClick(object sender)
        {
            SettingsGui = new MyPluginSettingsMenu(true);
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

            var baseMethod = typeof(MyGuiScreenScenarioSelectionCustomizeScreen).GetMethod("CreateModButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var postfix = typeof(ScenarioCustomizationPatch).GetMethod("CreateModButton");

            harmony.Patch(baseMethod, postfix: new HarmonyMethod(postfix));
        }
    }
}
