using Sandbox.Game.World.Generator;
using VRage.Game.Entity;

namespace SEWorldGenPlugin.Generator.ProceduralGeneration
{
    /// <summary>
    /// Base interface for all Procedural generator modules.
    /// </summary>
    public interface IMyProceduralGeneratorModule
    {
        /// <summary>
        /// Adds all procedural gpss for this player tracker.
        /// </summary>
        /// <param name="entity">Player entity</param>
        void UpdateGpsForPlayer(MyEntityTracker entity);
    }
}
