using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public class ProceduralCell
    {
        public Vector3I CellId
        {
            get;
            private set;
        }

        public BoundingBoxD BoundingVolume
        {
            get;
            private set;
        }

        public int proxyId = -1;
        private MyDynamicAABBTreeD m_objectTree = new MyDynamicAABBTreeD(Vector3D.Zero);

        public void AddObject(CellObject obj)
        {
            var bounds = obj.BoundingVolume;
            obj.m_proxyId = m_objectTree.AddProxy(ref bounds, obj, 0);
        }

        public ProceduralCell(Vector3I cellId, double cellSize)
        {
            CellId = cellId;
            BoundingVolume = new BoundingBoxD(CellId * cellSize, (CellId + 1) * cellSize);
        }

        public void OverlapAllBoundingSphere(ref BoundingSphereD sphere, List<CellObject> list, bool clear = false)
        {
            m_objectTree.OverlapAllBoundingSphere(ref sphere, list, clear);
        }

        public void OverlapAllBoundingBox(ref BoundingBoxD box, List<CellObject> list, bool clear = false)
        {
            m_objectTree.OverlapAllBoundingBox(ref box, list, 0, clear);
        }

        public void GetAllObjects(List<CellObject> list, bool clear = true)
        {
            m_objectTree.GetAll(list, clear);
        }

        public override int GetHashCode()
        {
            return CellId.GetHashCode();
        }

        public override string ToString()
        {
            return CellId.ToString();
        }
    }

    public class CellObject
    {
        public MyObjectSeedParams Params = new MyObjectSeedParams();

        public int m_proxyId = -1;

        public BoundingBoxD BoundingVolume
        {
            get;
            private set;
        }

        public float Size
        {
            get;
            private set;
        }

        public ProceduralCell Cell
        {
            get;
            private set;
        }

        public Vector3I CellId
        {
            get { return Cell.CellId;  }
        }

        public object UserData
        {
            get;
            set;
        }

        public CellObject()
        {

        }

        public CellObject(ProceduralCell cell, Vector3D position, double size)
        {
            Cell = cell;
            Size = (float)size;
            BoundingVolume = new BoundingBoxD(position - Size, position + Size);
        }
    }
}
