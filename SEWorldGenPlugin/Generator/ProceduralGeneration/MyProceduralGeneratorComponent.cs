using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Session;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;

namespace SEWorldGenPlugin.Generator.ProceduralGeneration
{
    /// <summary>
    /// The Procedural Generator session component which handles asteroid and
    /// planet generation of the plugin using both modules. It tracks all entities that
    /// should be considered when generating objects. It is a singleton class
    /// and only one should exist at a time.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 590)]
    public class MyProceduralGeneratorComponent : MySessionComponentBase, IMyEntityTracker
    {
        /// <summary>
        /// Singleton instance of this class, since it should only exists once per session.
        /// </summary>
        public static MyProceduralGeneratorComponent Static;

        /// <summary>
        /// Seed used for generation
        /// </summary>
        private int m_seed;

        /// <summary>
        /// The vanilla procedural density overwritten by this component, IF the plugin
        /// asteroid generator is used.
        /// </summary>
        private int m_procDensity;

        /// <summary>
        /// All currently tracked entities
        /// </summary>
        private Dictionary<MyEntity, MyEntityTracker> m_trackedEntities = new Dictionary<MyEntity, MyEntityTracker>();

        public override void LoadData()
        {
            if (!MySettingsSession.Static.IsEnabled()) return;
        }

        protected override void UnloadData()
        {
            m_trackedEntities.Clear();
        }

        public void TrackEntity(MyEntity entity)
        {
            if (entity == null) return;
            if(entity is MyCharacter)
            {
                TrackEntityWithRange(entity, MySession.Static.Settings.ViewDistance);
            }
            if(entity is MyCubeGrid)
            {
                TrackEntityWithRange(entity, entity.PositionComp.WorldAABB.HalfExtents.Length());
            }
            
        }

        /// <summary>
        /// Tracks the given entity with given range
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="range">Range</param>
        private void TrackEntityWithRange(MyEntity entity, double range)
        {
            if (m_trackedEntities.ContainsKey(entity))
            {
                m_trackedEntities[entity].Radius = range;
            }
            else
            {
                MyEntityTracker tracker = new MyEntityTracker(entity, range);
                m_trackedEntities.Add(entity, tracker);
            }
        }

        public void UntrackEntity(MyEntity entity)
        {
            m_trackedEntities.Remove(entity);
            //TODO: Unload all objects loaded by this entity
        }
    }
}
