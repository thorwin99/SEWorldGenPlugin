using Sandbox;
using Sandbox.Game.Screens;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.http;
using SEWorldGenPlugin.Utilities;
using SpaceEngineers.Game.GUI;
using System;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    /// <summary>
    /// Replacement for the main menu, to add plugin specific information, such as the version of the plugin and if it is enabled.
    /// Also replaces the new game button to insert its own new game menu.
    /// </summary>
    public class MyPluginMainMenu : MyGuiScreenMainMenu
    {
        private static readonly StringBuilder PLUGIN_LOADED = new StringBuilder("SEWorldGenPlugin version {0} installed");

        private static bool OPENED_VERSION_NOTIFICATION = false;

        private MyGuiControlElementGroup m_elemtents;

        public MyPluginMainMenu() : this(false)
        {

        }

        public MyPluginMainMenu(bool pauseGame) : base(pauseGame)
        {
            m_pauseGame = pauseGame;
        }

        /// <summary>
        /// Recreates the gui controls and replaces the new game button to insert its own new game menu.
        /// Also shows a message, if a new version of the plugin is available
        /// </summary>
        /// <param name="constructor"></param>
        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            if (!m_pauseGame)
            {
                m_elemtents = new MyGuiControlElementGroup();
                m_elemtents.HighlightChanged += OnHighlightChange;
                MyGuiControlButton button = null;
                foreach (var c in Controls)
                {
                    if (c is MyGuiControlButton)
                    {
                        m_elemtents.Add(c);
                        if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonCampaign))
                            button = (MyGuiControlButton)c;
                    }
                }
                if (button != null)
                {
                    int index = Controls.IndexOf(button);
                    MyGuiControlButton newGameButton = MakeButton(button.Position, MyCommonTexts.ScreenMenuButtonCampaign, OnNewGameClick);
                    Controls.Add(newGameButton);
                    m_elemtents.Add(newGameButton);
                    newGameButton.Name = button.Name;
                    Controls[index] = newGameButton;
                    newGameButton.SetToolTip(button.Tooltips);
                }

                if (!VersionCheck.Static.IsNewest() && !OPENED_VERSION_NOTIFICATION)
                {
                    OPENED_VERSION_NOTIFICATION = true;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder(MyPluginTexts.MESSAGES.UPDATE_AVAILABLE_BOX), new StringBuilder(MyPluginTexts.MESSAGES.UPDATE_AVAILABLE_TITLE), null, null, null, null, OnUpdateNotifiactionMessageClose));
                }
            }

            MyGuiControlLabel pluginVersionLabel = new MyGuiControlLabel();
            pluginVersionLabel.Text = string.Format(PLUGIN_LOADED.ToString(), VersionCheck.Static.GetVersion());
            pluginVersionLabel.Position = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, 8, 8);
            pluginVersionLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            pluginVersionLabel.PositionY -= pluginVersionLabel.Size.Y / 2;

            MyGuiControlButton pluginSettingsBtn = MyPluginGuiHelper.CreateDebugButton("Settings", OnPluginSettingsClick);
            pluginSettingsBtn.Position = pluginVersionLabel.Position;
            pluginSettingsBtn.PositionX += pluginVersionLabel.Size.X + (8 / MyGuiConstants.GUI_OPTIMAL_SIZE.X);
            pluginSettingsBtn.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;

            Controls.Add(pluginVersionLabel);
            Controls.Add(pluginSettingsBtn);
        }

        /// <summary>
        /// Callback when the message box, which notifies the user of a new plugin version, gets closed.
        /// Opens the plugins github page, if the message box was confirmed with yes
        /// </summary>
        /// <param name="r">Message box result</param>
        private void OnUpdateNotifiactionMessageClose(MyGuiScreenMessageBox.ResultEnum r)
        {
            if(r == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MyGuiSandbox.OpenUrl(VersionCheck.Static.GetLatestVersionPage(), UrlOpenMode.SteamOrExternalWithConfirm);
            }
        }

        /// <summary>
        /// Callback for when the highlight of an gui element gets changed. Updates the highlights of the elements.
        /// </summary>
        /// <param name="obj"></param>
        private void OnHighlightChange(MyGuiControlElementGroup obj)
        {
            foreach (var c in m_elemtents)
            {
                if (c.HasFocus && m_elemtents.SelectedIndex != obj.SelectedIndex)
                {
                    FocusedControl = c;
                    break;
                }
            }
        }

        /// <summary>
        /// Callback for the new game button to open the plugins new game menu
        /// </summary>
        /// <param name="sender"></param>
        private void OnNewGameClick(object sender)
        {
            RunWithTutorialCheck(delegate
            {
                MyGuiSandbox.AddScreen(new PluginGuiScreenNewGame());
            });
        }

        /// <summary>
        /// Shows tutorial and runs afterTutorial if necessary, if not just runs afterTurorial.
        /// </summary>
        /// <param name="afterTutorial">Action to run after doing the tutorial</param>
        private void RunWithTutorialCheck(Action afterTutorial)
        {
            if (MySandboxGame.Config.FirstTimeTutorials)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenTutorialsScreen(afterTutorial));
            }
            else
            {
                afterTutorial();
            }
        }

        /// <summary>
        /// Runs when the settings button for the plugin is clicked.
        /// </summary>
        /// <param name="btn">The button that called this event</param>
        private void OnPluginSettingsClick(MyGuiControlButton btn)
        {
            MyGuiSandbox.AddScreen(new MyPluginGlobalSettings());
        }
    }
}
