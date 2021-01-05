using Sandbox.Game.Entities;
using SEWorldGenPlugin.Session;
using VRage.Game.Entity;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    /// <summary>
    /// Extension class for VRage.Game.Entity.MyEntity which holds extension methods.
    /// </summary>
    public static class EntityExtension
    {
        /// <summary>
        /// This method adds the MyEntity to the MyEntityTrackerComponent as a 
        /// tracked Entity.
        /// </summary>
        /// <param name="thisEntity">The Entiy this method executes on</param>
        public static void EntityTracking(this MyEntity thisEntity)
        {
            if (thisEntity is MyVoxelBase) return;
            if(MyEntityTrackerComponent.Static != null)
            {
                MyEntityTrackerComponent.Static.TrackEntity(thisEntity);
            }
        }
    }
}
