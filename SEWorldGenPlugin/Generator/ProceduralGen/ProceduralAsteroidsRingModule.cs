using Sandbox;
using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Voxels;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Library.Utils;
using VRage.Profiler;
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
        private const int SUBCELL_SIZE = OBJECT_SIZE_MAX * 3;
        private const int SUBCELLS = 6;

        private HashSet<MyVoxelBase> m_NotSavedMaps;
        private bool m_saving;
        private MyAsteroidGeneratorDefinition m_data;

        public ProceduralAsteroidsRingModule(int seed) : base(seed, SUBCELLS * SUBCELL_SIZE)
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
        }

        public override MyProceduralCell GenerateCell(ref Vector3I id)
        {
            MyProceduralCell cell = new MyProceduralCell(id, CELL_SIZE);
            int cellSeed = GetCellSeed(ref id);

            using (MyRandom.Instance.PushSeed(cellSeed))
            {
                int index = 0;

                Vector3I subcellId = Vector3I.Zero;
                Vector3I max = new Vector3I(SUBCELLS - 1);

                for(var it = new Vector3I_RangeIterator(ref Vector3I.Zero, ref max); it.IsValid(); it.GetNext(out subcellId))
                {
                    Vector3D position = new Vector3D(MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble());
                    position += (Vector3D)subcellId;
                    position *= SUBCELL_SIZE;
                    position += id * CELL_SIZE;

                    if (!MyEntities.IsInsideWorld(position)) continue;

                    MySystemItem obj = IsInsideRing(position);

                    if (obj == null) continue;
                    int minSize = OBJECT_SIZE_MIN;

                    if (obj.Type == SystemObjectType.BELT)
                        minSize = ((MySystemBeltItem)obj).RoidSize;

                    if (obj.Type == SystemObjectType.RING)
                        minSize = ((MyPlanetRingItem)obj).RoidSize;

                    var cellObject = new MyObjectSeed(cell, position, MyRandom.Instance.Next(Math.Min(OBJECT_SIZE_MAX, minSize), OBJECT_SIZE_MAX));
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
                        var provider = MyPluginCompositeShapeProvider.CreateAsteroidShape(obj.Params.Seed, obj.Size, m_data.UseGeneratorSeed ? obj.Params.GeneratorSeed : 0);
                        var storage = new MyOctreeStorage(provider, GetAsteroidVoxelSize(obj.Size));

                        Vector3D pos = obj.BoundingVolume.Center - MathHelper.GetNearestBiggerPowerOfTwo(obj.Size) / 2;

                        MyVoxelMap voxelMap;

                        voxelMap = MyWorldGenerator.AddVoxelMap(storageName, storage, pos, GetAsteroidEntityId(storageName));
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
