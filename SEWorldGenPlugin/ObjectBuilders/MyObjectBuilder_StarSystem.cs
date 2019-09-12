using ProtoBuf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;
using VRageMath;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [ProtoContract]
    public class MyObjectBuilder_StarSystem : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(3010)]
        public HashSet<MySystemItem> SystemObjects = new HashSet<MySystemItem>();
    }

    [ProtoContract]
    [ProtoInclude(1501, typeof(MyPlanetItem))]
    [ProtoInclude(1502, typeof(MySystemBeltItem))]
    [ProtoInclude(1503, typeof(MyPlanetMoonItem))]
    [ProtoInclude(1504, typeof(MyPlanetRingItem))]
    [XmlInclude(typeof(MyPlanetItem))]
    [XmlInclude(typeof(MySystemBeltItem))]
    [XmlInclude(typeof(MyPlanetMoonItem))]
    [XmlInclude(typeof(MyPlanetRingItem))]
    public class MySystemItem
    {
        [ProtoMember(1)]
        public SystemObjectType Type;
    }

    public enum SystemObjectType
    {
        PLANET,
        RING,
        MOON,
        BELT
    }

    [ProtoContract]
    public class MyPlanetItem : MySystemItem
    {
        [ProtoMember(2)]
        public string DefName;

        [ProtoMember(3)]
        public int Size;

        [ProtoMember(4)]
        public SerializableVector3D CenterPosition;

        [ProtoMember(5)]
        public SerializableVector3D OffsetPosition;

        [ProtoMember(6)]
        public bool Generated;

        [ProtoMember(7)]
        public MyPlanetRingItem PlanetRing;

        [ProtoMember(8)]
        [DefaultValue(null)]
        [XmlArrayItem("MyPlanetMoon")]
        public MyPlanetMoonItem[] PlanetMoons;

        public override string ToString()
        {
            return DefName + "\n Size = " + Size + "\n Generated = " + Generated;
        }
    }

    [ProtoContract]
    public class MyPlanetMoonItem : MySystemItem
    {
        [ProtoMember(2)]
        public int Size;

        [ProtoMember(3)]
        public float Distance;

        [ProtoMember(4)]
        public string DefName;
    }

    [ProtoContract]
    public class MyPlanetRingItem : MySystemItem
    {
        [ProtoMember(2)]
        public long Radius;

        [ProtoMember(3)]
        public int Width;

        [ProtoMember(4)]
        public int Height;

        [ProtoMember(5)]
        public float AngleDegrees;

        [ProtoMember(6)]
        public int RoidSize;

        [ProtoMember(7)]
        public SerializableVector3D Center;
    }

    [ProtoContract]
    public class MySystemBeltItem : MySystemItem
    {
        [ProtoMember(2)]
        public long Radius;

        [ProtoMember(3)]
        public int Width;

        [ProtoMember(4)]
        public int Height;

        [ProtoMember(5)]
        public int RoidSize;
    }
}
