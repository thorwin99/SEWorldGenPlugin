using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SEWorldGenPlugin.ObjectBuilders
{
    /// <summary>
    /// Serializable ObjectBuilder used to save the solar systems data and contains all system objects.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyObjectBuilder_SystemData
    {
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
        [ProtoMember(1)]
        public MySystemObjectType Type;

        [ProtoMember(2)]
        public string DisplayName;
    }

    /// <summary>
    /// Class representing a planets data in the solar system
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemPlanet : MySystemObject
    {
        public MySystemPlanet()
        {
            Type = MySystemObjectType.PLANET;
        }
    }

    /// <summary>
    /// Class representing a moon of a planet in the solar system
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemPlanetMoon : MySystemObject
    {
        public MySystemPlanetMoon()
        {
            Type = MySystemObjectType.MOON;
        }
    }

    /// <summary>
    /// Class representing a ring around a planet
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemPlanetRing : MySystemObject
    {
        public MySystemPlanetRing()
        {
            Type = MySystemObjectType.RING;
        }
    }

    /// <summary>
    /// Class representing a belt in the system
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemBelt : MySystemObject
    {
        public MySystemBelt()
        {
            Type = MySystemObjectType.BELT;
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
