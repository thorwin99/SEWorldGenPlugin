using Sandbox.Game.Entities;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using VRage.Game.Entity;

namespace SEWorldGenPlugin.Generator.ProceduralGenerator
{
    /// <summary>
    /// Extension class for VRage.Game.Entity.MyEntity which holds extension methods.
    /// </summary>
    public static class MyEntityExtension
    {
        /// <summary>
        /// This method adds the MyEntity to the MyEntityTrackerComponent as a 
        /// tracked Entity.
        /// </summary>
        /// <param name="thisEntity">The Entiy this method executes on</param>
        public static void EntityTracking(this MyEntity thisEntity)
        {
            MyPluginLog.Debug("Try Track Entity" + thisEntity.Name);

            if (thisEntity is MyVoxelBase) return;
            if(MyEntityTrackerComponent.Static != null)
            {
                MyPluginLog.Debug("Track Entity");
                MyEntityTrackerComponent.Static.TrackEntity(thisEntity);
            }
        }
    }
}
