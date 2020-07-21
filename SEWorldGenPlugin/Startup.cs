using Sandbox.Game;
using SEWorldGenPlugin.Generator.ProceduralGen;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.http;
using SEWorldGenPlugin.Utilities;
using VRage.Game.Entity;
using VRage.Plugins;

namespace SEWorldGenPlugin
{
    public class Startup : IPlugin
    {
        MySettings settings;
        VersionCheck updater;

        public void Dispose()
        {
            settings.SaveSettings();
        }

        public void Init(object gameInstance)
        {
            PluginLog.Log("Begin init");

            settings = new MySettings();
            settings.LoadSettings();
            settings.SaveSettings();

            updater = new VersionCheck();
            PluginLog.Log("Version is " + updater.GetVersion());
            PluginLog.Log("Latest Version is " + updater.GetNewestVersion());

            MyEntity.MyProceduralWorldGeneratorTrackEntityExtCallback += EntityExtension.ProceduralGeneratorTracking;
            MyEntity.MyProceduralWorldGeneratorTrackEntityExtCallback += EntityExtension.ProceduralGeneratorTracking;

            MyPerGameSettings.GUI.MainMenu = typeof(PluginMainMenu);
            MyPerGameSettings.GUI.EditWorldSettingsScreen = typeof(PluginWorldSettings);
            MyPerGameSettings.GUI.AdminMenuScreen = typeof(PluginAdminMenu);

            PluginLog.Log("Init completed");
        }

        public void Update()
        {
        }
    }
}
