using ProtoBuf;
using System.ComponentModel;
using System.Xml.Serialization;
using VRage;

namespace SEWorldGenPlugin.SaveItems
{
    [ProtoContract]
    public class PlanetItem
    {
        [ProtoMember(1)]
        public string DefName;

        [ProtoMember(2)]
        public int Size;

        [ProtoMember(3)]
        public SerializableVector3D CenterPosition;

        [ProtoMember(4)]
        public SerializableVector3D OffsetPosition;

        [ProtoMember(5)]
        public bool Generated;

        [ProtoMember(6)]
        public PlanetRingItem PlanetRing;

        [ProtoMember(7)]
        [DefaultValue(null)]
        [XmlArrayItem("MyPlanetMoon")]
        public PlanetMoonItem[] PlanetMoons;

    }

    [ProtoContract]
    public class PlanetRingItem
    {
        [ProtoMember(1)]
        public double AsteroidCount;

        [ProtoMember(2)]
        public int Radius;

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

        [ProtoMember(8)]
        public double Density;
    }

    [ProtoContract]
    public class PlanetMoonItem
    {
        [ProtoMember(1)]
        public int Size;

        [ProtoMember(2)]
        public float Distance;

        [ProtoMember(3)]
        public string DefName;
    }
}
