using ProtoBuf;
using System.Collections.Generic;
using VRage;
using VRageMath;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [ProtoContract]
    public class MyObjectBuilder_GpsManager
    {
        [ProtoMember(1)]
        public HashSet<GpsData> Gpss;

        [ProtoMember(2)]
        public HashSet<DynamicGpsData> DynamicGpss;

        public MyObjectBuilder_GpsManager()
        {
            Gpss = new HashSet<GpsData>();
            DynamicGpss = new HashSet<DynamicGpsData>();
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

    public class DynamicGpsData
    {
        [ProtoMember(1)]
        public long playerId;

        [ProtoMember(2)]
        public int gpsHash;

        [ProtoMember(3)]
        public string gpsName;

        public DynamicGpsData()
        {
            playerId = 0;
            gpsHash = 0;
            gpsName = "";
        }
    }
}
