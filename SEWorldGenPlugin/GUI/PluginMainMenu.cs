using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using System.Text;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    public class PluginMainMenu : MyGuiScreenMainMenu
    {
        private static readonly StringBuilder PLUGIN_LOADED = new StringBuilder("SEWorldGenPlugin is loaded and ");
        private static readonly StringBuilder PLUGIN_ENABLED = new StringBuilder("ENABLED");
        private static readonly StringBuilder PLUGIN_DISABLED = new StringBuilder("DISABLED");

        public PluginMainMenu() : this(false)
        {

        }

        public PluginMainMenu(bool pauseGame) : base(pauseGame)
        {
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