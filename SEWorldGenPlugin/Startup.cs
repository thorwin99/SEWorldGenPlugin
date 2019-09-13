using Sandbox.Game.Entities;
using SEWorldGenPlugin.Generator.ProceduralGen;
using VRage.Game.Entity;
using VRage.Plugins;

namespace SEWorldGenPlugin
{
    public class Startup : IPlugin
    {
        public void Dispose()
        {
        }

        public void Init(object gameInstance)
        {
            //MyEntity.MyProceduralWorldGeneratorTrackEntityExtCallback += EntityExtension.ProceduralGeneratorTracking;
        }

        public void Update()
        {
        }
    }
}
