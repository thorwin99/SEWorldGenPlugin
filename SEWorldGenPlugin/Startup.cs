using HarmonyLib;
using Sandbox.Game;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.http;
using SEWorldGenPlugin.Patches;
using SEWorldGenPlugin.Utilities;
using System;
using System.IO;
using VRage.Plugins;

namespace SEWorldGenPlugin
{
    /// <summary>
    /// Startup class for the plugin. This loads the plugin for SE.
    /// Needs to inherit from IPlugin for SE to load it.
    /// </summary>
    public class Startup : IPlugin
    {
        /// <summary>
        /// Global settings instance
        /// </summary>
        MySettings settings;

        /// <summary>
        /// Version checker instance
        /// </summary>
        VersionCheck updater;

        /// <summary>
        /// Called, when SE closes. Saves the global config settings
        /// </summary>
        public void Dispose()
        {
            settings.SaveSettings();
        }

        /// <summary>
        /// Called, when SE initializes the plugin. Loads the global settings, sets the procedural
        /// generator entity tracking extension method up, and replaces the GUI screens of SE if the plugin
        /// ones.
        /// </summary>
        /// <param name="gameInstance">Isntance of the game</param>
        public void Init(object gameInstance)
        {
            MyPluginLog.Log("Begin SEWorldGenPlugin init");

            settings = new MySettings();
            settings.LoadSettings();
            settings.SaveSettings();

            updater = new VersionCheck();
            MyPluginLog.Log("Version is " + updater.GetVersion());
            MyPluginLog.Log("Latest Version is " + updater.GetNewestVersion());

            MyPerGameSettings.GUI.AdminMenuScreen = typeof(MyAdminMenuExtension);

            TryEnablePatches();

            MyPluginLog.Log("Init completed");
        }

        /// <summary>
        /// Called on update
        /// </summary>
        public void Update()
        {
        }

        /// <summary>
        /// Tries to apply the harmony patches to fix more complicated bugs.
        /// Will only actually patch, if 0harmony.dll is found AND patching is enabled in the global settings.
        /// </summary>
        private bool TryEnablePatches()
        {
            try
            {
                MyPluginLog.Log("Patching...");
                Patch();
                MyPluginLog.Log("Patches applied");

                return true;
            }
            catch (FileNotFoundException)
            {
                MyPluginLog.Log("0harmony.dll not found, skipping patching. It is required for this plugin to function correctly.", LogLevel.ERROR);
            }
            catch (Exception e)
            {
                MyPluginLog.Log("Something went wrong while patching: ", LogLevel.ERROR);
                MyPluginLog.Log(e.Message, LogLevel.ERROR);
                MyPluginLog.Log(e.StackTrace, LogLevel.ERROR);
            }

            return false;
        }

        /// <summary>
        /// Applies all harmony patches
        /// </summary>
        private void Patch()
        {
            Harmony harmony = new Harmony("thorwin99.SEWorldGenPlugin");

            var asteroidPatch = new PatchOverlapAllAsteroidSeedsInSphere();
            asteroidPatch.ApplyPatch(harmony);
            var encounterPatch = new PatchAsteroidGeneration();
            encounterPatch.ApplyPatch(harmony);
            var copyDirectoryPatch = new CopyDirectoryPatch();
            copyDirectoryPatch.ApplyPatch(harmony);
            var mainMenuPatch = new MainMenuPatch();
            mainMenuPatch.ApplyPatch(harmony);
            var worldSettingsPatch = new WorldSettingsMenuPatch();
            worldSettingsPatch.ApplyPatch(harmony);
            var scenarioCustomizationPatch = new ScenarioCustomizationPatch();
            scenarioCustomizationPatch.ApplyPatch(harmony);
        }

        /// <summary>
        /// Used with PluginLoader only. Opens the global plugin config menu for SEWG
        /// </summary>
        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyPluginGlobalSettings());
        }
    }
}
