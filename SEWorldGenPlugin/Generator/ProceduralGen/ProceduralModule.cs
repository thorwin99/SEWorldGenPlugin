using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public abstract class ProceduralModule
    {

        protected int m_seed;
        protected Dictionary<Vector3I, string> m_cells = new Dictionary<Vector3I, string>();

        public readonly double CELL_SIZE;

        protected ProceduralModule(int seed, double cellSize)
        {
            m_seed = seed;
            CELL_SIZE = Math.Max(cellSize, 25000);
        }

    }
}
