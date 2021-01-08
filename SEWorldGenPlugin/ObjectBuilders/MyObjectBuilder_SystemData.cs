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
        public MySystemObject CenterObject;

        public int Count()
        {
            return 1 + CenterObject.ChildCount();
        }

        /// <summary>
        /// Finds a system object by name
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <returns>The object or null if not present</returns>
        public MySystemObject FindObjectByName(string name)
        {
            if (name == null) return null;
            foreach(var child in CenterObject.GetAllChildren())
            {
                if (child.DisplayName.ToLower() == name.ToLower()) return child;
            }

            return null;
        }

        /// <summary>
        /// Checks if an object with given name exists
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <returns>true if it exists</returns>
        public bool ObjectExists(string name)
        {
            return FindObjectByName(name) != null;
        }
    }

    /// <summary>
    /// Baseclass for all system objects containing a type and display name
    /// </summary>
    [ProtoContract]
    [ProtoInclude(5001, typeof(MySystemPlanet))]
    [ProtoInclude(5002, typeof(MySystemPlanetMoon))]
    [ProtoInclude(5003, typeof(MySystemRing))]
    [XmlInclude(typeof(MySystemPlanet))]
    [XmlInclude(typeof(MySystemPlanetMoon))]
    [XmlInclude(typeof(MySystemRing))]
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

        /// <summary>
        /// All Child objects, such as moons.
        /// </summary>
        [ProtoMember(4)]
        [Serialize(MyObjectFlags.Nullable)]
        public HashSet<MySystemObject> ChildObjects;

        /// <summary>
        /// Initializes a new empty system object
        /// </summary>
        public MySystemObject()
        {
            Type = MySystemObjectType.EMPTY;
            DisplayName = "";
            CenterPosition = Vector3D.Zero;
            ChildObjects = null;
        }

        /// <summary>
        /// Returns the amount of objects in this tree
        /// </summary>
        /// <returns></returns>
        public int ChildCount()
        {
            int count = 0;

            if (ChildObjects == null) return 0;

            foreach(var child in ChildObjects)
            {
                count += child.ChildCount();
            }

            count += ChildObjects.Count;

            return count;
        }

        /// <summary>
        /// Gets all children of this object recursively
        /// </summary>
        /// <returns>All children</returns>
        public HashSet<MySystemObject> GetAllChildren()
        {
            HashSet<MySystemObject> children = new HashSet<MySystemObject>();

            foreach(var child in children)
            {
                foreach(var c in child.GetAllChildren())
                {
                    children.Add(c);
                }
                children.Add(child);
            }

            return children;
        }
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
    public class MySystemPlanetMoon : MySystemPlanet
    {
        public MySystemPlanetMoon()
        {
            Type = MySystemObjectType.MOON;
            DisplayName = "";
            CenterPosition = Vector3D.Zero;
            SubtypeId = "";
            Diameter = 0;
            Generated = false;
        }
    }

    /// <summary>
    /// Class representing a ring or belt
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemRing : MySystemObject
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

        public MySystemRing()
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
    /// Enum for the body type of an object in the solar system.
    /// </summary>
    public enum MySystemObjectType
    {
        PLANET,
        RING,
        MOON,
        EMPTY
    }
}
