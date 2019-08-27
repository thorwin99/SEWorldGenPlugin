using Sandbox;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.SaveItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Library.Utils;
using VRage.Network;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralWorld
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 500, typeof(MyObjectBuilder_WorldGenerator), null)]
    [StaticEventOwner]
    public class MyCustomProceduralWorldGenerator : MyProceduralWorldGenerator
    {
        private Dictionary<MyEntity, MyEntityTracker> m_trackedEntities = new Dictionary<MyEntity, MyEntityTracker>();

        private Dictionary<MyEntity, MyEntityTracker> m_toAddTrackedEntities = new Dictionary<MyEntity, MyEntityTracker>();

        private HashSet<MyObjectSeedParams> m_existingObjectsSeeds = new HashSet<MyObjectSeedParams>();

        private List<MyProceduralCell> m_tempProceduralCellsList = new List<MyProceduralCell>();

        private List<MyObjectSeed> m_tempObjectSeedList = new List<MyObjectSeed>();

        private MyProceduralAsteroidRingCellGenerator m_asteroidRingModule;

        private HashSet<EmptyArea> m_markedAreas = new HashSet<EmptyArea>();

        private HashSet<EmptyArea> m_deletedAreas = new HashSet<EmptyArea>();

        public static MyCustomProceduralWorldGenerator StaticGen;

        public override void LoadData()
        {
            base.LoadData();

            if (true)
            {
                MyObjectBuilder_SessionSettings settings = MySession.Static.Settings;
                MySandboxGame.Log.WriteLine("Loading Procedural Asteroid rings module");
                using (MyRandom.Instance.PushSeed(settings.ProceduralSeed))
                {
                    m_asteroidRingModule = new MyProceduralAsteroidRingCellGenerator(null, settings.ProceduralSeed, 0.75);
                    //m_modules.Add(m_asteroidsModule);
                }
                if(Static != null)
                    Static = this;
                StaticGen = this;
            }
        }

        public override bool UpdatedBeforeInit()
        {
            return true;
        }

        protected override void UnloadData()
        {
            MySandboxGame.Log.WriteLine("Unloading Custom Procedural World Generator for Asteroid Rings");
            m_asteroidRingModule = null;
            m_trackedEntities.Clear();
            m_tempObjectSeedList.Clear();
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (m_asteroidRingModule == null) return;
            if (Enabled)
            {
                if (m_toAddTrackedEntities.Count != 0)
                {
                    foreach (KeyValuePair<MyEntity, MyEntityTracker> toAddTrackedEntity in m_toAddTrackedEntities)
                    {
                        m_trackedEntities.Add(toAddTrackedEntity.Key, toAddTrackedEntity.Value);
                    }
                    m_toAddTrackedEntities.Clear();
                }
                foreach (MyEntityTracker value in m_trackedEntities.Values)
                {
                    MyLog.Default.WriteLine("ProcGen: Checking tracked entity");
                    if (value.ShouldGenerate())
                    {
                        BoundingSphereD boundingVolume = value.BoundingVolume;
                        value.UpdateLastPosition();
                        m_asteroidRingModule.GetObjectSeeds(value.BoundingVolume, m_tempObjectSeedList);
                        m_asteroidRingModule.GenerateObjects(m_tempObjectSeedList, m_existingObjectsSeeds);
                        m_tempObjectSeedList.Clear();
                        m_asteroidRingModule.MarkCellsDirty(boundingVolume, value.BoundingVolume);
                    }
                }
                m_asteroidRingModule.ProcessDirtyCells(m_trackedEntities);
            }
            if (!MySandboxGame.AreClipmapsReady && MySession.Static.VoxelMaps.Instances.Count == 0 && Sync.IsServer)
            {
                MySandboxGame.AreClipmapsReady = true;
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_WorldGenerator myObjectBuilder_WorldGenerator = (MyObjectBuilder_WorldGenerator)sessionComponent;
            if (!Sync.IsServer)
            {
                m_markedAreas = myObjectBuilder_WorldGenerator.MarkedAreas;
            }
            m_deletedAreas = myObjectBuilder_WorldGenerator.DeletedAreas;
            m_existingObjectsSeeds = myObjectBuilder_WorldGenerator.ExistingObjectsSeeds;
            if (m_markedAreas == null)
            {
                m_markedAreas = new HashSet<EmptyArea>();
            }
            foreach (EmptyArea markedArea in m_markedAreas)
            {
                MarkModules(markedArea.Position, markedArea.Radius, planet: true);
            }
            foreach (EmptyArea deletedArea in m_deletedAreas)
            {
                MarkModules(deletedArea.Position, deletedArea.Radius, planet: false);
            }
        }

        private void MarkModules(Vector3D pos, float radius, bool planet)
        {
            MySphereDensityFunction func = (!planet) ? new MySphereDensityFunction(pos, radius, 0.0) : new MySphereDensityFunction(pos, (double)radius * 1.1 + 16000.0, 16000.0);
            m_asteroidRingModule.AddDensityFunctionRemoved(func);
        }

        [Event(null, 551)]
        [Reliable]
        [ServerInvoked]
        [Broadcast]
        public static new void AddExistingObjectsSeed(MyObjectSeedParams seed)
        {
            MyProceduralWorldGenerator.AddExistingObjectsSeed(seed);
            if(Static.GetType() == typeof(MyCustomProceduralWorldGenerator))
                ((MyCustomProceduralWorldGenerator)Static).m_existingObjectsSeeds.Add(seed);
        }

        public new void TrackEntity(MyEntity entity)
        {
            MyLog.Default.WriteLine("Tracking new Entity");
            if (!Enabled)
            {
                return;
            }
            if (entity is MyCharacter)
            {
                int num = MySession.Static.Settings.ViewDistance;
                if (MyFakes.USE_GPS_AS_FRIENDLY_SPAWN_LOCATIONS)
                {
                    num = 50000;
                }
                TrackEntity(entity, num);
            }
            MyCubeGrid myCubeGrid;
            if ((myCubeGrid = (entity as MyCubeGrid)) != null && !myCubeGrid.IsStatic)
            {
                TrackEntity(entity, entity.PositionComp.WorldAABB.HalfExtents.Length());
                myCubeGrid.OnStaticChanged += OnGridStaticChanged;
            }
        }

        private void OnGridStaticChanged(MyCubeGrid grid, bool newIsStatic)
        {
            if (!newIsStatic)
            {
                this.TrackEntity(grid, grid.PositionComp.WorldAABB.HalfExtents.Length());
            }
            else
            {
                this.RemoveTrackedEntity(grid);
            }
        }

        private void TrackEntity(MyEntity entity, double range)
        {
            if (m_trackedEntities.TryGetValue(entity, out MyEntityTracker value) || m_toAddTrackedEntities.TryGetValue(entity, out value))
            {
                value.Radius = range;
                return;
            }
            value = new MyEntityTracker(entity, range);
            m_toAddTrackedEntities.Add(entity, value);
            entity.OnClose += delegate (MyEntity e)
            {
                RemoveTrackedEntity(e);
                MyCubeGrid myCubeGrid;
                if ((myCubeGrid = (entity as MyCubeGrid)) != null)
                {
                    myCubeGrid.OnStaticChanged -= OnGridStaticChanged;
                }
            };
        }

        public new void RemoveTrackedEntity(MyEntity entity)
        {
            if (m_trackedEntities.TryGetValue(entity, out MyEntityTracker value))
            {
                m_trackedEntities.Remove(entity);
                m_toAddTrackedEntities.Remove(entity);
                m_asteroidRingModule.MarkCellsDirty(value.BoundingVolume);
            }
        }

        public void AddRing(PlanetRingItem ring)
        {
            m_asteroidRingModule.AddRing(ring);
        }
    }
}
