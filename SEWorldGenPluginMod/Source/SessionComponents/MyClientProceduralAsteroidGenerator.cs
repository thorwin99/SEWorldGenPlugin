using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SEWorldGenPluginMod.Source.Networking;
using SEWorldGenPluginMod.Source.ObjectBuilders;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Library.Utils;
using VRage.Voxels;
using VRageMath;

namespace SEWorldGenPluginMod.Source.SessionComponents
{
    /// <summary>
    /// Class used to generate asteroids client side.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyClientProceduralAsteroidGenerator : MySessionComponentBase
    {
        /// <summary>
        /// Cell size parameters
        /// </summary>
        private const double OBJECT_SIZE_MAX = 1024;
        private const double CELL_SIZE = OBJECT_SIZE_MAX * 10;

        /// <summary>
        /// Whether the component is enabled to ensure it running only on clientside, not on server side
        /// </summary>
        private bool m_enabled;

        /// <summary>
        /// All cells, that should get loaded, if entities are in range of it.
        /// </summary>
        private CachingHashSet<Vector3I> m_toLoadCells = new CachingHashSet<Vector3I>();

        /// <summary>
        /// All currently loaded cells
        /// </summary>
        protected Dictionary<Vector3I, MyProceduralCell> m_loadedCells = new Dictionary<Vector3I, MyProceduralCell>();

        /// <summary>
        /// All currently network pending cells
        /// </summary>
        protected Dictionary<Vector3I, MyProceduralCell> m_pendingCells = new Dictionary<Vector3I, MyProceduralCell>();

        /// <summary>
        /// Tree of cell positions, to get cells based on bounding boxes, spheres etc...
        /// </summary>
        protected MyDynamicAABBTreeD m_cellsTree = new MyDynamicAABBTreeD(Vector3D.Zero);

        /// <summary>
        /// All currently existing object seeds.
        /// </summary>
        protected HashSet<MyObjectSeed> m_existingObjectSeeds = new HashSet<MyObjectSeed>();

        /// <summary>
        /// List of asteroids, that should only get synced up, but not saved.
        /// </summary>
        private List<MyVoxelBase> m_tmpAsteroids = new List<MyVoxelBase>();

        /// <summary>
        /// Current generator settings.
        /// </summary>
        private MyGeneratorSettings m_settings = null;

        /// <summary>
        /// Asteroid generator definition
        /// </summary>
        private MyAsteroidGeneratorDefinition m_definition;

        /// <summary>
        /// If this component is currently waiting for a network response regarding cell position lookups on the server.
        /// </summary>
        private bool m_waitingForResponse = false;

        public override void LoadData()
        {
            base.LoadData();

            m_enabled = !MyAPIGateway.Multiplayer.IsServer;
            m_definition = GetData();
        }

        public override void BeforeStart()
        {
            base.BeforeStart();

            if (!m_enabled) return;

            MyPositionLookupEventHandler.Static.SetCallback(ResponseHandler);
        }

        public override void UpdateBeforeSimulation()
        {
            if (!m_enabled) return;

            var player = MyAPIGateway.Session.Player;
            int range = MyAPIGateway.Session.SessionSettings.ViewDistance;

            if (m_settings == null)
            {
                if (MyGeneratorSettingsSession.Static == null) return;
                m_settings = MyGeneratorSettingsSession.Static.Settings;

                if (m_settings == null) return;
            }

            if (m_waitingForResponse) return;

            MarkToLoadCellsInBounds(new BoundingSphereD(player.GetPosition(), range));
            UnloadCells();
            LoadCells();
            GenerateLoadedCellObjects();
        }

        protected override void UnloadData()
        {
            base.UnloadData();
        }

        /// <summary>
        /// Marks cells to load or keep loaded inside the bounds
        /// </summary>
        /// <param name="bounds">Spherical bounds</param>
        public void MarkToLoadCellsInBounds(BoundingSphereD bounds)
        {
            BoundingBoxD box = BoundingBoxD.CreateFromSphere(bounds);
            Vector3I cellId = Vector3I.Floor(box.Min / CELL_SIZE);

            for (var it = GetCellsIterator(box); it.IsValid(); it.GetNext(out cellId))
            {
                if (m_toLoadCells.Contains(cellId)) continue;

                BoundingBoxD cellBounds = new BoundingBoxD(cellId * CELL_SIZE, (cellId + 1) * CELL_SIZE);
                if (bounds.Contains(cellBounds) == ContainmentType.Disjoint) continue;

                m_toLoadCells.Add(cellId);
            }

            m_toLoadCells.ApplyAdditions();
        }

        /// <summary>
        /// Unloads all marked cells, except those, that are also marked to be loaded, due 
        /// to overlapping bounds when marking.
        /// </summary>
        public void UnloadCells()
        {
            List<MyProceduralCell> unloadCells = new List<MyProceduralCell>();
            foreach (var cell in m_loadedCells.Values)
            {
                if (m_toLoadCells.Contains(cell.CellId)) continue;
                unloadCells.Add(cell);
            }

            foreach (var cell in unloadCells)
            {
                List<MyObjectSeed> seeds = new List<MyObjectSeed>();

                cell.GetAll(seeds);

                foreach (var seed in seeds)
                {
                    if (seed.Generated)
                    {
                        CloseObject(seed);
                    }
                }
                seeds.Clear();

                m_loadedCells.Remove(cell.CellId);
                m_cellsTree.RemoveProxy(cell.ProxyId);
            }

            return;
        }

        /// <summary>
        /// Generates all marked to be loaded cells seeds
        /// </summary>
        public void LoadCells()
        {
            m_toLoadCells.ApplyAdditions();

            foreach (var cellId in m_toLoadCells)
            {
                MyProceduralCell cell = GenerateCellSeeds(cellId);
                if (cell != null)
                {
                    m_pendingCells.Add(cell.CellId, cell);

                    BoundingBoxD aabb = cell.BoundingVolume;
                    cell.ProxyId = m_cellsTree.AddProxy(ref aabb, cell, 0u);
                }
            }

            m_toLoadCells.Clear();

            m_waitingForResponse = true;

            SendLookupRequest();
        }

        /// <summary>
        /// Sends a lookup request for all pending cells regarding their object positions to the server.
        /// Used to determine, whether an object is generated or not.
        /// </summary>
        private void SendLookupRequest()
        {
            List<Vector3D> positions = new List<Vector3D>();
            List<MyObjectSeed> seeds = new List<MyObjectSeed>();

            foreach (var cell in m_pendingCells.Values)
            {
                cell.GetAll(seeds, false);
            }

            foreach (var seed in seeds)
            {
                positions.Add(seed.Position);
            }
        }

        /// <summary>
        /// Handles responses of positional lookup. If size is -1 for an object, it wont be generated.
        /// </summary>
        /// <param name="sizes">List of sizes where there index indicates the object the size corresponds to.</param>
        private void ResponseHandler(List<float> sizes)
        {
            List<MyObjectSeed> seeds = new List<MyObjectSeed>();

            foreach (var cell in m_pendingCells.Values)
            {
                cell.GetAll(seeds, false);
                m_loadedCells.Add(cell.CellId, cell);
            }

            for (int i = 0; i < sizes.Count; i++)
            {
                seeds[i].Size = sizes[i];
            }

            m_pendingCells.Clear();

            m_waitingForResponse = false;
        }

        /// <summary>
        /// Gets the seed for the given cell
        /// </summary>
        /// <param name="cellId">The cell id for the cell to get the seed for</param>
        /// <returns>The cell seed</returns>
        protected int CalculateCellSeed(Vector3I cellId)
        {
            return MyAPIGateway.Session.SessionSettings.ProceduralSeed + cellId.X * 16785407 + cellId.Y * 39916801 + cellId.Z * 479001599;
        }

        /// <summary>
        /// Generates seeds for the given cell
        /// </summary>
        /// <param name="cellId">Id of the cell</param>
        /// <returns>Procedural cell with its seeds.</returns>
        protected MyProceduralCell GenerateCellSeeds(Vector3I cellId)
        {
            if (m_settings.UsePluginGenerator) return null;

            if (m_loadedCells.ContainsKey(cellId)) return null;

            MyProceduralCell cell = new MyProceduralCell(cellId, CELL_SIZE);
            int cellSeed = CalculateCellSeed(cellId);
            int index = 0;
            double subCellSize = OBJECT_SIZE_MAX * 1f / m_settings.AsteroidDensity;
            int subcells = (int)(CELL_SIZE / subCellSize);

            using (MyRandom.Instance.PushSeed(cellSeed))
            {
                Vector3I subcellId = Vector3I.Zero;
                Vector3I max = new Vector3I(subcells - 1);

                List<Vector3D> positions = new List<Vector3D>();

                for (var it = new Vector3I_RangeIterator(ref Vector3I.Zero, ref max); it.IsValid(); it.GetNext(out subcellId))
                {
                    Vector3D position = new Vector3D(MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble(), MyRandom.Instance.NextDouble());
                    position += (Vector3D)subcellId;
                    position *= subCellSize;
                    position += (Vector3D)cellId * CELL_SIZE;

                    if (!MyEntities.IsInsideWorld(position) || m_settings.WorldSize >= 0 && position.Length() > m_settings.WorldSize) continue;

                    var cellObjectSeed = new MyObjectSeed(cell, position, -1);
                    cellObjectSeed.Seed = MyRandom.Instance.Next();
                    cellObjectSeed.Index = index++;
                    cellObjectSeed.GeneratorSeed = m_definition.UseGeneratorSeed ? MyRandom.Instance.Next() : 0;

                    cell.AddObject(cellObjectSeed);
                }
            }

            return cell;
        }

        /// <summary>
        /// Generates all objects inside the currently loaded cells, if they are not
        /// already generated.
        /// </summary>
        public void GenerateLoadedCellObjects()
        {
            List<MyObjectSeed> seeds = new List<MyObjectSeed>();

            foreach (var cell in m_loadedCells.Values)
            {
                cell.GetAll(seeds, false);
            }

            List<MyVoxelBase> tmp_voxelMaps = new List<MyVoxelBase>();

            foreach (var seed in seeds)
            {
                if (seed.Generated) continue;

                seed.Generated = true;

                if (seed.Size < 0) continue;

                using (MyRandom.Instance.PushSeed(GetObjectIdSeed(seed)))
                {
                    tmp_voxelMaps.Clear();

                    var bounds = seed.BoundingVolume;
                    MyGamePruningStructure.GetAllVoxelMapsInBox(ref bounds, tmp_voxelMaps);

                    string storageName = string.Format("Asteroid_{0}_{1}_{2}_{3}_{4}", seed.CellId.X, seed.CellId.Y, seed.CellId.Z, seed.Index, seed.Seed);

                    bool exists = false;
                    foreach (var tmp in tmp_voxelMaps)
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

                    Vector3D pos = seed.BoundingVolume.Center - MathHelper.GetNearestBiggerPowerOfTwo(seed.Size) / 2;

                    MyVoxelMap voxelMap = MyAPIGateway.Session.VoxelMaps.CreateProceduralVoxelMap(seed.Seed, seed.Size, MatrixD.CreateWorld(pos, Vector3D.Forward, Vector3D.Up)) as MyVoxelMap;
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

        /// <summary>
        /// Gets the seed of the object based on the objects hashcode.
        /// </summary>
        /// <param name="obj">The MyObjectSeed for the object</param>
        /// <returns>The seed of the object</returns>
        protected int GetObjectIdSeed(MyObjectSeed obj)
        {
            int hash = obj.CellId.GetHashCode();
            hash = hash * 397 ^ MyAPIGateway.Session.SessionSettings.ProceduralSeed;
            hash = hash * 397 ^ obj.Index;
            hash = hash * 397 ^ obj.Seed;
            return hash;
        }

        /// <summary>
        /// Gets an iterator for all Vector3I within the bounding box bbox
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Vector3I Iterator for all vectors inside bbox</returns>
        protected Vector3I_RangeIterator GetCellsIterator(BoundingBoxD bbox)
        {
            Vector3I min = Vector3I.Floor(bbox.Min / CELL_SIZE);
            Vector3I max = Vector3I.Floor(bbox.Max / CELL_SIZE);

            return new Vector3I_RangeIterator(ref min, ref max);
        }

        /// <summary>
        /// Closes the specified object. This should remove it from the world and memory.
        /// </summary>
        /// <param name="seed">The MyObjectSeed of the object that should be removed.</param>
        public void CloseObject(MyObjectSeed seed)
        {
            m_existingObjectSeeds.Remove(seed);
            MyVoxelBase voxelMap;

            if (seed.UserData != null)
            {
                if (seed.UserData is MyVoxelBase)
                {
                    voxelMap = seed.UserData as MyVoxelBase;

                    if (!m_tmpAsteroids.Contains(voxelMap)) return;

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

                string storageName = string.Format("Asteroid_{0}_{1}_{2}_{3}_{4}", seed.CellId.X, seed.CellId.Y, seed.CellId.Z, seed.Index, seed.Seed);

                foreach (MyVoxelBase map in voxelMaps)
                {
                    if (map.StorageName == storageName)
                    {
                        if (m_tmpAsteroids.Contains(map))
                        {
                            map.Save = false;

                            m_tmpAsteroids.Remove(map);
                            map.IsSeedOpen = false;
                            map.Close();
                            break;
                        }
                    }
                }

                voxelMaps.Clear();
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
            int voxelGeneratorVersion = MyAPIGateway.Session.SessionSettings.VoxelGeneratorVersion;
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
                foreach (MyAsteroidGeneratorDefinition value2 in MyDefinitionManager.Static.GetAsteroidGeneratorDefinitions().Values)
                {
                    if (myAsteroidGeneratorDefinition == null || value2.Version > voxelGeneratorVersion && (myAsteroidGeneratorDefinition.Version < voxelGeneratorVersion || value2.Version < myAsteroidGeneratorDefinition.Version))
                    {
                        myAsteroidGeneratorDefinition = value2;
                    }
                }
                return myAsteroidGeneratorDefinition;
            }
            return myAsteroidGeneratorDefinition;
        }
    }

    /// <summary>
    /// Cell of objects used for procedural generation
    /// </summary>
    public class MyProceduralCell
    {
        /// <summary>
        /// Id of the cell
        /// </summary>
        public Vector3I CellId;

        /// <summary>
        /// Id of the cells proxy
        /// </summary>
        public int ProxyId;

        /// <summary>
        /// Set of seeds in the cell
        /// </summary>
        private HashSet<MyObjectSeed> m_seeds;

        /// <summary>
        /// Bounding box of the cell
        /// </summary>
        public BoundingBoxD BoundingVolume;

        public MyProceduralCell(Vector3I cellId, double cellSize)
        {
            CellId = cellId;
            BoundingVolume = new BoundingBoxD(cellId * cellSize, (cellId + 1) * cellSize);
            m_seeds = new HashSet<MyObjectSeed>();
        }

        /// <summary>
        /// Adds new object seed to the cell
        /// </summary>
        /// <param name="cellObjectSeed">Object seed to add</param>
        public void AddObject(MyObjectSeed cellObjectSeed)
        {
            m_seeds.Add(cellObjectSeed);
        }

        /// <summary>
        /// Puts all object seeds into the <paramref name="seeds"/> list.
        /// Optionally clears the list beforehand
        /// </summary>
        /// <param name="seeds">List to put seeds into</param>
        /// <param name="clear">Whether to clear the list</param>
        public void GetAll(List<MyObjectSeed> seeds, bool clear = true)
        {
            if (clear)
                seeds.Clear();

            foreach (var seed in m_seeds)
            {
                seeds.Add(seed);
            }
        }

        public override int GetHashCode()
        {
            return CellId.GetHashCode();
        }
    }

    /// <summary>
    /// Class representing an object to be generated by the generator
    /// </summary>
    public class MyObjectSeed
    {
        /// <summary>
        /// Position of the object in world space
        /// </summary>
        public Vector3D Position;

        /// <summary>
        /// Whether it is generated or not
        /// </summary>
        public bool Generated;

        /// <summary>
        /// Seed of the object
        /// </summary>
        public int Seed;

        /// <summary>
        /// Size of the object
        /// </summary>
        public float Size;

        /// <summary>
        /// Index of the object in its cell
        /// </summary>
        public int Index;

        /// <summary>
        /// Seed of its generator
        /// </summary>
        public int GeneratorSeed;

        /// <summary>
        /// Boudning box of the object
        /// </summary>
        public BoundingBoxD BoundingVolume;

        /// <summary>
        /// Custom user data
        /// </summary>
        public object UserData;

        /// <summary>
        /// Cell id of the cell this object lies in.
        /// </summary>
        public Vector3I CellId;

        public MyObjectSeed(MyProceduralCell cell, Vector3D position, float size)
        {
            Position = position;
            Size = size;
            Generated = false;
            CellId = cell.CellId;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Seed.GetHashCode();
        }
    }
}
