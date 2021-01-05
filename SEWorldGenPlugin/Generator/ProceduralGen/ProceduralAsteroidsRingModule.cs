using Sandbox.Definitions;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Library.Utils;
using VRage.Voxels;
using VRageMath;

/*
 * Code is primarily taken from the Space Engineers GitHub repository. 
 */

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    /// <summary>
    /// A Procedural generator module that generates asteroids for the plugin, contained inside
    /// asteroid rings and asteroid belts.
    /// </summary>
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

        /// <summary>
        /// Creates a new Asteroid generator module instance with the given seed
        /// </summary>
        /// <param name="seed">Seed of the generator</param>
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
            m_density = MySettingsSession.Static.Settings.GeneratorSettings.AsteroidDensity;
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

                    MySystemItem obj = GetContainingRingOrBelt(position);

                    if (obj == null) continue;
                    int minSize = OBJECT_SIZE_MIN;
                    int maxSize = OBJECT_SIZE_MAX;

                    if (obj.Type == LegacySystemObjectType.BELT)
                        minSize = ((MySystemBeltItem)obj).RoidSize;

                    if (obj.Type == LegacySystemObjectType.RING)
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

        /// <summary>
        /// Updates the dynamic gps of asteroid rings that get generated, when a
        /// tracked player is closer than 5000 km to the planet surrounded by it.
        /// </summary>
        /// <param name="tracker"></param>
        public void UpdateGps(MyEntityTracker tracker)
        {
            foreach(MySystemItem p in SystemGenerator.Static.Objects)
            {
                if (p.Type != LegacySystemObjectType.PLANET || ((MyPlanetItem)p).PlanetRing == null) continue;

                MyPlanetRingItem ring = ((MyPlanetItem)p).PlanetRing;
                Vector3D entityPos = tracker.Entity.PositionComp.GetPosition();

                string pre = ((MyPlanetItem)p).DisplayName;
                var center = ((MyPlanetItem)p).CenterPosition;
                int l = (int)(Math.Sqrt(center.X * center.X + center.Y * center.Y + center.Z * center.Z)) % 1000;
                pre = Regex.Replace(pre, @"-[\d\w]+$| \d+ \d+ - \w+$", " " + l.ToString());

                string name = pre + " Ring";

                if (Vector3D.Subtract(ring.Center, entityPos).Length() > 5000000)
                {
                    GlobalGpsManager.Static.RemoveDynamicGps(name, ((MyCharacter)tracker.Entity).GetPlayerIdentityId());
                    continue;
                }

                AsteroidRingShape shape = AsteroidRingShape.CreateFromRingItem(ring);

                if (shape.Contains(tracker.LastPosition) == ContainmentType.Contains)
                {
                    GlobalGpsManager.Static.RemoveDynamicGps(name, ((MyCharacter)tracker.Entity).GetPlayerIdentityId());
                    continue;
                }

                Vector3D pos = shape.ClosestPointAtRingCenter(entityPos);
                GlobalGpsManager.Static.AddOrUpdateDynamicGps(name, ((MyCharacter)tracker.Entity).GetPlayerIdentityId(), pos, Color.Gold);
            }
        }

        /// <summary>
        /// Checks whether the given position is inside an asteroid ring or belt 
        /// and returns the ring or belt.
        /// </summary>
        /// <param name="position">Position ot check</param>
        /// <returns>Returns the ring or belt the position is inside</returns>
        private MySystemItem GetContainingRingOrBelt(Vector3D position)
        {
            foreach(MySystemItem p in SystemGenerator.Static.Objects)
            {
                if(p.Type == LegacySystemObjectType.PLANET)
                {
                    MyPlanetItem planet = (MyPlanetItem)p;

                    if (planet.PlanetRing == null) continue;
                    AsteroidRingShape shape = AsteroidRingShape.CreateFromRingItem(planet.PlanetRing);
                    if (shape.Contains(position) == ContainmentType.Contains) return planet.PlanetRing;
                }
                else if(p.Type == LegacySystemObjectType.BELT)
                {
                    MySystemBeltItem belt = (MySystemBeltItem)p;
                    AsteroidBeltShape shape = AsteroidBeltShape.CreateFromBeltItem(belt);
                    if (shape.Contains(position) == ContainmentType.Contains) return belt;
                }

            }
            return null;
        }

        /// <summary>
        /// Gets the size of the asteroid for its generated voxel storage.
        /// </summary>
        /// <param name="asteroidRadius">Radius of the asteroid</param>
        /// <returns>A Vector3I containing only the size of the voxels</returns>
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

        /// <summary>
        /// Gets the entity Id of an asteroid based of its storage name.
        /// </summary>
        /// <param name="storageName">The storage name of the asteroid</param>
        /// <returns>A long representing the Entity id of the asteroid</returns>
        private long GetAsteroidEntityId(string storageName)
        {
            long hash = storageName.GetHashCode64();
            return hash & 0x00FFFFFFFFFFFFFF | ((long)MyEntityIdentifier.ID_OBJECT_TYPE.ASTEROID << 56);
        }

        // Nasty hack to get the network syncronisation working with objects generated by the plugin.
        // In vanilla SE asteroids are both generated on client and server, while this plugin would only run on the server and
        // needs to sync the asteroids to the clients, which is done by setting them to save, which means they have been modified and therefore
        // beed to be synced. When the game actually saves, those asteroids that have not been modified should not actually be saved to disk, since
        // that would create huge amount of files.
        public override void UpdateObjects()
        {
            if (!m_saving) return;
            foreach(var map in m_NotSavedMaps)
            {
                map.Save = true;
            }
            m_saving = false;
        }

        /// <summary>
        /// Creates a new Asteroid voxel storage.
        /// </summary>
        /// <param name="storageSize">Size of the voxel storage</param>
        /// <param name="seed">Seed of the asteroid</param>
        /// <param name="size">Size of the generated asteroid</param>
        /// <param name="generatorSeed">Seed of the asteroid generator</param>
        /// <param name="generator">The asteroid generator ID</param>
        /// <returns>The storage of the asteroid</returns>
        private MyOctreeStorage CreateAsteroidStorage(Vector3I storageSize, int seed, float size, int generatorSeed = 0, int? generator = default(int?))
        {
            object[] args = new object[] { seed, size, generatorSeed, generator };

            object prov = m_createRoid.Invoke(null, args);

            ConstructorInfo ctor = typeof(MyOctreeStorage).GetConstructor(new Type[] { m_providerType, typeof(Vector3I) });
            MyOctreeStorage instance = (MyOctreeStorage)ctor.Invoke(new object[] { prov, storageSize });
            return instance;
        }

        /// <summary>
        /// Gets the asteroid generator definition used by the Asteroid generator module.
        /// </summary>
        /// <returns>MyAsteroidGeneratorDefinition asteroid generator definition</returns>
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
                MyPluginLog.Log("Generator of version " + voxelGeneratorVersion + "not found!");
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
