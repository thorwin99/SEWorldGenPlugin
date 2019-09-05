using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public abstract class ProceduralModule
    {

        protected int m_seed;
        protected Dictionary<Vector3I, ProceduralCell> m_cells = new Dictionary<Vector3I, ProceduralCell>();
        protected MyDynamicAABBTreeD m_cellsTree = new MyDynamicAABBTreeD(Vector3D.Zero);
        protected List<ProceduralCell> m_toUnloadCells;

        public readonly double CELL_SIZE;

        protected ProceduralModule(int seed, double cellSize)
        {
            m_seed = seed;
            CELL_SIZE = Math.Max(cellSize, 25000);
            m_toUnloadCells = new List<ProceduralCell>();
        }

        public void MarkToUnloadCells(BoundingSphereD toUnload, BoundingSphereD toExclude)
        {
            Vector3I cellId = Vector3I.Floor((toUnload.Center - toUnload.Radius) / CELL_SIZE);
            for (var iter = GetCellsIterator(toUnload); iter.IsValid(); iter.GetNext(out cellId))
            {
                ProceduralCell cell;
                if (m_cells.TryGetValue(cellId, out cell))
                {
                    if (toExclude == null || toExclude.Contains(cell.BoundingVolume) == ContainmentType.Disjoint)
                    {
                        m_toUnloadCells.Add(cell);
                    }
                }
            }
        }

        public abstract void UnloadCells();

        public void GetObjectsInSphere(BoundingSphereD sphere, List<CellObject> outObjects)
        {
            GenerateObjectsData(ref sphere);
            GetAllObjectsInSphere(ref sphere, outObjects);
        }

        public abstract void GenerateObjects(List<CellObject> objects, HashSet<MyObjectSeedParams> existingObjectSeeds);

        protected void GenerateObjectsData(ref BoundingSphereD sphere)
        {
            BoundingBoxD box = BoundingBoxD.CreateFromSphere(sphere);

            Vector3I cellId = Vector3I.Floor(box.Min / CELL_SIZE);

            for(var it = GetCellsIterator(sphere); it.IsValid(); it.GetNext(out cellId))
            {
                if (m_cells.ContainsKey(cellId)) continue;

                BoundingBoxD cellBounds = new BoundingBoxD(cellId * CELL_SIZE, (cellId + 1) * CELL_SIZE);
                if (sphere.Contains(cellBounds) == ContainmentType.Disjoint) continue;

                ProceduralCell cell = GenerateCell(ref cellId);
                if(cell != null)
                {
                    m_cells.Add(cellId, cell);
                    cell.proxyId = m_cellsTree.AddProxy(ref cellBounds, cell, 0);
                }
            }
        }

        public abstract ProceduralCell GenerateCell(ref Vector3I id);

        protected void GetAllObjectsInSphere(ref BoundingSphereD sphere, List<CellObject> outList)
        {
            List<ProceduralCell> cells = new List<ProceduralCell>();

            m_cellsTree.OverlapAllBoundingSphere(ref sphere, cells);

            foreach(var cell in cells)
            {
                cell.OverlapAllBoundingSphere(ref sphere, outList);
            }
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

        protected int GetObjectIdSeed(CellObject obj)
        {
            int hash = obj.CellId.GetHashCode();
            hash = (hash * 397) ^ m_seed;
            hash = (hash * 397) ^ obj.Params.Index;
            hash = (hash * 397) ^ obj.Params.Seed;
            return hash;
        }

    }
}
