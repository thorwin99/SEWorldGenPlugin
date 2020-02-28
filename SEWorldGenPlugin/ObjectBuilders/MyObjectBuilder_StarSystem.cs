using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using VRage;
using VRage.Serialization;
using VRageMath;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [ProtoContract]
    [Serializable]
    public class MyObjectBuilder_StarSystem
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
    [Serializable]
    public class MySystemItem
    {
        [ProtoMember(1)]
        public SystemObjectType Type;

        [ProtoMember(2)]
        public string DisplayName;
    }

    public enum SystemObjectType
    {
        PLANET,
        RING,
        MOON,
        BELT
    }

    [ProtoContract]
    [Serializable]
    public class MyPlanetItem : MySystemItem
    {
        [ProtoMember(3)]
        public string DefName;

        [ProtoMember(4)]
        public float Size;

        [ProtoMember(5)]
        public SerializableVector3D CenterPosition;

        [ProtoMember(6)]
        public SerializableVector3D OffsetPosition;

        [ProtoMember(7)]
        public bool Generated;

        [ProtoMember(8)]
        [Serialize(MyObjectFlags.Nullable)]
        public MyPlanetRingItem PlanetRing;

        [ProtoMember(9)]
        [DefaultValue(null)]
        [XmlArrayItem("MyPlanetMoon")]
        [Serialize(MyObjectFlags.Nullable)]
        public MyPlanetMoonItem[] PlanetMoons;

        public MyPlanetItem()
        {
            Type = SystemObjectType.PLANET;
            DisplayName = "Planet";
            DefName = "";
            Size = 0;
            CenterPosition = Vector3D.Zero;
            OffsetPosition = Vector3D.Zero;
            Generated = false;
            PlanetMoons = new MyPlanetMoonItem[0];
        }
    }

    [ProtoContract]
    [Serializable]
    public class MyPlanetMoonItem : MySystemItem
    {
        [ProtoMember(3)]
        public float Size;

        [ProtoMember(4)]
        public float Distance;

        [ProtoMember(5)]
        public string DefName;
    }

    [ProtoContract]
    [Serializable]
    public class MyPlanetRingItem : MySystemItem
    {
        [ProtoMember(3)]
        public double Radius;

        [ProtoMember(4)]
        public int Width;

        [ProtoMember(5)]
        public int Height;

        [ProtoMember(6)]
        public float AngleDegrees;

        [ProtoMember(7)]
        public float AngleDegreesY;

        [ProtoMember(8)]
        public float AngleDegreesZ;

        [ProtoMember(9)]
        public int RoidSize;

        [ProtoMember(10)]
        public SerializableVector3D Center;
    }

    [ProtoContract]
    [Serializable]
    public class MySystemBeltItem : MySystemItem
    {
        [ProtoMember(3)]
        public double Radius;

        [ProtoMember(4)]
        public int Width;

        [ProtoMember(5)]
        public int Height;

        [ProtoMember(6)]
        public int RoidSize;

        public MySystemBeltItem()
        {
            Type = SystemObjectType.BELT;
            DisplayName = "";
            Radius = 0;
            Width = 0;
            Height = 0;
            RoidSize = 0;
        }
    }
}
