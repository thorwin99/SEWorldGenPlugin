using Sandbox.Game;
using SEWorldGenPlugin.Generator.ProceduralGenerator;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.http;
using SEWorldGenPlugin.Utilities;
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

            MyPluginLog.Log("Init completed");
        }

        /// <summary>
        /// Called on update
        /// </summary>
        public void Update()
        {
        }
    }
}
