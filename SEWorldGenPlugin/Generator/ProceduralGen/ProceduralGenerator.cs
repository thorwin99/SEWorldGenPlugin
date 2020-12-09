using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using Sandbox.ModAPI;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;

/**
 * Code was partly taken from the KSH Github page of Space Engineers (https://github.com/KeenSoftwareHouse/SpaceEngineers)
 * because it already had an implemention for a Procedural generator for its own world generator, which only needed slight modifications,
 * simplifications and adaptations for use with the plugin.
 */

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    /// <summary>
    /// The Procedural Generator session component which handles asteroid and
    /// planet generation of the plugin using both modules. It tracks all entities that
    /// should be considered when generating objects. It is a singleton class
    /// and only one should exist at a time.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 590)]
    public class ProceduralGenerator : MySessionComponentBase
    {
        public static ProceduralGenerator Static;

        private const string STORAGE_FILE = "AsteroidGeneratorData.xml";

        private int m_seed;

        private Dictionary<MyEntity, MyEntityTracker> m_trackedEntities = new Dictionary<MyEntity, MyEntityTracker>();
        private Dictionary<MyEntity, MyEntityTracker> m_toTrackedEntities = new Dictionary<MyEntity, MyEntityTracker>();
        private List<MyEntity> m_toRemoveTrackedEntities = new List<MyEntity>();
        private HashSet<MyObjectSeedParams> m_existingObjectSeeds = new HashSet<MyObjectSeedParams>();
        private ProceduralAsteroidsRingModule asteroidModule = null;
        private ProceduralPlanetModule planetModule = null;

        private float proceduralDensity = 0;
        private bool Enabled = false;

        /// <summary>
        /// Loads data for the asteroid generator. Prepares all modules and
        /// initializes them.
        /// </summary>
        public override void LoadData()
        {
            PluginLog.Log("Loading procedural generator data");
            Static = this;

            m_seed = MySession.Static.Settings.ProceduralSeed;

            planetModule = new ProceduralPlanetModule(m_seed);

            if (!SettingsSession.Static.Settings.Enable || !Sync.IsServer || MySession.Static.Settings.WorldSizeKm != 0) return;

            MyObjectBuilder_AsteroidGenerator b = GetConfig();

            m_existingObjectSeeds = b.ExistingObjectsSeeds;

            proceduralDensity = b.ProceduralDensity == 0 ? MySession.Static.Settings.ProceduralDensity : b.ProceduralDensity;

            if(SettingsSession.Static.Settings.GeneratorSettings.AsteroidGenerator == AsteroidGenerator.PLUGIN)
            {
                MySession.Static.Settings.ProceduralDensity = 0;
                MySession.Static.OnSavingCheckpoint += delegate
                {
                    if (Enabled)
                        MySession.Static.Settings.ProceduralDensity = proceduralDensity;
                };
            }
            else
            {
                if(MySession.Static.Settings.ProceduralDensity != 0 && MySession.Static.Settings.ProceduralDensity != proceduralDensity)
                {
                    proceduralDensity = MySession.Static.Settings.ProceduralDensity;
                }
                else
                {
                    MySession.Static.Settings.ProceduralDensity = proceduralDensity;
                }
            }

            asteroidModule = new ProceduralAsteroidsRingModule(m_seed);
        }

        /// <summary>
        /// Updates the generators and generates all objects that needs to be generated, aswell as unloading
        /// all Cells that need to be unloaded.
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (!Enabled)
                return;

            if (!Sync.IsServer || !SettingsSession.Static.Settings.Enable || MySession.Static.Settings.WorldSizeKm > 0) return;

            if (SettingsSession.Static.Settings.GeneratorSettings.AsteroidGenerator == AsteroidGenerator.PLUGIN)
            {
                MySession.Static.Settings.ProceduralDensity = 0;
            }

            planetModule.GeneratePlanets();
            foreach (var entity in m_toTrackedEntities)
            {
                if (m_trackedEntities.ContainsKey(entity.Key))
                {
                    m_trackedEntities[entity.Key] = entity.Value;
                }
                else
                {
                    m_trackedEntities.Add(entity.Key, entity.Value);
                }
            }

            lock(m_toRemoveTrackedEntities)
            {
                foreach (var entity in m_toRemoveTrackedEntities.ToArray())
                {
                    m_trackedEntities.Remove(entity);
                }
            }
            

            foreach(MyEntityTracker tracker in m_trackedEntities.Values)
            {
                if (tracker.ShouldGenerate(true))
                {
                    List<MyObjectSeed> cellObjects = new List<MyObjectSeed>();

                    var oldBounding = tracker.BoundingVolume;
                    tracker.UpdateLastPosition();

                    if (asteroidModule == null) continue;

                    asteroidModule.GetObjectsInSphere(tracker.BoundingVolume, cellObjects);
                    asteroidModule.GenerateObjects(cellObjects, m_existingObjectSeeds);

                    asteroidModule.MarkToUnloadCells(oldBounding, tracker.BoundingVolume);

                    if(tracker.Entity is MyCharacter && SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.ShowRingGPS)
                    {
                        asteroidModule.UpdateGps(tracker);
                    }
                }
            }
            if (asteroidModule == null) return;

            asteroidModule.UnloadCells(m_trackedEntities);

            asteroidModule.UpdateObjects();
        }

        /// <summary>
        /// Unloads all Data used by this Session component
        /// </summary>
        protected override void UnloadData()
        {
            PluginLog.Log("unloading procedural generator data");
            Enabled = false;

            asteroidModule = null;

            planetModule = null;

            m_existingObjectSeeds?.Clear();

            m_toTrackedEntities?.Clear();
            m_trackedEntities?.Clear();

            Static = null;
        }

        /// <summary>
        /// Saves the data used by this Session component
        /// </summary>
        public override void SaveData()
        {
            SaveConfig();
        }

        /// <summary>
        /// Tracks a new entity within the Procedural generator, if it is enabled
        /// </summary>
        /// <param name="entity">The to track entity</param>
        public void TrackEntity(MyEntity entity)
        {
            if (!Enabled)
                return;

            if (entity == null)
                return;

            if(entity is MyCharacter)
            {
                TrackEntityRanged(entity, MySession.Static.Settings.ViewDistance);
            }
            if (entity is MyCubeGrid)
            {
                TrackEntityRanged(entity, entity.PositionComp.WorldAABB.HalfExtents.Length());
            }
        }

        /// <summary>
        /// Tracks an entity with given track range. The range determines,
        /// how far the entity affects generation
        /// </summary>
        /// <param name="entity">MyEntity to track</param>
        /// <param name="range">Tracking range</param>
        public void TrackEntityRanged(MyEntity entity, double range)
        {
            MyEntityTracker tracker;

            if (m_trackedEntities.TryGetValue(entity, out tracker) || m_toTrackedEntities.TryGetValue(entity, out tracker))
            {
                tracker.Radius = range;
            }
            else
            {
                tracker = new MyEntityTracker(entity, range);
                m_toTrackedEntities.Add(entity, tracker);
                entity.OnMarkForClose += (e) =>
                {
                    m_toRemoveTrackedEntities.Add(e);
                    asteroidModule.MarkToUnloadCells(tracker.BoundingVolume);
                };
            }
        }

        /// <summary>
        /// Enables the Procedural generator, if it should.
        /// </summary>
        /// <param name="sessionComponent">Session component that is initialized</param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            PluginLog.Log("Initializing procedural generator");

            if (!Sync.IsServer || !SettingsSession.Static.Settings.Enable || MySession.Static.Settings.WorldSizeKm > 0) return;
            base.Init(sessionComponent);

            Enabled = true;

            m_seed = MySession.Static.Settings.ProceduralSeed;
        }

        override public bool UpdatedBeforeInit()
        {
            return true;
        }

        /// <summary>
        /// Gets the session components object builder
        /// </summary>
        /// <returns>MyObjectBuilder_SessionComponent object builder of this component</returns>
        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            return base.GetObjectBuilder();
        }

        /// <summary>
        /// Gets the storage file of the asteroid generator.
        /// </summary>
        /// <returns>Asteroid generator storage file</returns>
        private MyObjectBuilder_AsteroidGenerator GetConfig()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(STORAGE_FILE, typeof(ProceduralGenerator)))
            {
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(STORAGE_FILE, typeof(ProceduralGenerator)))
                    {
                        MyObjectBuilder_AsteroidGenerator saveFile = MyAPIGateway.Utilities.SerializeFromXML<MyObjectBuilder_AsteroidGenerator>(reader.ReadToEnd());
                        return saveFile;
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Log("Couldnt load Starsystem save file.", LogLevel.ERROR);
                    PluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);

                    MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(ProceduralGenerator));
                    return null;
                }
            }
            else
            {
                return new MyObjectBuilder_AsteroidGenerator();
            }
        }

        /// <summary>
        /// Saves the asteroid generator storage file.
        /// </summary>
        private void SaveConfig()
        {
            PluginLog.Log("Saving procedural generator files");
            MyObjectBuilder_AsteroidGenerator conf = new MyObjectBuilder_AsteroidGenerator();

            conf.ExistingObjectsSeeds = m_existingObjectSeeds;
            conf.ProceduralDensity = proceduralDensity;

            MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(ProceduralGenerator));

            string xml = MyAPIGateway.Utilities.SerializeToXML(conf);
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(STORAGE_FILE, typeof(ProceduralGenerator)))
            {
                writer.Write(xml);
                writer.Close();
            }
        }
    }
}
