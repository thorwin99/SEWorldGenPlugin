using Sandbox.Game;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.Generator.ProceduralGen;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.ObjectBuilders;
using VRage.Game.Entity;
using VRage.Plugins;
using VRage.Utils;

namespace SEWorldGenPlugin
{
    public class Startup : IPlugin
    {
        MySettings settings;

        public void Dispose()
        {
            settings.SaveSettings();
        }

        public void Init(object gameInstance)
        {
            settings = new MySettings();
            settings.LoadSettings();
            settings.SaveSettings();

            MyEntity.MyProceduralWorldGeneratorTrackEntityExtCallback += EntityExtension.ProceduralGeneratorTracking;

            MyPerGameSettings.GUI.MainMenu = typeof(PluginMainMenu);
            MyPerGameSettings.GUI.CustomWorldScreen = typeof(PluginWorldSettings);
            MyPerGameSettings.GUI.EditWorldSettingsScreen = typeof(PluginWorldSettings);
            MyPerGameSettings.GUI.AdminMenuScreen = typeof(PluginAdminMenu);
        }

        public void Update()
        {
        }
    }
}
