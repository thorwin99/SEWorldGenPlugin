using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Utils;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.FileSystem;
using VRage.Game;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    public class PluginMainMenu : MyGuiScreenMainMenu
    {
        private static readonly StringBuilder PLUGIN_LOADED = new StringBuilder("SEWorldGenPlugin is loaded and ");
        private static readonly StringBuilder PLUGIN_ENABLED = new StringBuilder("ENABLED");
        private static readonly StringBuilder PLUGIN_DISABLED = new StringBuilder("DISABLED");

        private bool m_pauseGame;

        public PluginMainMenu() : this(false)
        {

        }

        public PluginMainMenu(bool pauseGame) : base(pauseGame)
        {
            m_pauseGame = pauseGame;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            if (m_pauseGame) return;
            MyGuiControlButton button = null;
            foreach (var c in Controls)
            {
                if(c is MyGuiControlButton)
                {
                    button = (MyGuiControlButton)c;
                    if (button.Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonCampaign))
                        break;
                }
            }
            if(button != null)
            {
                Controls.Remove(button);
                MyGuiControlButton newGameButton = new MyGuiControlButton(button.Position, button.VisualStyle, button.Size, button.ColorMask, button.OriginAlign, null, new StringBuilder(button.Text), button.TextScale, button.TextAlignment, button.HighlightType, OnNewGameClick, button.CueEnum);
                Controls.Add(newGameButton);
            }
        }

        private void OnNewGameClick(object sender)
        {
            if (!MyFakes.LIMITED_MAIN_MENU || MyInput.Static.ENABLE_DEVELOPER_KEYS)
            {
                RunWithTutorialCheck(delegate
                {
                    MyGuiSandbox.AddScreen(new PluginGuiScreenNewGame());
                });
            }
            else
            {
                QuickstartScenario("Red Ship");
            }
        }

        private void QuickstartScenario(string scenarioName)
        {
            string path = "CustomWorlds";
            string sessionPath = Path.Combine(MyFileSystem.ContentPath, path, scenarioName);
            ulong sizeInBytes;
            MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(sessionPath, out sizeInBytes);
            if (checkpoint != null)
            {
                MySessionLoader.LoadSingleplayerSession(checkpoint, sessionPath, sizeInBytes, delegate
                {
                    MyAsyncSaving.Start(null, Path.Combine(MyFileSystem.SavesPath, checkpoint.SessionName.Replace(':', '-')));
                });
            }
        }

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

        public override bool Draw()
        {
            DrawPluginLoaded();
            return base.Draw();
        }

        private void DrawPluginLoaded()
        {
            Vector2 size;
            Vector2 textLeftBottomPosition = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, 8, 8);
            textLeftBottomPosition.Y -= 0 * TEXT_LINE_HEIGHT;

            size = MyGuiManager.MeasureString(MyFontEnum.BuildInfo, PLUGIN_LOADED, 1);

            MyGuiManager.DrawString(MyFontEnum.BuildInfo, PLUGIN_LOADED, textLeftBottomPosition, 1, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 1), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);

            textLeftBottomPosition.X += size.X;

            if (MySettings.Static.Settings.Enable)
            {
                MyGuiManager.DrawString(MyFontEnum.BuildInfoHighlight, PLUGIN_ENABLED, textLeftBottomPosition, 1, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 1), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            }
            else
            {
                MyGuiManager.DrawString(MyFontEnum.BuildInfoHighlight, PLUGIN_DISABLED, textLeftBottomPosition, 1, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 1), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            }
        }
    }
}
