using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Code is taken from the Space Engineers code base, since this class is not viewable for mods but is needed to generate Asteroids/Planets
 */
namespace SEWorldGenPlugin.Generator.Asteroids
{
    internal delegate IMyCompositionInfoProvider MyCompositeShapeGeneratorDelegate(int generatorSeed, int seed, float size);
}
