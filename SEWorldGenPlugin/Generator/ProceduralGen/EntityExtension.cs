using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRage.Utils;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public static class EntityExtension
    {

        public static void ProceduralGeneratorTracking(this MyEntity thisEntity)
        {
            if (thisEntity is MyVoxelBase) return;
            if(ProceduralGenerator.Static != null)
            {
                ProceduralGenerator.Static.TrackEntity(thisEntity);
            }
        }

    }
}
