using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Generator.ProceduralGen;
using SEWorldGenPlugin.GUI;
using SpaceEngineers.Game.GUI;
using VRage.Game.Entity;
using VRage.Plugins;

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
        }

        public void Update()
        {
        }
    }
}
