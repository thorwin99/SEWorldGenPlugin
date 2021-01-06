using ProtoBuf;
using System.Collections.Generic;
using VRage;
using VRageMath;

namespace SEWorldGenPlugin.ObjectBuilders
{
    /// <summary>
    /// The serializable ObjectBuilder for the GpsManagers savedata.
    /// Contains saved gpss and dynamic gpss.
    /// </summary>
    [ProtoContract]
    public class LegacyMyObjectBuilder_GpsManager
    {
        [ProtoMember(1)]
        public HashSet<GpsData> Gpss;

        [ProtoMember(2)]
        public HashSet<DynamicGpsData> DynamicGpss;

        public LegacyMyObjectBuilder_GpsManager()
        {
            Gpss = new HashSet<GpsData>();
            DynamicGpss = new HashSet<DynamicGpsData>();
        }
    }

    /// <summary>
    /// Serializable class which contains gps data such as the name, color and position of the gps.
    /// </summary>
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

    /// <summary>
    /// Serializable class for dynamic gpss. It contains the player id of the player that gets shown this
    /// gps, the hash of the gps and the name of the gps.
    /// </summary>
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
