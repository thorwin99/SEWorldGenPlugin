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
    /// <summary>
    /// Serializable ObjectBuilder used to save the solar systems data and contains all system objects.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class LegacyMyObjectBuilder_StarSystem
    {
        [ProtoMember(3010)]
        public HashSet<MySystemItem> SystemObjects = new HashSet<MySystemItem>();
    }

    /// <summary>
    /// Baseclass for all system objects containing a type and display name
    /// </summary>
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

    /// <summary>
    /// Enum for the body type of an object in the solar system.
    /// </summary>
    public enum SystemObjectType
    {
        PLANET,
        RING,
        MOON,
        BELT
    }

    /// <summary>
    /// Class representing a planets data in the solar system
    /// </summary>
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

    /// <summary>
    /// Class representing a moons data in the solar system
    /// </summary>
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

    /// <summary>
    /// Class representing a rings data in the solar system
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyPlanetRingItem : MySystemItem
    {
        public MyPlanetRingItem()
        {
            RoidSizeMax = 512;
        }

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
        public float AngleDegreesX;

        [ProtoMember(9)]
        public int RoidSize;

        [ProtoMember(10)]
        public SerializableVector3D Center;

        [ProtoMember(11)]
        public int RoidSizeMax;
    }

    /// <summary>
    /// Class representing a belts data in the solar system
    /// </summary>
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
