using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Utilities;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.Entity;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Sessioncomponent that manages the tracking of Entities for other components
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 501)]
    public class MyEntityTrackerComponent : MySessionComponentBase
    {
        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        public static MyEntityTrackerComponent Static;

        /// <summary>
        /// List of IMyPlayerTrackers, that want to track players.
        /// </summary>
        private List<IMyEntityTracker> m_entityTrackers;

        /// <summary>
        /// All entities, that are tracked by all currently registered EntityTrackers.
        /// </summary>
        private List<MyEntity> m_trackedEntities;

        /// <summary>
        /// All entities that are not yet tracked by all EntityTrackers.
        /// </summary>
        private List<MyEntity> m_newTrackedEntities;

        /// <summary>
        /// all entities that should get untracked from all EntityTrackers.
        /// </summary>
        private List<MyEntity> m_toUntrackEntities;

        /// <summary>
        /// Runs before simulation update. If the plugin is enabled, will
        /// notify each registered EntityTracker about newly tracked entities and
        /// will refresh the tracked entity list.
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (!MySettingsSession.Static.IsEnabled() || !Sync.IsServer) return;

            lock (m_toUntrackEntities) lock (m_entityTrackers)
            {
                foreach(var entity in m_toUntrackEntities)
                {
                    foreach(var tracker in m_entityTrackers)
                    {
                        tracker.UntrackEntity(entity);
                    }
                    m_trackedEntities.Remove(entity);
                    m_newTrackedEntities.Remove(entity);
                }
                m_toUntrackEntities.Clear();
            }

            lock (m_newTrackedEntities) lock (m_trackedEntities) lock (m_entityTrackers)
            {
                foreach(var entity in m_newTrackedEntities)
                {
                    foreach(var tracker in m_entityTrackers)
                    {
                        tracker.TrackEntity(entity);
                    }
                    m_trackedEntities.Add(entity);
                }
                m_newTrackedEntities.Clear();
            }
        }

        /// <summary>
        /// Loads necessary data for this class and initializes the fields
        /// </summary>
        public override void LoadData()
        {
            MyPluginLog.Log("Entity tracker loading data");

            base.LoadData();

            Static = this;
            m_entityTrackers = new List<IMyEntityTracker>();
            m_trackedEntities = new List<MyEntity>();
            m_newTrackedEntities = new List<MyEntity>();
            m_toUntrackEntities = new List<MyEntity>();

            MyEntities.OnEntityAdd += TrackEntity;

            MyPluginLog.Log("Entity tracker loading data completed");
        }

        protected override void UnloadData()
        {
            base.UnloadData();

            MyEntities.OnEntityAdd -= TrackEntity;
        }

        /// <summary>
        /// Registers a new entity, that should get tracked.
        /// </summary>
        /// <param name="entity">Entity to register</param>
        public void TrackEntity(MyEntity entity)
        {
            if (!MySettingsSession.Static.IsEnabled() || !Sync.IsServer) return;

            
            if (entity is MyCharacter)
            {
                if (m_trackedEntities == null || m_newTrackedEntities == null || m_trackedEntities.Contains(entity)) return;
    
                m_newTrackedEntities.Add(entity);
                entity.OnMarkForClose += OnEntityClose;
            }
            if (entity is MyCubeGrid grid)
            {
                if (m_trackedEntities == null || m_newTrackedEntities == null || m_trackedEntities.Contains(entity)) return;

                if (grid.PlayerPresenceTier == VRage.Game.ModAPI.MyUpdateTiersPlayerPresence.Normal)
                {
                    m_newTrackedEntities.Add(entity);
                    entity.OnMarkForClose += OnEntityClose;
                    grid.PlayerPresenceTierChanged += OnGridPlayerPresenceUpdate;
                }
            }
        }

        /// <summary>
        /// Registers a new Entity tracker and notifies him about all
        /// currently tracked entities.
        /// Only run in or after init phase
        /// </summary>
        /// <param name="tracker">Tracker to register</param>
        public void RegisterTracker(IMyEntityTracker tracker)
        {
            m_entityTrackers.Add(tracker);

            foreach (var entity in m_trackedEntities)
            {
                tracker.TrackEntity(entity);
            }
        }

        /// <summary>
        /// Unregisters an Entity tracker
        /// Only run in or after init phase
        /// </summary>
        /// <param name="tracker"></param>
        public void UnregisterTracker(IMyEntityTracker tracker)
        {
            m_entityTrackers.Remove(tracker);
        }

        /// <summary>
        /// Delegate used entity tracking for grids according to Player presence
        /// </summary>
        /// <param name="grid">The grid</param>
        private void OnGridPlayerPresenceUpdate(MyCubeGrid grid)
        {
            if(grid.PlayerPresenceTier != VRage.Game.ModAPI.MyUpdateTiersPlayerPresence.Normal)
            {
                lock(m_toUntrackEntities)
                    m_toUntrackEntities.Add(grid);
            }
        }

        /// <summary>
        /// Delegate used to remove closing entities
        /// </summary>
        /// <param name="e">The entity</param>
        private void OnEntityClose(MyEntity e)
        {
            lock (m_toUntrackEntities)
                m_toUntrackEntities.Add(e);

            e.OnMarkForClose -= OnEntityClose;

            if(e is MyCubeGrid grid)
            {
                grid.PlayerPresenceTierChanged -= OnGridPlayerPresenceUpdate;
            }
        }
    }
}
