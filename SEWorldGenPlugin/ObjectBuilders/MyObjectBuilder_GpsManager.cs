using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRageMath;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [ProtoContract]
    public class MyObjectBuilder_GpsManager
    {
        [ProtoMember(1)]
        public HashSet<GpsData> Gpss;

        public MyObjectBuilder_GpsManager()
        {
            Gpss = new HashSet<GpsData>();
        }
    }

    public class GpsData
    {
        [ProtoMember(1)]
        public string name;

        [ProtoMember(2)]
        public SerializableVector3 color;

        [ProtoMember(3)]
        public SerializableVector3D position;

        public GpsData()
        {
            name = "Gps";
            color = Vector3.Zero;
            position = Vector3D.Zero;
        }
    }
}
