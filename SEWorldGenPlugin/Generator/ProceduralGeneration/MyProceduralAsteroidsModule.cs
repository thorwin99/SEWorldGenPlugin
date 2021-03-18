using Sandbox.Definitions;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using VRage;
using VRage.Library.Utils;
using VRage.Voxels;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGeneration
{

    /// <summary>
    /// Procedural generator module, which generates all asteroids for the plugin
    /// </summary>
    public class MyProceduralAsteroidsModule : MyAbstractProceduralCellModule
    {
        /// <summary>
        /// Cell size parameters
        /// </summary>
        private const double OBJECT_SIZE_MAX = 1024;
        private const double CELL_SIZE = OBJECT_SIZE_MAX * 10;

        /// <summary>
        /// Reflexion of asteroid creation method of SE
        /// </summary>
        private MethodInfo m_createRoid;
        private Type m_providerType;

        /// <summary>
        /// List of asteroids, that should only get synced up, but not saved.
        /// </summary>
        private List<MyVoxelBase> m_tmpAsteroids;
        private bool m_isSaving = false;

        private MyAsteroidGeneratorDefinition m_definition;

        public MyProceduralAsteroidsModule(int seed) : base(seed, CELL_SIZE)
        {
            m_tmpAsteroids = new List<MyVoxelBase>();
            m_providerType = typeof(MyProceduralWorldGenerator).Assembly.GetType("Sandbox.Game.World.Generator.MyCompositeShapeProvider");
            m_createRoid = m_providerType.GetMethod("CreateAsteroidShape");
            m_definition = GetData();

            MySession.Static.OnSavingCheckpoint += delegate
            {
                foreach(var roid in m_tmpAsteroids)
                {
                    roid.Save = false;
                }
                m_isSaving = true;
            };
        }

        public override void CloseObject(MyObjectSeed seed)
        {
            m_existingObjectSeeds.Remove(seed);
            MyVoxelBase voxelMap;

            if(seed.UserData != null)
            {
                if(seed.UserData is MyVoxelBase)
                {
                    voxelMap = seed.UserData as MyVoxelBase;

                    //If the reference cant be found, search the array by comparing storage names.
                    //Only a failsafe, if somehow the voxelmap references differs, should not happen normally.
                    if (!m_tmpAsteroids.Remove(voxelMap))
                    {
                        for (int i = 0; i < m_tmpAsteroids.Count; i++)
                        {
                            if (m_tmpAsteroids[i].StorageName.Equals(voxelMap.StorageName))
                            {
                                m_tmpAsteroids.RemoveAt(i);
                                voxelMap.Save = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        voxelMap.Save = false;
                    }

                    voxelMap.IsSeedOpen = false;
                    seed.UserData = null;

                    voxelMap.Close();
                }
            }
            else
            {
                List<MyVoxelBase> voxelMaps = new List<MyVoxelBase>();
                var bounds = seed.BoundingVolume;

                MyGamePruningStructure.GetAllVoxelMapsInBox(ref bounds, voxelMaps);

                string storageName = string.Format("Asteroid_{0}_{1}_{2}_{3}_{4}", seed.CellId.X, seed.CellId.Y, seed.CellId.Z, seed.Params.Index, seed.Params.Seed);

                foreach (MyVoxelBase map in voxelMaps)
                {
                    if (map.StorageName == storageName)
                    {
                        if (m_tmpAsteroids.Contains(map))
                            map.Save = false;

                        m_tmpAsteroids.Remove(map);
                        map.IsSeedOpen = false;
                        map.Close();
                        break;
                    }
                }

                voxelMaps.Clear();
            }
        }

        public override void GenerateLoadedCellObjects()
        {
            List<MyObjectSeed> seeds = new List<MyObjectSeed>();

            foreach(var cell in m_loadedCells.Values)
            {
                cell.GetAll(seeds, false);
            }

            List<MyVoxelBase> tmp_voxelMaps = new List<MyVoxelBase>();

            foreach(var seed in seeds)
            {
                if (seed.Params.Generated) continue;
                if (seed.Params.Type != VRage.Game.MyObjectSeedType.Asteroid) continue;

                seed.Params.Generated = true;

                using (MyRandom.Instance.PushSeed(GetObjectIdSeed(seed)))
                {
                    tmp_voxelMaps.Clear();

                    var bounds = seed.BoundingVolume;
                    MyGamePruningStructure.GetAllVoxelMapsInBox(ref bounds, tmp_voxelMaps);

                    string storageName = string.Format("Asteroid_{0}_{1}_{2}_{3}_{4}", seed.CellId.X, seed.CellId.Y, seed.CellId.Z, seed.Params.Index, seed.Params.Seed);

                    bool exists = false;
                    foreach(var tmp in tmp_voxelMaps)
                    {
                        if (tmp.StorageName == storageName)
                        {
                            if (!m_existingObjectSeeds.Contains(seed))
                            {
                                m_existingObjectSeeds.Add(seed);
                            }
                            exists = true;

                            if (tmp_voxelMaps.Contains(tmp))
                            {
                                tmp.Save = true;
                            }
                            break;
                        }
                    }

                    if (exists) continue;

                    var storage = CreateAsteroidStorage(GetAsteroidVoxelSize(seed.Size), seed.Params.Seed, seed.Size, m_definition.UseGeneratorSeed ? seed.Params.GeneratorSeed : 0);
                    Vector3D pos = seed.BoundingVolume.Center - MathHelper.GetNearestBiggerPowerOfTwo(seed.Size) / 2;

                    MyVoxelMap voxelMap;

                    voxelMap = MyWorldGenerator.AddVoxelMap(storageName, storage, pos, GetAsteroidEntityId(storageName));
                    if (voxelMap == null) continue;

                    MyVoxelBase.StorageChanged del = null;
                    del = delegate (MyVoxelBase voxel, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData)
                    {
                        voxel.Save = true;
                        m_tmpAsteroids.Remove(voxel);
                        voxel.RangeChanged -= del;
                    };
                    voxelMap.RangeChanged += del;

                    voxelMap.IsSeedOpen = true;

                    seed.UserData = voxelMap;
                    m_tmpAsteroids.Add(voxelMap);
                }
            }
        }

        public override void UpdateGpsForPlayer(MyEntityTracker entity)
        {
            if (!(entity.Entity is MyCharacter)) return;

            var settings = MySettingsSession.Static.Settings.GeneratorSettings.GPSSettings;
            if (settings.AsteroidGPSMode != MyGPSGenerationMode.DISCOVERY) return;

            var objects = MyStarSystemGenerator.Static.StarSystem.GetAllObjects();
            MyCharacter player = entity.Entity as MyCharacter;

            foreach(var obj in objects)
            {
                if (obj.Type != MySystemObjectType.ASTEROIDS) continue;

                MySystemAsteroids asteroid = obj as MySystemAsteroids;
                Vector3D entityPosition = player.PositionComp.GetPosition();
                var provider = MyAsteroidObjectsManager.Static.AsteroidObjectProviders[asteroid.AsteroidTypeName];

                IMyAsteroidObjectShape shape = provider.GetAsteroidObjectShape(asteroid);

                if (shape == null) continue;

                if(shape.Contains(entityPosition) != ContainmentType.Disjoint)
                {
                    MyGPSManager.Static.RemoveDynamicGps(player.GetPlayerIdentityId(), asteroid.Id);
                }

                Vector3D closestPos = shape.GetClosestPoint(entityPosition);
                double distance = Vector3D.Subtract(entityPosition, closestPos).Length();
                if (distance > 5000000)
                {
                    MyGPSManager.Static.RemoveDynamicGps(player.GetPlayerIdentityId(), asteroid.Id);
                }
                else if(distance > 5)
                {
                    if(MyGPSManager.Static.DynamicGpsExists(asteroid.Id, player.GetPlayerIdentityId()))
                    {
                        MyGPSManager.Static.ModifyDynamicGps(asteroid.DisplayName, Color.Yellow, closestPos, player.GetPlayerIdentityId(), asteroid.Id);
                        continue;
                    }
                    MyGPSManager.Static.AddDynamicGps(asteroid.DisplayName, Color.Yellow, closestPos, player.GetPlayerIdentityId(), asteroid.Id);
                }
                else
                {
                    MyGPSManager.Static.RemoveDynamicGps(player.GetPlayerIdentityId(), asteroid.Id);
                }
            }
        }

        protected override MyProceduralCell GenerateCellSeeds(Vector3I cellId)
        {
            if (m_loadedCells.ContainsKey(cellId)) return null;

            var settings = MySettingsSession.Static.Settings.GeneratorSettings;

            if (settings.AsteroidGenerator == AsteroidGenerationMethod.VANILLA) return null;

            MyProceduralCell cell = new MyProceduralCell(cellId, CELL_SIZE);
            int cellSeed = CalculateCellSeed(cellId);
            int index = 0;
            double subCellSize = OBJECT_SIZE_MAX * 1f / settings.AsteroidDensity;
            int subcells = (int)(CELL_SIZE / subCellSize);

            using (MyRandom.Instance.PushSeed(cellSeed))
            {
                Vector3I subcellId = Vector3I.Zero;
                Vector3I max = new Vector3I(subcells - 1);

                for (var it = new Vector3I_RangeIterator(ref Vector3I.Zero, ref max); it.IsValid(); it.GetNext(out subcellId))
                {
                    Vector3D position = new Vector3D(MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble());
                    position += (Vector3D)subcellId;
                    position *= subCellSize;
                    position += ((Vector3D)cellId) * CELL_SIZE;

                    if (!MyEntities.IsInsideWorld(position) || (settings.WorldSize >= 0 && position.Length() > settings.WorldSize)) continue;

                    var ring = GetAsteroidObjectAt(position);

                    if (ring == null) continue;

                    var cellObjectSeed = new MyObjectSeed(cell, position, MyRandom.Instance.Next(ring.AsteroidSize.Min, ring.AsteroidSize.Max));
                    cellObjectSeed.Params.Type = VRage.Game.MyObjectSeedType.Asteroid;
                    cellObjectSeed.Params.Seed = MyRandom.Instance.Next();
                    cellObjectSeed.Params.Index = index++;
                    cellObjectSeed.Params.GeneratorSeed = m_definition.UseGeneratorSeed ? MyRandom.Instance.Next() : 0;

                    cell.AddObject(cellObjectSeed);

                    MyPluginLog.Debug("Adding seed");
                }
            }

            return cell;
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
        /// Gets the entity Id of an asteroid based of its storage name.
        /// </summary>
        /// <param name="storageName">The storage name of the asteroid</param>
        /// <returns>A long representing the Entity id of the asteroid</returns>
        private long GetAsteroidEntityId(string storageName)
        {
            long hash = storageName.GetHashCode64();
            return hash & 0x00FFFFFFFFFFFFFF | ((long)MyEntityIdentifier.ID_OBJECT_TYPE.ASTEROID << 56);
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


        /// <summary>
        /// Tries to find a ring in the system, that contains the given position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>The first ring found, that contains the given position.</returns>
        private MySystemAsteroids GetAsteroidObjectAt(Vector3D position)
        {
            var systemObjects = MyStarSystemGenerator.Static.StarSystem.GetAllObjects();

            foreach(var obj in systemObjects)
            {
                if (obj.Type != MySystemObjectType.ASTEROIDS) continue;
                var asteroids = obj as MySystemAsteroids;

                var prov = MyAsteroidObjectsManager.Static.AsteroidObjectProviders[asteroids.AsteroidTypeName];

                IMyAsteroidObjectShape shape = prov.GetAsteroidObjectShape(asteroids);

                if (shape == null) return null;

                if (shape.Contains(position) == ContainmentType.Contains) return obj as MySystemAsteroids;
            }

            return null;
        }

        /// <summary>
        /// Gets the seed for the given cell
        /// </summary>
        /// <param name="cellId">The cell id for the cell to get the seed for</param>
        /// <returns>The cell seed</returns>
        protected int CalculateCellSeed(Vector3I cellId)
        {
            return m_seed + cellId.X * 16785407 + cellId.Y * 39916801 + cellId.Z * 479001599;
        }

        public override void UpdateCells()
        {
            if(m_isSaving && !MySession.Static.IsSaveInProgress)
            {
                foreach(var roid in m_tmpAsteroids)
                {
                    roid.Save = true;
                }
                m_isSaving = false;
            }
        }

        /// <summary>
        /// Retreives the data for the vanilla asteroid generator to
        /// correctly generate asteroids.
        /// </summary>
        /// <returns>The asteroid generator definition</returns>
        private MyAsteroidGeneratorDefinition GetData()
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
