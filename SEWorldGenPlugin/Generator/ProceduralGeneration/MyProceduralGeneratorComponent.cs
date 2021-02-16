using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
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
        private float m_procDensity;

        /// <summary>
        /// All currently tracked entities
        /// </summary>
        private Dictionary<MyEntity, MyEntityTracker> m_trackedEntities = new Dictionary<MyEntity, MyEntityTracker>();

        /// <summary>
        /// List of all registered cell modules
        /// </summary>
        private List<MyAbstractProceduralCellModule> m_cellModules = new List<MyAbstractProceduralCellModule>();

        /// <summary>
        /// List of all registered object modules
        /// </summary>
        private List<MyAbstractProceduralObjectModul> m_objectModules = new List<MyAbstractProceduralObjectModul>();

        /// <summary>
        /// If the plugin is enabled on this local session. Does not reflect server state.
        /// Used to save some calls in update
        /// </summary>
        private bool m_isEnabled;

        /// <summary>
        /// Final initialization of this component
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            if (MySettingsSession.Static.IsEnabled())
            {
                MyEntityTrackerComponent.Static.RegisterTracker(this);
            }
        }

        /// <summary>
        /// Loads data used by the procedural generator
        /// </summary>
        public override void LoadData()
        {
            if (!MySettingsSession.Static.IsEnabled()) return;

            MyPluginLog.Debug("Loading Procedural Generator Component");

            Static = this;

            m_procDensity = MySession.Static.Settings.ProceduralDensity;
            m_seed = MySession.Static.Settings.ProceduralSeed;

            if (MySettingsSession.Static.Settings.GeneratorSettings.AsteroidGenerator == ObjectBuilders.AsteroidGenerationMethod.PLUGIN)
            {
                MySession.Static.Settings.ProceduralDensity = 0f;
            }

            m_isEnabled = true;

            //Add default impl of registering of standard generator components
            RegisterModule(new MyProceduralPlanetModule(m_seed));
            RegisterModule(new MyProceduralAsteroidsModule(m_seed));
        }

        /// <summary>
        /// Updates all modules
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (!m_isEnabled) return;

            foreach(var objectModule in m_objectModules)
            {
                objectModule.GenerateObjects();
            }

            foreach(var tracker in m_trackedEntities)
            {
                foreach(var module in m_cellModules)
                {
                    var oldBounds = tracker.Value.BoundingVolume;
                    tracker.Value.UpdateLastPosition();

                    module.MarkToLoadCellsInBounds(tracker.Value.BoundingVolume);

                    if(tracker.Key is MyCharacter)
                        module.UpdateGpsForPlayer(tracker.Value);
                }
            }

            foreach (var module in m_cellModules)
            {
                module.UnloadCells();
                module.LoadCells();
                module.GenerateLoadedCellObjects();
                module.UpdateCells();
            }
        }

        /// <summary>
        /// Unloads used data.
        /// </summary>
        protected override void UnloadData()
        {
            m_trackedEntities.Clear();
            m_cellModules.Clear();
            m_objectModules.Clear();
        }

        /// <summary>
        /// Registers a new module for this instance of the procedural generator
        /// </summary>
        /// <param name="module"></param>
        public void RegisterModule(MyAbstractProceduralCellModule module)
        {
            if (!m_cellModules.Contains(module))
            {
                m_cellModules.Add(module);
            }
        }

        /// <summary>
        /// Registers a new module for this instance of the procedural generator
        /// </summary>
        /// <param name="module"></param>
        public void RegisterModule(MyAbstractProceduralObjectModul module)
        {
            if (!m_objectModules.Contains(module))
            {
                m_objectModules.Add(module);
            }
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
