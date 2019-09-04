using Sandbox.Game.Entities;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.SaveItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Library.Utils;
using VRage.Noise;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public class ProceduralAsteroidsRingModule : ProceduralModule
    {
        private const int OBJECT_SIZE_MIN = 64;
        private const int OBJECT_SIZE_MAX = 512;
        private const int SUBCELL_SIZE = 4 * 1024 + OBJECT_SIZE_MAX * 2;
        private const int SUBCELLS = 3;

        List<PlanetRingItem> m_asteroidRings;

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
            throw new NotImplementedException();
        }

        public override void UnloadCellObjects(BoundingSphereD toUnload, BoundingSphereD toExclude)
        {
            throw new NotImplementedException();
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
    }
}
