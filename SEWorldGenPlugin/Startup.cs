using Sandbox.Game;
using SEWorldGenPlugin.Generator.ProceduralGen;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.http;
using VRage.Game.Entity;
using VRage.Plugins;
using VRage.Utils;

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
            settings = new MySettings();
            settings.LoadSettings();
            settings.SaveSettings();

            updater = new VersionCheck();
            MyLog.Default.WriteLine("SEWorldGenPlugin - Version is " + updater.GetVersion());
            MyLog.Default.WriteLine("SEWorldGenPlugin - Latest Version is " + updater.GetNewestVersion());

            MyEntity.MyProceduralWorldGeneratorTrackEntityExtCallback += EntityExtension.ProceduralGeneratorTracking;

            MyPerGameSettings.GUI.MainMenu = typeof(PluginMainMenu);
            MyPerGameSettings.GUI.EditWorldSettingsScreen = typeof(PluginWorldSettings);
            MyPerGameSettings.GUI.AdminMenuScreen = typeof(PluginAdminMenu);
        }

        public void Update()
        {
        }
    }
}
