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
    public class MyObjectBuilder_SystemData
    {
        /// <summary>
        /// Objects that are located in this star system.
        /// </summary>
        [ProtoMember(1)]
        public HashSet<MySystemObject> Objects = new HashSet<MySystemObject>();
    }

    /// <summary>
    /// Baseclass for all system objects containing a type and display name
    /// </summary>
    [ProtoContract]
    [ProtoInclude(5001, typeof(MySystemPlanet))]
    [ProtoInclude(5002, typeof(MySystemPlanetMoon))]
    [ProtoInclude(5003, typeof(MySystemPlanetRing))]
    [ProtoInclude(5004, typeof(MySystemBelt))]
    [XmlInclude(typeof(MySystemPlanet))]
    [XmlInclude(typeof(MySystemPlanetMoon))]
    [XmlInclude(typeof(MySystemPlanetRing))]
    [XmlInclude(typeof(MySystemBelt))]
    [Serializable]
    public class MySystemObject
    {
        /// <summary>
        /// Type of the Object. Can be Moon, Planet, Ring or Belt
        /// </summary>
        [ProtoMember(1)]
        public MySystemObjectType Type;

        /// <summary>
        /// Display Name of the object, used for menus or gps
        /// </summary>
        [ProtoMember(2)]
        public string DisplayName;

        /// <summary>
        /// The position of the objects center
        /// </summary>
        [ProtoMember(3)]
        public SerializableVector3D CenterPosition;
    }

    /// <summary>
    /// Class representing a planets data in the solar system
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemPlanet : MySystemObject
    {
        /// <summary>
        /// The subtype id of the planet. This is the id defined for the planet in its definition file.
        /// </summary>
        [ProtoMember(4)]
        public string SubtypeId;

        /// <summary>
        /// The diameter of the planet in meters.
        /// </summary>
        [ProtoMember(5)]
        public double Diameter;

        /// <summary>
        /// If the planet is already generated, or still needs to be spawned.
        /// </summary>
        [ProtoMember(6)]
        public bool Generated;

        /// <summary>
        /// The moons of this planet. null, if no moons are present.
        /// </summary>
        [ProtoMember(7)]
        [DefaultValue(null)]
        [XmlArrayItem("MySystemPlanetMoon")]
        [Serialize(MyObjectFlags.Nullable)]
        public MySystemPlanetMoon[] Moons;

        /// <summary>
        /// The ring around the planet. null, if not present.
        /// </summary>
        [ProtoMember(8)]
        [Serialize(MyObjectFlags.Nullable)]
        public MySystemPlanetRing Ring;

        public MySystemPlanet()
        {
            Type = MySystemObjectType.PLANET;
            DisplayName = "";
            CenterPosition = Vector3D.Zero;
            SubtypeId = "";
            Diameter = 0;
            Generated = false;
        }
    }

    /// <summary>
    /// Class representing a moon of a planet in the solar system. It will always be generated,
    /// if the planet it orbits around is generated.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemPlanetMoon : MySystemObject
    {
        /// <summary>
        /// The subtype id of the moon. This is the id defined for the planet in its definition file.
        /// </summary>
        [ProtoMember(4)]
        public string SubtypeId;

        /// <summary>
        /// The diameter of the moon in meters.
        /// </summary>
        [ProtoMember(5)]
        public double Diameter;

        public MySystemPlanetMoon()
        {
            Type = MySystemObjectType.MOON;
            DisplayName = "";
            CenterPosition = Vector3D.Zero;
            SubtypeId = "";
            Diameter = 0;
        }
    }

    /// <summary>
    /// Class representing a ring around a planet
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemPlanetRing : MySystemObject
    {
        /// <summary>
        /// The radius of the asteroid ring to the center.
        /// This is the distance of the area, where asteroids spawn
        /// to the center. In meters
        /// </summary>
        [ProtoMember(4)]
        public double Radius;

        /// <summary>
        /// The width of the asteroid ring. This is the width of the area where
        /// asteroids will spawn. In meters
        /// </summary>
        [ProtoMember(5)]
        public double Width;

        /// <summary>
        /// The height of the area, where asteroids will spawn. In meters
        /// </summary>
        [ProtoMember(6)]
        public double Height;

        /// <summary>
        /// The angle of the asteroid ring around the planet. Needs to have 3 components.
        /// X is the angle around the x axis, y around the y axis and z around the z axis.
        /// The angles should be in degrees (0 to 360)
        /// </summary>
        [ProtoMember(7)]
        public SerializableVector3D AngleDegrees;

        /// <summary>
        /// The minimum and maximum size of the asteroids in this ring in meters.
        /// </summary>
        [ProtoMember(8)]
        public MySerializableMinMax AsteroidSize;

        public MySystemPlanetRing()
        {
            Type = MySystemObjectType.RING;
            DisplayName = "";
            CenterPosition = Vector3D.Zero;
            Radius = 0;
            Width = 0;
            Height = 0;
            AngleDegrees = Vector3D.Zero;
            AsteroidSize = new MySerializableMinMax(0, 0);
        }
    }

    /// <summary>
    /// Class representing a belt in the system
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemBelt : MySystemObject
    {
        /// <summary>
        /// The radius of the asteroid belt to the center.
        /// This is the distance of the area, where asteroids spawn
        /// to the center.
        /// </summary>
        [ProtoMember(4)]
        public double Radius;

        /// <summary>
        /// The width of the asteroid belt. This is the width of the area where
        /// asteroids will spawn. In meters
        /// </summary>
        [ProtoMember(5)]
        public double Width;

        /// <summary>
        /// The height of the area, where asteroids will spawn.
        /// In meters
        /// </summary>
        [ProtoMember(6)]
        public double Height;

        /// <summary>
        /// The minimum and maximum size of the asteroids in this belt in meters.
        /// </summary>
        [ProtoMember(7)]
        public MySerializableMinMax AsteroidSize;

        public MySystemBelt()
        {
            Type = MySystemObjectType.BELT;
            DisplayName = "";
            CenterPosition = Vector3D.Zero;
            Radius = 0;
            Width = 0;
            Height = 0;
            AsteroidSize = new MySerializableMinMax(0, 0);
        }

    }

    /// <summary>
    /// Enum for the body type of an object in the solar system.
    /// </summary>
    public enum MySystemObjectType
    {
        PLANET,
        RING,
        MOON,
        BELT
    }
}
