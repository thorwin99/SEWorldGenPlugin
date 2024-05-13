using HarmonyLib;
using Sandbox.Graphics.GUI;
using Sandbox.Graphics;
using SEWorldGenPlugin.http;
using SpaceEngineers.Game.GUI;
using System.Text;
using VRage.Utils;
using SEWorldGenPlugin.Utilities;

namespace SEWorldGenPlugin.Patches
{
    /// <summary>
    /// Patch class to patch the main menu to add the version string and new version popup
    /// </summary>
    public class MainMenuPatch : HarmonyPatchBase
    {

        private static readonly StringBuilder PLUGIN_LOADED = new StringBuilder("SEWorldGenPlugin version {0} installed");

        public MainMenuPatch() : base("Main menu GUI patch")
        {
        }

        /// <summary>
        /// Postfix patch for the RecreateControls method of MyGuiScreenMainMenu, that adds the plugin version string to it.
        /// </summary>
        /// <param name="constructor">Whether called from constructor or not.</param>
        /// <param name="__instance">Instance of MyGuiScreenMainMenu that was patched.</param>
        public static void RecreateControls(bool constructor, MyGuiScreenMainMenu __instance)
        {
            MyGuiControlLabel pluginVersionLabel = new MyGuiControlLabel();
            pluginVersionLabel.Text = string.Format(PLUGIN_LOADED.ToString(), VersionCheck.Static.GetVersion());
            pluginVersionLabel.Position = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, 8, 8);
            pluginVersionLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            pluginVersionLabel.PositionY -= pluginVersionLabel.Size.Y / 2;

            __instance.Controls.Add(pluginVersionLabel);
        }

        public override void ApplyPatch(Harmony harmony)
        {
            base.ApplyPatch(harmony);

            var baseMethod = typeof(MyGuiScreenMainMenu).GetMethod("RecreateControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var postfix = typeof(MainMenuPatch).GetMethod("RecreateControls");

            harmony.Patch(baseMethod, postfix: new HarmonyMethod(postfix));
        }
    }
}
