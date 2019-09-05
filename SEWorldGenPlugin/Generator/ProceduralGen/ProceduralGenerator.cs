using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 500, typeof(MyObjectBuilder_WorldGenerator))]
    public class ProceduralGenerator : MySessionComponentBase
    {
        public static ProceduralGenerator Static;

        private int m_seed;

        private Dictionary<MyEntity, MyEntityTracker> m_trackedEntities = new Dictionary<MyEntity, MyEntityTracker>();
        private Dictionary<MyEntity, MyEntityTracker> m_toTrackedEntities = new Dictionary<MyEntity, MyEntityTracker>();
        private HashSet<MyObjectSeedParams> m_existingObjectSeeds = new HashSet<MyObjectSeedParams>();
        private HashSet<EmptyArea> m_areas = new HashSet<EmptyArea>();
        private ProceduralModule module = null;

        private bool Enabled = false;

        public override void LoadData()
        {
            Static = this;

            m_seed = MySession.Static.Settings.ProceduralSeed;

            //Currently so that the original Procedural world generator still works
            if (MySession.Static.Settings.ProceduralDensity != 0) return;

            module = new ProceduralAsteroidsRingModule(m_seed);

            Enabled = true;
        }

        public override void UpdateBeforeSimulation()
        {
            if (!Enabled || module == null)
                return;

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

                    module.GetObjectsInSphere(tracker.BoundingVolume, cellObjects);
                    module.GenerateObjects(cellObjects, m_existingObjectSeeds);

                    module.MarkToUnloadCells(oldBounding, tracker.BoundingVolume);
                }
            }
        }

        protected override void UnloadData()
        {
            Enabled = false;

            module = null;

            m_trackedEntities.Clear();
            Static = null;
        }

        public override void SaveData()
        {
        }

        public void TrackEntity(MyEntity entity)
        {
            if (!Enabled)
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
                };
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            MyObjectBuilder_WorldGenerator b = (MyObjectBuilder_WorldGenerator)sessionComponent;

            m_existingObjectSeeds = b.ExistingObjectsSeeds;
            m_areas = b.MarkedAreas;
        }

        override public bool UpdatedBeforeInit()
        {
            return true;
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_WorldGenerator builder = (MyObjectBuilder_WorldGenerator)base.GetObjectBuilder();
            builder.MarkedAreas = m_areas;
            builder.ExistingObjectsSeeds = m_existingObjectSeeds;
            return builder;
        }
    }
}
