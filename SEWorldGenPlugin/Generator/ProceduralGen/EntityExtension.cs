using Sandbox.Game.Entities;
using VRage.Game.Entity;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    /// <summary>
    /// Extension class for VRage.Game.Entity.MyEntity which holds extension methods.
    /// </summary>
    public static class EntityExtension
    {
        /// <summary>
        /// This method adds the MyEntity to the Procedural Generator as a 
        /// tracked Entity.
        /// </summary>
        /// <param name="thisEntity">The Entiy this method executes on</param>
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
