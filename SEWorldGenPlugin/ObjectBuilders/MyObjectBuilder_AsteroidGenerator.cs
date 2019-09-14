using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.ObjectBuilders;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [ProtoContract]
    public class MyObjectBuilder_AsteroidGenerator
    {
        [ProtoMember(3005)]
        public HashSet<MyObjectSeedParams> ExistingObjectsSeeds;
    }
}
