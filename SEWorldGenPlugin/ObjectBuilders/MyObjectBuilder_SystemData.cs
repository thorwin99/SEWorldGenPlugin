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
    [XmlInclude(typeof(MySystemPlanet))]
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
