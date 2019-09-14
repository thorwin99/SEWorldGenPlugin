using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using Sandbox.ModAPI;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;

/*
 * Code is primarily taken from the Space Engineers GitHub repository. 
 */

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 501)]
    public class ProceduralGenerator : MySessionComponentBase
    {
        public static ProceduralGenerator Static;

        private const string STORAGE_FILE = "AsteroidGeneratorData.xml";

        private int m_seed;

        private Dictionary<MyEntity, MyEntityTracker> m_trackedEntities = new Dictionary<MyEntity, MyEntityTracker>();
        private Dictionary<MyEntity, MyEntityTracker> m_toTrackedEntities = new Dictionary<MyEntity, MyEntityTracker>();
        private HashSet<MyObjectSeedParams> m_existingObjectSeeds = new HashSet<MyObjectSeedParams>();
        private ProceduralAsteroidsRingModule asteroidModule = null;
        private ProceduralPlanetModule planetModule = null;

        private bool Enabled = false;

        public override void LoadData()
        {
            MyLog.Default.WriteLine("Loading World Generator");
            Static = this;

            m_seed = MySession.Static.Settings.ProceduralSeed;

            planetModule = new ProceduralPlanetModule(m_seed);

            //Currently so that the original Procedural world generator still works
            if (MySession.Static.Settings.ProceduralDensity != 0) return;

            asteroidModule = new ProceduralAsteroidsRingModule(m_seed);
        }

        public override void UpdateBeforeSimulation()
        {
            if (!Enabled)
                return;

            if (!Sync.IsServer) return;

            foreach(var entity in m_toTrackedEntities)
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
            foreach(MyEntityTracker tracker in m_trackedEntities.Values)
            {
                if (tracker.ShouldGenerate())
                {
                    List<CellObject> cellObjects = new List<CellObject>();

                    var oldBounding = tracker.BoundingVolume;
                    tracker.UpdateLastPosition();

                    planetModule.GeneratePlanets(tracker.Entity);

                    if (asteroidModule == null) continue;

                    asteroidModule.GetObjectsInSphere(tracker.BoundingVolume, cellObjects);
                    asteroidModule.GenerateObjects(cellObjects, m_existingObjectSeeds);

                    asteroidModule.MarkToUnloadCells(oldBounding, tracker.BoundingVolume);
                }
            }
            if (asteroidModule == null) return;

            asteroidModule.UnloadCells();
        }

        protected override void UnloadData()
        {
            Enabled = false;

            asteroidModule = null;

            m_toTrackedEntities.Clear();
            m_trackedEntities.Clear();

            Static = null;
        }

        public override void SaveData()
        {
            SaveConfig();
        }

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
                    m_trackedEntities.Remove(e);
                    m_toTrackedEntities.Remove(e);
                    asteroidModule.MarkToUnloadCells(tracker.BoundingVolume);
                };
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (!Sync.IsServer) return;

            base.Init(sessionComponent);

            MyObjectBuilder_AsteroidGenerator b = GetConfig();

            m_existingObjectSeeds = b.ExistingObjectsSeeds;

            Enabled = true;
        }

        override public bool UpdatedBeforeInit()
        {
            return true;
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            return base.GetObjectBuilder();
        }

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
                    MyLog.Default.Error("Couldnt load Starsystem save file.");
                    MyLog.Default.Error(e.Message + "\n" + e.StackTrace);
                    MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(ProceduralGenerator));
                    return null;
                }
            }
            else
            {
                return new MyObjectBuilder_AsteroidGenerator();
            }
        }

        private void SaveConfig()
        {
            MyObjectBuilder_AsteroidGenerator conf = new MyObjectBuilder_AsteroidGenerator();

            conf.ExistingObjectsSeeds = m_existingObjectSeeds;

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
