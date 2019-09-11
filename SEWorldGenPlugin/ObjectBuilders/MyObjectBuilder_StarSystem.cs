using ProtoBuf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [ProtoContract]
    public class MyObjectBuilder_StarSystem : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(3010)]
        public HashSet<MyPlanetItem> SystemPlanets = new HashSet<MyPlanetItem>();
    }

    [ProtoContract]
    public class MyPlanetItem
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
        public MyPlanetRingItem PlanetRing;

        [ProtoMember(7)]
        [DefaultValue(null)]
        [XmlArrayItem("MyPlanetMoon")]
        public MyPlanetMoonItem[] PlanetMoons;

        public override string ToString()
        {
            return DefName + "\n Size = " + Size + "\n Generated = " + Generated;
        }
    }

    [ProtoContract]
    public class MyPlanetMoonItem
    {
        [ProtoMember(1)]
        public int Size;

        [ProtoMember(2)]
        public float Distance;

        [ProtoMember(3)]
        public string DefName;
    }

    [ProtoContract]
    public class MyPlanetRingItem
    {
        [ProtoMember(1)]
        public int Radius;

        [ProtoMember(2)]
        public int Width;

        [ProtoMember(3)]
        public int Height;

        [ProtoMember(4)]
        public float AngleDegrees;

        [ProtoMember(5)]
        public int RoidSize;

        [ProtoMember(6)]
        public SerializableVector3D Center;
    }
}
