using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;


/*
 * Code is primarily taken from the Space Engineers GitHub repository. 
 */
namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public abstract class ProceduralModule
    {
        protected int m_seed;
        protected Dictionary<Vector3I, MyProceduralCell> m_cells = new Dictionary<Vector3I, MyProceduralCell>();
        protected MyDynamicAABBTreeD m_cellsTree = new MyDynamicAABBTreeD(Vector3D.Zero);
        protected CachingHashSet<MyProceduralCell> m_toUnloadCells;

        public readonly double CELL_SIZE;

        protected ProceduralModule(int seed, double cellSize)
        {
            m_seed = seed;
            CELL_SIZE = Math.Min(cellSize, 25000);
            m_toUnloadCells = new CachingHashSet<MyProceduralCell>();
        }

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

        public abstract void CloseObject(MyObjectSeed seed);

        public void GetObjectsInSphere(BoundingSphereD sphere, List<MyObjectSeed> outObjects)
        {
            GenerateObjectsData(ref sphere);
            GetAllObjectsInSphere(ref sphere, outObjects);
        }

        public abstract void GenerateObjects(List<MyObjectSeed> objects, HashSet<MyObjectSeedParams> existingObjectSeeds);
        public abstract void UpdateObjects();

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

        public abstract MyProceduralCell GenerateCell(ref Vector3I id);

        protected void GetAllObjectsInSphere(ref BoundingSphereD sphere, List<MyObjectSeed> outList)
        {
            List<MyProceduralCell> cells = new List<MyProceduralCell>();

            m_cellsTree.OverlapAllBoundingSphere(ref sphere, cells);

            foreach(var cell in cells)
            {
                cell.OverlapAllBoundingSphere(ref sphere, outList);
            }
            cells.Clear();
        }

        protected Vector3I_RangeIterator GetCellsIterator(BoundingSphereD sphere)
        {
            return GetCellsIterator(BoundingBoxD.CreateFromSphere(sphere));
        }

        protected Vector3I_RangeIterator GetCellsIterator(BoundingBoxD bbox)
        {
            Vector3I min = Vector3I.Floor(bbox.Min / CELL_SIZE);
            Vector3I max = Vector3I.Floor(bbox.Max / CELL_SIZE);

            return new Vector3I_RangeIterator(ref min, ref max);
        }

        protected int GetCellSeed(ref Vector3I cellId)
        {
            unchecked
            {
                return m_seed + cellId.X * 16785407 + cellId.Y * 39916801 + cellId.Z * 479001599;
            }
        }

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
