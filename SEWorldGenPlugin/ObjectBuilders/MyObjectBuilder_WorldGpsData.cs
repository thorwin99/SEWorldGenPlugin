using ProtoBuf;
using System;
using System.Collections.Generic;
using VRage;
using VRageMath;

namespace SEWorldGenPlugin.ObjectBuilders
{
    /// <summary>
    /// A serializable class, that contains the data for the plugins gpss.
    /// </summary>
    [ProtoContract]
    public class MyObjectBuilder_WorldGpsData
    {
        [ProtoMember(1)]
        public HashSet<PersistentGpsData> PersistentGpss = new HashSet<PersistentGpsData>();

        [ProtoMember(2)]
        public HashSet<DynamicGpsData> DynamicGpss = new HashSet<DynamicGpsData>();
    }

    [ProtoContract]
    public class PersistentGpsData
    {
        /// <summary>
        /// The name of this gps
        /// </summary>
        [ProtoMember(1)]
        public string Name = "";

        /// <summary>
        /// The color of this gps
        /// </summary>
        [ProtoMember(2)]
        public SerializableVector3 Color = Vector3.Zero;

        /// <summary>
        /// The position of this gps
        /// </summary>
        [ProtoMember(3)]
        public SerializableVector3D Position = Vector3D.Zero;

        /// <summary>
        /// The Id of the gps object
        /// </summary>
        [ProtoMember(4)]
        public Guid Id = Guid.Empty;

        /// <summary>
        /// If the gps is hidden by default
        /// </summary>
        [ProtoMember(5)]
        public bool Hidden = false;

        /// <summary>
        /// A set of all players, that already know of this gps.
        /// </summary>
        [ProtoMember(6)]
        public HashSet<long> PlayerIds = new HashSet<long>();
    }

    [ProtoContract]
    public class DynamicGpsData
    {
        /// <summary>
        /// Player id which currently holds dynamic gpss
        /// </summary>
        [ProtoMember(1)]
        public long PlayerId;

        /// <summary>
        /// List of identifiers of dynamic gpss
        /// </summary>
        [ProtoMember(2)]
        public HashSet<DynamicGpsId> DynamicGpss = new HashSet<DynamicGpsId>();
    }

    [ProtoContract]
    public class DynamicGpsId
    {
        /// <summary>
        /// Id of gps object
        /// </summary>
        [ProtoMember(1)]
        public Guid ID = Guid.Empty;

        /// <summary>
        /// Hash of gps
        /// </summary>
        [ProtoMember(2)]
        public int Hash;

        public DynamicGpsId()
        {

        }

        public DynamicGpsId(Guid iD, int hash)
        {
            ID = iD;
            Hash = hash;
        }
    }
}
