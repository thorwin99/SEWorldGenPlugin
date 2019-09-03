using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public class ProceduralAsteroidsRingModule : ProceduralModule
    {
        public ProceduralAsteroidsRingModule() : base(0, 0)
        {

        }

        public override ProceduralCell GenerateCell(ref Vector3I id)
        {
            throw new NotImplementedException();
        }

        public override void GenerateObjects(List<CellObject> objects, HashSet<MyObjectSeedParams> existingObjectSeeds)
        {
            throw new NotImplementedException();
        }

        public override void UnloadCellObjects(BoundingSphereD toUnload, BoundingSphereD toExclude)
        {
            throw new NotImplementedException();
        }
    }
}
