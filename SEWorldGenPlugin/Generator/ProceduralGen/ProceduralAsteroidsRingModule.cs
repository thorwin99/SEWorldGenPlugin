using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.SaveItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.Library.Utils;
using VRage.Noise;
using VRage.Profiler;
using VRage.Voxels;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public class ProceduralAsteroidsRingModule : ProceduralModule
    {
        private const int OBJECT_SIZE_MIN = 64;
        private const int OBJECT_SIZE_MAX = 512;
        private const int SUBCELL_SIZE = 4 * 1024 + OBJECT_SIZE_MAX * 2;
        private const int SUBCELLS = 3;

        private List<PlanetRingItem> m_asteroidRings;

        public ProceduralAsteroidsRingModule(int seed) : base(seed, SUBCELLS * SUBCELL_SIZE)
        {
            m_asteroidRings = new List<PlanetRingItem>();
        }

        public override ProceduralCell GenerateCell(ref Vector3I id)
        {
            ProceduralCell cell = new ProceduralCell(id, CELL_SIZE);
            int cellSeed = GetCellSeed(ref id);

            using (MyRandom.Instance.PushSeed(cellSeed))
            {
                int index = 0;

                Vector3I subcellId = Vector3I.Zero;
                Vector3I max = new Vector3I(SUBCELLS - 1);

                for(var it = new Vector3I_RangeIterator(ref Vector3I.Zero, ref max); it.IsValid(); it.GetNext(out subcellId))
                {
                    Vector3D position = new Vector3D(MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble());
                    position += (Vector3D)subcellId / SUBCELL_SIZE;
                    position += id;
                    position *= CELL_SIZE;

                    if (!MyEntities.IsInsideWorld(position)) continue;

                    int ringIndex = IsInsideRing(position);

                    if (ringIndex == -1) continue;

                    double density = m_asteroidRings[ringIndex].Density;//Currently useless...

                    //TODO: Add density functionality.

                    var cellObject = new CellObject(cell, position, MyRandom.Instance.Next(OBJECT_SIZE_MIN, OBJECT_SIZE_MAX));
                    cellObject.Params.Type = MyObjectSeedType.Asteroid;
                    cellObject.Params.Seed = MyRandom.Instance.Next();
                    cellObject.Params.Index = index++;
                }
            }

            return cell;
        }

        public override void GenerateObjects(List<CellObject> objects, HashSet<MyObjectSeedParams> existingObjectSeeds)
        {
            ProfilerShort.Begin("GenerateAsteroids");

            List<MyVoxelBase> tmp_voxelMaps = new List<MyVoxelBase>();

            foreach(var obj in objects)
            {
                if (obj.Params.Generated || existingObjectSeeds.Contains(obj.Params)) continue;

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
                            existingObjectSeeds.Add(obj.Params);
                            exists = true;
                        }
                    }

                    if (!exists)
                    {
                        var provider = MyCompositeShapeProvider.CreateAsteroidShape(obj.Params.Seed, obj.Size, MySession.Static.Settings.VoxelGeneratorVersion);
                        var storage = new MyOctreeStorage(provider, GetAsteroidVoxelSize(obj.Size));

                        MyVoxelMap voxelMap = new MyVoxelMap();

                        voxelMap.EntityId = MyRandom.Instance.NextLong();
                        voxelMap.Init(storageName, storage, obj.BoundingVolume.Center - VRageMath.MathHelper.GetNearestBiggerPowerOfTwo(obj.Size) / 2);
                        MyEntities.RaiseEntityCreated(voxelMap);
                        MyEntities.Add(voxelMap);

                        voxelMap.Save = false;
                        MyVoxelBase.StorageChanged OnStorageRangeChanged = null;
                        OnStorageRangeChanged = delegate (MyVoxelBase voxel, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData)
                        {
                            voxelMap.Save = true;
                            voxelMap.RangeChanged -= OnStorageRangeChanged;
                        };
                        voxelMap.RangeChanged += OnStorageRangeChanged;
                    }
                    tmp_voxelMaps.Clear();
                }

                obj.Params.Generated = true;
            }
            ProfilerShort.End();
        }

        private int IsInsideRing(Vector3D position)
        {
            foreach(PlanetRingItem ring in m_asteroidRings)
            {
                AsteroidRingShape shape = AsteroidRingShape.CreateFromRingItem(ring);
                if (shape.Contains(position) == ContainmentType.Contains) return m_asteroidRings.IndexOf(ring);
            }
            return -1;
        }

        private Vector3I GetAsteroidVoxelSize(double asteroidRadius)
        {
            int radius = Math.Max(64, (int)Math.Ceiling(asteroidRadius));
            return new Vector3I(radius);
        }

        public override void UnloadCells()
        {
            if (m_toUnloadCells.Count == 0) return;

            List<CellObject> cellObjects = new List<CellObject>();

            foreach (var cell in m_toUnloadCells)
            {
                cell.GetAllObjects(cellObjects);

                foreach(CellObject obj in cellObjects)
                {
                    if (obj.Params.Generated)
                    {
                        UnloadAsteroid(obj);
                    }
                }
                cellObjects.Clear();
            }
            m_toUnloadCells.Clear();
        }

        private void UnloadAsteroid(CellObject obj)
        {
            List<MyVoxelBase> voxelMaps = new List<MyVoxelBase>();
            var bounds = obj.BoundingVolume;

            MyGamePruningStructure.GetAllVoxelMapsInBox(ref bounds, voxelMaps);

            string storageName = string.Format("Asteroid_{0}_{1}_{2}_{3}_{4}", obj.CellId.X, obj.CellId.Y, obj.CellId.Z, obj.Params.Index, obj.Params.Seed);

            foreach(MyVoxelBase map in voxelMaps)
            {
                if(map.StorageName == storageName)
                {
                    if (!map.Save)
                    {
                        map.Close();
                    }
                    break;
                }
            }

            voxelMaps.Clear();
        }
    }
}
