using HarmonyLib;
using Sandbox.Game;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Generator.ProceduralGenerator;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.http;
using SEWorldGenPlugin.Patches;
using SEWorldGenPlugin.Utilities;
using System;
using System.IO;
using VRage.Game.Entity;
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

            MyPerGameSettings.GUI.MainMenu = typeof(MyPluginMainMenu);
            MyPerGameSettings.GUI.EditWorldSettingsScreen = typeof(PluginWorldSettings);
            MyPerGameSettings.GUI.AdminMenuScreen = typeof(MyPluginAdminMenu);

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
        private void TryEnablePatches()
        {
            if (settings.Settings.EnablePatching)
            {
                try
                {
                    MyPluginLog.Log("Patching...");
                    Patch();
                    MyPluginLog.Log("Patches applied");
                }
                catch (FileNotFoundException)
                {
                    MyPluginLog.Log("0harmony.dll not found, skipping patching.", LogLevel.WARNING);
                }
                catch (Exception e)
                {
                    MyPluginLog.Log("Something went wrong while patching: ", LogLevel.ERROR);
                    MyPluginLog.Log(e.StackTrace, LogLevel.ERROR);
                }
            }
            else
            {
                MyPluginLog.Log("Patching is disabled, skipping patches.");
            }
        }

        /// <summary>
        /// Applies all harmony patches
        /// </summary>
        private void Patch()
        {
            Harmony harmony = new Harmony("thorwin99.SEWorldGenPlugin");

            var asteroidPatch = new PatchOverlapAllAsteroidSeedsInSphere();
            asteroidPatch.ApplyPatch(harmony);
        }
    }
}
