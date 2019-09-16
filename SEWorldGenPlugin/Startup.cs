using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Generator.ProceduralGen;
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

            MyEntity.MyProceduralWorldGeneratorTrackEntityExtCallback += EntityExtension.ProceduralGeneratorTracking;
        }

        public void Update()
        {
        }
    }
}
