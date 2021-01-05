using VRage.Game.Entity;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Interface for entity tracking
    /// </summary>
    public interface IMyEntityTracker
    {
        /// <summary>
        /// Tracks the given entity
        /// </summary>
        /// <param name="entity">Entity to track</param>
        void TrackEntity(MyEntity entity);

        /// <summary>
        /// Removes the entity from the tracked entities.
        /// </summary>
        /// <param name="entity">Entity to untrack</param>
        void UntrackEntity(MyEntity entity);
    }
}
