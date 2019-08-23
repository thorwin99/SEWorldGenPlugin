using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Library.Utils;
using VRage.Noise;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralWorld
{
    internal class MyInfiniteDensityFunction : IMyAsteroidFieldDensityFunction, IMyModule
    {
        private IMyModule noise;

        public MyInfiniteDensityFunction(MyRandom random, double frequency)
        {
            noise = new MySimplexFast(random.Next(), frequency);
        }

        public bool ExistsInCell(ref BoundingBoxD bbox)
        {
            return true;
        }

        public double GetValue(double x)
        {
            return noise.GetValue(x);
        }

        public double GetValue(double x, double y)
        {
            return noise.GetValue(x, y);
        }

        public double GetValue(double x, double y, double z)
        {
            return noise.GetValue(x, y, z);
        }
    }
}
