using Sandbox.Definitions;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using System.Collections.Generic;
using System.Reflection;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Library.Utils;
using VRage.Utils;
using VRage.Voxels;
using VRageMath;

/*
 * Code is primarily taken from the Space Engineers GitHub repository. 
 */

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public class ProceduralAsteroidsRingModule : ProceduralModule
    {
        private const int OBJECT_SIZE_MIN = 128;
        private const int OBJECT_SIZE_MAX = 512;
        private const int CELL_SIZE_ = OBJECT_SIZE_MAX * 20;

        private HashSet<MyVoxelBase> m_NotSavedMaps;
        private bool m_saving;
        private MyAsteroidGeneratorDefinition m_data;
        private MethodInfo m_createRoid;
        private Type m_providerType;
        private float m_density;

        public ProceduralAsteroidsRingModule(int seed) : base(seed, CELL_SIZE_)
        {
            m_NotSavedMaps = new HashSet<MyVoxelBase>();
            MySession.Static.OnSavingCheckpoint += delegate
            {
                foreach(var map in m_NotSavedMaps)
                {
                    map.Save = false;
                }
                m_saving = true;
            };
            m_data = GetData();
            m_providerType = typeof(MyProceduralWorldGenerator).Assembly.GetType("Sandbox.Game.World.Generator.MyCompositeShapeProvider");
            m_createRoid = m_providerType.GetMethod("CreateAsteroidShape");
            m_density = SettingsSession.Static.Settings.GeneratorSettings.AsteroidDensity;
        }

        public override MyProceduralCell GenerateCell(ref Vector3I id)
        {
            MyProceduralCell cell = new MyProceduralCell(id, CELL_SIZE_);
            int cellSeed = GetCellSeed(ref id);

            using (MyRandom.Instance.PushSeed(cellSeed))
            {
                int index = 0;

                int subCellSize = (int)(OBJECT_SIZE_MAX * 2 / m_density);
                int subcells = CELL_SIZE_ / subCellSize;

                Vector3I subcellId = Vector3I.Zero;
                Vector3I max = new Vector3I(subcells - 1);

                for(var it = new Vector3I_RangeIterator(ref Vector3I.Zero, ref max); it.IsValid(); it.GetNext(out subcellId))
                {
                    Vector3D position = new Vector3D(MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble());
                    position += (Vector3D)subcellId;
                    position *= subCellSize;
                    position += id * CELL_SIZE_;

                    if (!MyEntities.IsInsideWorld(position)) continue;

                    MySystemItem obj = IsInsideRing(position);

                    if (obj == null) continue;
                    int minSize = OBJECT_SIZE_MIN;
                    int maxSize = OBJECT_SIZE_MAX;

                    if (obj.Type == SystemObjectType.BELT)
                        minSize = ((MySystemBeltItem)obj).RoidSize;

                    if (obj.Type == SystemObjectType.RING)
                    {
                        minSize = ((MyPlanetRingItem)obj).RoidSize;
                        maxSize = ((MyPlanetRingItem)obj).RoidSizeMax;
                    }

                    var cellObject = new MyObjectSeed(cell, position, MyRandom.Instance.Next(Math.Min(maxSize, minSize), Math.Max(maxSize, minSize)));
                    cellObject.Params.Type = MyObjectSeedType.Asteroid;
                    cellObject.Params.Seed = MyRandom.Instance.Next();
                    cellObject.Params.Index = index++;

                    cell.AddObject(cellObject);
                }
            }

            return cell;
        }

        public override void GenerateObjects(List<MyObjectSeed> objects, HashSet<MyObjectSeedParams> existingObjectSeeds)
        {
            List<MyVoxelBase> tmp_voxelMaps = new List<MyVoxelBase>();

            foreach(var obj in objects)
            {
                if (obj.Params.Generated) continue;

                obj.Params.Generated = true;

                using (MyRandom.Instance.PushSeed(GetObjectIdSeed(obj)))
                {
                    if (obj.Params.Type != MyObjectSeedType.Asteroid) continue;

                    var bounds = obj.BoundingVolume;
                    MyGamePruningStructure.GetAllVoxelMapsInBox(ref bounds, tmp_voxelMaps);

                    string storageName = string.Format("Asteroid_{0}_{1}_{2}_{3}_{4}", obj.CellId.X, obj.CellId.Y, obj.CellId.Z, obj.Params.Index, obj.Params.Seed);

                    bool exists = false;
                    foreach(var voxelMap in tmp_voxelMaps)
                    {
                        if(voxelMap.StorageName == storageName)
                        {
                            if (!existingObjectSeeds.Contains(obj.Params))
                                existingObjectSeeds.Add(obj.Params);
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        var storage = CreateAsteroidStorage(GetAsteroidVoxelSize(obj.Size), obj.Params.Seed, obj.Size, m_data.UseGeneratorSeed ? obj.Params.GeneratorSeed : 0, 3);
                        Vector3D pos = obj.BoundingVolume.Center - MathHelper.GetNearestBiggerPowerOfTwo(obj.Size) / 2;

                        MyVoxelMap voxelMap;

                        voxelMap = MyWorldGenerator.AddVoxelMap(storageName, storage, pos, GetAsteroidEntityId(storageName));
                        if (voxelMap == null) continue;
                        voxelMap.Save = true;

                        m_NotSavedMaps.Add(voxelMap);

                        MyVoxelBase.StorageChanged OnStorageRangeChanged = null;
                        OnStorageRangeChanged = delegate (MyVoxelBase voxel, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData)
                        {
                            voxelMap.Save = true;
                            m_NotSavedMaps.Remove(voxelMap);
                            voxelMap.RangeChanged -= OnStorageRangeChanged;
                        };

                        voxelMap.RangeChanged += OnStorageRangeChanged;
                        
                    }
                    tmp_voxelMaps.Clear();
                }
            }
        }

        public void UpdateGps(MyEntityTracker tracker)
        {
            foreach(MySystemItem p in SystemGenerator.Static.m_objects)
            {
                if (p.Type != SystemObjectType.PLANET || ((MyPlanetItem)p).PlanetRing == null) continue;

                MyPlanetRingItem ring = ((MyPlanetItem)p).PlanetRing;
                MyLog.Default.WriteLine(((MyPlanetItem)p).DisplayName + " Ring");
                Vector3D entityPos = tracker.Entity.PositionComp.GetPosition();

                if (Vector3D.Subtract(ring.Center, entityPos).Length() > 5000000)
                {
                    MyLog.Default.WriteLine(    "Is Out of distance " + entityPos + " at distance " + Vector3D.Subtract(ring.Center, entityPos).Length());
                    GlobalGpsManager.Static.RemoveDynamicGps(((MyPlanetItem)p).DisplayName + " Ring", ((MyCharacter)tracker.Entity).GetPlayerIdentityId());
                    continue;
                }

                AsteroidRingShape shape = AsteroidRingShape.CreateFromRingItem(ring);

                if (shape.Contains(tracker.LastPosition) == ContainmentType.Contains)
                {
                    MyLog.Default.WriteLine(    "Is Inside Ring " + entityPos);
                    GlobalGpsManager.Static.RemoveDynamicGps(((MyPlanetItem)p).DisplayName + " Ring", ((MyCharacter)tracker.Entity).GetPlayerIdentityId());
                    continue;
                }

                Vector3D pos = shape.ClosestPointAtRingCenter(entityPos);

                MyLog.Default.WriteLine(    "Closest position is " + pos);
                GlobalGpsManager.Static.AddOrUpdateDynamicGps(((MyPlanetItem)p).DisplayName + " Ring", ((MyCharacter)tracker.Entity).GetPlayerIdentityId(), pos, Color.Gold);
            }
        }

        private MySystemItem IsInsideRing(Vector3D position)
        {
            foreach(MySystemItem p in SystemGenerator.Static.m_objects)
            {
                if(p.Type == SystemObjectType.PLANET)
                {
                    MyPlanetItem planet = (MyPlanetItem)p;

                    if (planet.PlanetRing == null) continue;
                    AsteroidRingShape shape = AsteroidRingShape.CreateFromRingItem(planet.PlanetRing);
                    if (shape.Contains(position) == ContainmentType.Contains) return planet.PlanetRing;
                }
                else if(p.Type == SystemObjectType.BELT)
                {
                    MySystemBeltItem belt = (MySystemBeltItem)p;
                    AsteroidBeltShape shape = AsteroidBeltShape.CreateFromRingItem(belt);
                    if (shape.Contains(position) == ContainmentType.Contains) return belt;
                }

            }
            return null;
        }

        private Vector3I GetAsteroidVoxelSize(double asteroidRadius)
        {
            int radius = Math.Max(64, (int)Math.Ceiling(asteroidRadius));
            return new Vector3I(radius);
        }

        public override void CloseObject(MyObjectSeed obj)
        {
            List<MyVoxelBase> voxelMaps = new List<MyVoxelBase>();
            var bounds = obj.BoundingVolume;

            MyGamePruningStructure.GetAllVoxelMapsInBox(ref bounds, voxelMaps);

            string storageName = string.Format("Asteroid_{0}_{1}_{2}_{3}_{4}", obj.CellId.X, obj.CellId.Y, obj.CellId.Z, obj.Params.Index, obj.Params.Seed);

            foreach(MyVoxelBase map in voxelMaps)
            {
                if(map.StorageName == storageName)
                {
                    if (m_NotSavedMaps.Contains(map))
                    {
                        Action<MyEntity> onClose = null;
                        onClose = delegate
                        {
                            obj.Params.Generated = false;
                            map.OnClose -= onClose;
                        };
                        map.Save = false;
                        map.Close();
                        map.OnClose += onClose;
                        m_NotSavedMaps.Remove(map);
                    }
                    break;
                }
            }

            voxelMaps.Clear();
        }

        private long GetAsteroidEntityId(string storageName)
        {
            long hash = storageName.GetHashCode64();
            return hash & 0x00FFFFFFFFFFFFFF | ((long)MyEntityIdentifier.ID_OBJECT_TYPE.ASTEROID << 56);
        }

        public override void UpdateObjects()
        {
            if (!m_saving) return;
            foreach(var map in m_NotSavedMaps)
            {
                map.Save = true;
            }
            m_saving = false;
        }

        private MyOctreeStorage CreateAsteroidStorage(Vector3I storageSize, int seed, float size, int generatorSeed = 0, int? generator = default(int?))
        {
            object[] args = new object[] { seed, size, generatorSeed, generator };

            object prov = m_createRoid.Invoke(null, args);

            ConstructorInfo ctor = typeof(MyOctreeStorage).GetConstructor(new Type[] { m_providerType, typeof(Vector3I) });
            MyOctreeStorage instance = (MyOctreeStorage)ctor.Invoke(new object[] { prov, storageSize });
            return instance;
        }

        private static MyAsteroidGeneratorDefinition GetData()
        {
            MyAsteroidGeneratorDefinition myAsteroidGeneratorDefinition = null;
            int voxelGeneratorVersion = MySession.Static.Settings.VoxelGeneratorVersion;
            foreach (MyAsteroidGeneratorDefinition value in MyDefinitionManager.Static.GetAsteroidGeneratorDefinitions().Values)
            {
                if (value.Version == voxelGeneratorVersion)
                {
                    myAsteroidGeneratorDefinition = value;
                    break;
                }
            }
            if (myAsteroidGeneratorDefinition == null)
            {
                MyLog.Default.WriteLine("Generator of version " + voxelGeneratorVersion + "not found!");
                {
                    foreach (MyAsteroidGeneratorDefinition value2 in MyDefinitionManager.Static.GetAsteroidGeneratorDefinitions().Values)
                    {
                        if (myAsteroidGeneratorDefinition == null || (value2.Version > voxelGeneratorVersion && (myAsteroidGeneratorDefinition.Version < voxelGeneratorVersion || value2.Version < myAsteroidGeneratorDefinition.Version)))
                        {
                            myAsteroidGeneratorDefinition = value2;
                        }
                    }
                    return myAsteroidGeneratorDefinition;
                }
            }
            return myAsteroidGeneratorDefinition;
        }
    }
}
