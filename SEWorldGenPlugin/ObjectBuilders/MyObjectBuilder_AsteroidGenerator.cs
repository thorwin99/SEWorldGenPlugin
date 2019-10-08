using ProtoBuf;
using System.Collections.Generic;
using VRage.Game;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [ProtoContract]
    public class MyObjectBuilder_AsteroidGenerator
    {
        [ProtoMember(3005)]
        public HashSet<MyObjectSeedParams> ExistingObjectsSeeds;

        [ProtoMember(3006)]
        public float ProceduralDensity;
    }
}
