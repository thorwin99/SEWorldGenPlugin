using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;

/**
 * Code was partly taken from the KSH Github page of Space Engineers (https://github.com/KeenSoftwareHouse/SpaceEngineers)
 * Because it already had an implemention for a Procedural module for its own asteroid generator, which only needed slight modifications
 * and simplifications for use with the plugin.
 */

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    /// <summary>
    /// Abstract class ProceduralModule which provides basic methods and abstract methods
    /// for Procudedural Generator modules. The Procedural Generator uses these modules.
    /// </summary>
    public abstract class ProceduralModule
    {
        protected int m_seed;
        protected Dictionary<Vector3I, MyProceduralCell> m_cells = new Dictionary<Vector3I, MyProceduralCell>();
        protected MyDynamicAABBTreeD m_cellsTree = new MyDynamicAABBTreeD(Vector3D.Zero);
        protected CachingHashSet<MyProceduralCell> m_toUnloadCells;

        public readonly double CELL_SIZE;

        /// <summary>
        /// Creates a new Procedural module with given seed and cell size.
        /// If the Cellsize is larger than 25000, it wont be used and 25000 will be used instead
        /// </summary>
        /// <param name="seed">Seed of the module</param>
        /// <param name="cellSize">Size of the indevidual cells</param>
        protected ProceduralModule(int seed, double cellSize)
        {
            m_seed = seed;
            CELL_SIZE = Math.Min(cellSize, 25000);
            m_toUnloadCells = new CachingHashSet<MyProceduralCell>();
        }

        /// <summary>
        /// Marks all cells inside of the toUnload bounding sphere to be unloaded, except all Objects inside toExclude.
        /// </summary>
        /// <param name="toUnload">Unload sphere</param>
        /// <param name="toExclude">Exclude sphere</param>
        public void MarkToUnloadCells(BoundingSphereD toUnload, BoundingSphereD? toExclude = null)
        {
            Vector3I cellId = Vector3I.Floor((toUnload.Center - toUnload.Radius) / CELL_SIZE);
            for (var iter = GetCellsIterator(toUnload); iter.IsValid(); iter.GetNext(out cellId))
            {
                MyProceduralCell cell;
                if (m_cells.TryGetValue(cellId, out cell))
                {
                    if (toExclude == null || !toExclude.HasValue || toExclude.Value.Contains(cell.BoundingVolume) == ContainmentType.Disjoint)
                    {
                        m_toUnloadCells.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// Unloads all cells that have been marked to be unloaded. It will remove all objects inside the sphere from
        /// the world. It will not unload cells that are still in the tracking volume of a tracked entity.
        /// </summary>
        /// <param name="trackedEntities">List of tracked entities</param>
        public void UnloadCells(Dictionary<MyEntity, MyEntityTracker> trackedEntities)
        {
            m_toUnloadCells.ApplyAdditions();
            if (m_toUnloadCells.Count == 0) return;

            List<MyObjectSeed> cellObjects = new List<MyObjectSeed>();

            foreach (MyProceduralCell cell in m_toUnloadCells)
            {
                foreach (MyEntityTracker tracker in trackedEntities.Values)
                {
                    BoundingSphereD boundingVolume = tracker.BoundingVolume;
                    if (boundingVolume.Contains(cell.BoundingVolume) != ContainmentType.Disjoint)
                    {
                        m_toUnloadCells.Remove(cell);
                        break;
                    }
                }
            }
            m_toUnloadCells.ApplyRemovals();
            foreach (var cell in m_toUnloadCells)
            {
                cell.GetAll(cellObjects);

                foreach (MyObjectSeed obj in cellObjects)
                {
                    if (obj.Params.Generated)
                    {
                        CloseObject(obj);
                    }
                }
                cellObjects.Clear();
            }
            foreach (MyProceduralCell cell in m_toUnloadCells)
            {
                m_cells.Remove(cell.CellId);
                m_cellsTree.RemoveProxy(cell.proxyId);
            }
            m_toUnloadCells.Clear();
        }

        /// <summary>
        /// Closes the specified object. This should remove it from the world and memory.
        /// </summary>
        /// <param name="seed">The MyObjectSeed of the object that should be removed.</param>
        public abstract void CloseObject(MyObjectSeed seed);

        /// <summary>
        /// Gets all objects that are inside the given Bounding sphere and puts them into the
        /// outObjects list.
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="outObjects"></param>
        public void GetObjectsInSphere(BoundingSphereD sphere, List<MyObjectSeed> outObjects)
        {
            GenerateObjectsData(ref sphere);

            List<MyProceduralCell> cells = new List<MyProceduralCell>();

            m_cellsTree.OverlapAllBoundingSphere(ref sphere, cells);

            foreach (var cell in cells)
            {
                cell.OverlapAllBoundingSphere(ref sphere, outObjects);
            }
            cells.Clear();
        }

        /// <summary>
        /// Generates the given objects if they don't exist and puts them into
        /// the existingObjectSeeds list.
        /// </summary>
        /// <param name="objects">Objects to generate</param>
        /// <param name="existingObjectSeeds">Existing objects</param>
        public abstract void GenerateObjects(List<MyObjectSeed> objects, HashSet<MyObjectSeedParams> existingObjectSeeds);

        /// <summary>
        /// Updates all Objects.
        /// </summary>
        public abstract void UpdateObjects();

        /// <summary>
        /// Generates all Cells data that are within sphere, if they have not been generated.
        /// </summary>
        /// <param name="sphere">Bounding Sphere of the cells to generate</param>
        protected void GenerateObjectsData(ref BoundingSphereD sphere)
        {
            BoundingBoxD box = BoundingBoxD.CreateFromSphere(sphere);

            Vector3I cellId = Vector3I.Floor(box.Min / CELL_SIZE);

            for(var it = GetCellsIterator(sphere); it.IsValid(); it.GetNext(out cellId))
            {
                if (m_cells.ContainsKey(cellId)) continue;

                BoundingBoxD cellBounds = new BoundingBoxD(cellId * CELL_SIZE, (cellId + 1) * CELL_SIZE);
                if (!(sphere.Contains(cellBounds) != ContainmentType.Disjoint)) continue;

                MyProceduralCell cell = GenerateCell(ref cellId);
                if(cell != null)
                {
                    m_cells.Add(cellId, cell);

                    BoundingBoxD aabb = cell.BoundingVolume;
                    cell.proxyId = m_cellsTree.AddProxy(ref aabb, cell, 0u);
                }
            }
        }

        /// <summary>
        /// Generates a single cell with its object seeds, at the given Cell coordinate
        /// </summary>
        /// <param name="id">The cell coordinate which is its id</param>
        /// <returns>The generated procedural cell</returns>
        public abstract MyProceduralCell GenerateCell(ref Vector3I id);

        protected Vector3I_RangeIterator GetCellsIterator(BoundingSphereD sphere)
        {
            return GetCellsIterator(BoundingBoxD.CreateFromSphere(sphere));
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
        /// Gets the seed for the given cell
        /// </summary>
        /// <param name="cellId">The cell id for the cell to get the seed for</param>
        /// <returns>The cell seed</returns>
        protected int GetCellSeed(ref Vector3I cellId)
        {
            unchecked
            {
                return m_seed + cellId.X * 16785407 + cellId.Y * 39916801 + cellId.Z * 479001599;
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
            hash = (hash * 397) ^ m_seed;
            hash = (hash * 397) ^ obj.Params.Index;
            hash = (hash * 397) ^ obj.Params.Seed;
            return hash;
        }
    }
}
