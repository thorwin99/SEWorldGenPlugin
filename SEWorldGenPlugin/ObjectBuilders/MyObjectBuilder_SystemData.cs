using ProtoBuf;
using SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// A fast access cache to quickly retrieve system objects and avoid traversal of system tree each time.
        /// </summary>
        private Dictionary<Guid, MySystemObject> m_objectCache = new Dictionary<Guid, MySystemObject>();

        /// <summary>
        /// A fast access cache to quickly retrieve system objects fo certain type and avoid traversal of system tree each time.
        /// </summary>
        private Dictionary<MySystemObjectType, HashSet<MySystemObject>> m_objectTypeCache = new Dictionary<MySystemObjectType, HashSet<MySystemObject>>();

        /// <summary>
        /// Objects that are located in this star system. Do not set directly unless you are calling <see cref="RebuildCache"/> afterwards.
        /// </summary>
        [ProtoMember(1)]
        public MySystemObject CenterObject;

        /// <summary>
        /// Constructs new System Data object builder and initializes cache.
        /// </summary>
        public MyObjectBuilder_SystemData()
        {
            RebuildCache();
        }

        /// <summary>
        /// Rebuilds the system cache for fast access. Rebuild cache after setting system center object 
        /// or loading the system it from a storage file.
        /// When adding or removing objects, the cache is automatically updated.
        /// </summary>
        public void RebuildCache()
        {
            m_objectCache.Clear();
            m_objectTypeCache.Clear();
            m_objectTypeCache.Add(MySystemObjectType.ASTEROIDS, new HashSet<MySystemObject>());
            m_objectTypeCache.Add(MySystemObjectType.PLANET, new HashSet<MySystemObject>());
            m_objectTypeCache.Add(MySystemObjectType.MOON, new HashSet<MySystemObject>());
            m_objectTypeCache.Add(MySystemObjectType.EMPTY, new HashSet<MySystemObject>());

            Foreach(delegate (int i, MySystemObject obj)
            {
                m_objectCache.Add(obj.Id, obj);
                m_objectTypeCache[obj.Type].Add(obj);
            });
        }

        /// <summary>
        /// Gets all objects currently in the system
        /// </summary>
        /// <returns>All objects</returns>
        public HashSet<MySystemObject> GetAll()
        {
            return m_objectCache.Values.ToHashSet();
        }

        /// <summary>
        /// Accesses system cache to return all objects of a specific system object type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public HashSet<MySystemObject> GetAllByType(MySystemObjectType type)
        {
            return m_objectTypeCache[type];
        }

        /// <summary>
        /// Returns the amount of objects in the system
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            if (CenterObject == null) return 0;
            return m_objectCache.Count;
        }

        /// <summary>
        /// Return the depth of the object in the system hierarchy.
        /// </summary>
        /// <param name="id">Id of the object</param>
        /// <returns>The depth of the object or -1 if it is not in the system</returns>
        public int GetDepth(Guid id)
        {
            int d = -1;
            Foreach((depth, obj) =>
            {
                if(obj.Id == id)
                {
                    d = depth;
                }
            });

            return d;
        }

        /// <summary>
        /// Accesses system cache to quickly retrieve the object associated with the given id.
        /// </summary>
        /// <param name="id">Id of the object</param>
        /// <returns>The object or null if not found</returns>
        public MySystemObject GetById(Guid id)
        {
            if(m_objectCache.TryGetValue(id, out MySystemObject obj))
            {
                return obj;
            }
            return null;
        }

        /// <summary>
        /// Checks if an object with given id exists
        /// </summary>
        /// <param name="id">Id of the object</param>
        /// <returns>true if it exists</returns>
        public bool Contains(Guid id)
        {
            return m_objectCache.ContainsKey(id);
        }

        /// <summary>
        /// Removes the object with the given id. Cant be the center object.
        /// All child objects will be removed too.
        /// </summary>
        /// <param name="id">Id of the object</param>
        /// <returns>True, if object was successfully removed.</returns>
        public bool Remove(Guid id)
        {
            if (CenterObject == null) return false;
            if (id == CenterObject.Id) return false;
            if (!Contains(id)) return false;

            var obj = GetById(id);
            var parent = GetById(obj.ParentId);

            if (parent == null) return false;

            parent.ChildObjects.Remove(obj);
            m_objectCache.Remove(id);
            m_objectTypeCache[obj.Type].Remove(obj);

            return true;
        }

        /// <summary>
        /// Tries to add the given System Object <paramref name="obj"/> as a child under <paramref name="parent"/>
        /// The currently set parent id of the object will be ignored, instead use <paramref name="parent"/>
        /// If the parent does not exist, the object wont be added.
        /// If the parent equals <see cref="Guid.Empty"/> and no center object exists, it will be added as the center object.
        /// </summary>
        /// <param name="obj">The object to add to the system</param>
        /// <param name="parent">The parent of the object</param>
        /// <returns>False if the <paramref name="parent"/> does not exist or the system already conains an object with the same id as <paramref name="obj"/></returns>
        public bool Add(MySystemObject obj, Guid parent)
        {
            if (Contains(obj.Id)) return false;

            if(parent == Guid.Empty && CenterObject == null)
            {
                CenterObject = obj;

                m_objectCache.Add(obj.Id, obj);
                m_objectTypeCache[obj.Type].Add(obj);

                return true;
            }

            var p = GetById(parent);
            if(p != null)
            {
                p.ChildObjects.Add(obj);
                obj.ParentId = parent;

                m_objectCache.Add(obj.Id, obj);
                m_objectTypeCache[obj.Type].Add(obj);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to add the given System Object <paramref name="obj"/> as a child under its parent.
        /// The currently set parent id of the object is used.
        /// If the parent does not exist, the object wont be added.
        /// </summary>
        /// <param name="obj">The object to add to the system</param>
        /// <param name="parent">The parent of the object</param>
        /// <returns>False if the parent does not exist or the system already conains an object with the same id as <paramref name="obj"/></returns>
        public bool Add(MySystemObject obj)
        {
            return Add(obj, obj.ParentId);
        }

        /// <summary>
        /// Executes the <paramref name="action"/> for each object in the system, where the first
        /// parameter is the depth of the object in the system tree and the second is the object itself.
        /// </summary>
        /// <param name="action">Action to execute for each system object</param>
        public void Foreach(Action<int, MySystemObject> action)
        {
            if (CenterObject == null) return;
            CenterObject.Iterate(action);
        }
    }

    /// <summary>
    /// Baseclass for all system objects containing a type and display name
    /// </summary>
    [ProtoContract]
    [ProtoInclude(5001, typeof(MySystemPlanet))]
    [ProtoInclude(5003, typeof(MySystemAsteroids))]
    [XmlInclude(typeof(MySystemPlanet))]
    [XmlInclude(typeof(MySystemAsteroids))]
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
        /// Id of the system object for unique identification.
        /// Is generated automatically on creation of the object
        /// </summary>
        [ProtoMember(3)]
        public Guid Id;

        /// <summary>
        /// The position of the objects center
        /// </summary>
        [ProtoMember(4)]
        public SerializableVector3D CenterPosition;

        /// <summary>
        /// The name of this objects parent
        /// </summary>
        [ProtoMember(5)]
        public Guid ParentId;

        /// <summary>
        /// All Child objects, such as moons. Do not directly add or remove objects, as it will break the Solar system object cache.
        /// </summary>
        [ProtoMember(6)]
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
            ChildObjects = new HashSet<MySystemObject>();
            Id = Guid.NewGuid();
            ParentId = Guid.Empty;
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

            foreach(var child in ChildObjects)
            {
                children.Add(child);
                foreach (var c in child.GetAllChildren())
                {
                    children.Add(c);
                }
            }

            return children;
        }

        /// <summary>
        /// Starts iteration over this system objects sub tree and executes the <paramref name="iterateAction"/>
        /// with parameters relative depth and the current iterated object.
        /// </summary>
        /// <param name="iterateAction">Action to execute on each object</param>
        public void Iterate(Action<int, MySystemObject> iterateAction)
        {
            iterateAction(0, this);

            foreach(var child in ChildObjects)
            {
                child.Iterate(1, iterateAction);
            }
        }

        /// <summary>
        /// Starts iteration over this system objects sub tree and executes the <paramref name="iterateAction"/>
        /// with parameters depth and the current iterated object.
        /// </summary>
        /// <param name="iterateAction">Action to execute on each object</param>
        /// <param name="depth">The current depth of this object</param>
        private void Iterate(int depth, Action<int, MySystemObject> iterateAction)
        {
            iterateAction(depth, this);

            foreach (var child in ChildObjects)
            {
                child.Iterate(depth + 1, iterateAction);
            }
        }
    }

    /// <summary>
    /// Class representing a planets data in the solar system
    /// </summary>
    [ProtoContract]
    [ProtoInclude(5004, typeof(MySystemPlanetMoon))]
    [XmlInclude(typeof(MySystemPlanetMoon))]
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
        /// Entity id of the generated planet
        /// </summary>
        [ProtoMember(7)]
        public long EntityId;

        public MySystemPlanet()
        {
            Type = MySystemObjectType.PLANET;
            DisplayName = "";
            CenterPosition = Vector3D.Zero;
            SubtypeId = "";
            Diameter = 0;
            Generated = false;
            EntityId = 0;
        }

        /// <summary>
        /// Tries to get a ring that is a child of this planet
        /// </summary>
        /// <param name="ring">Out the ring of the planet</param>
        /// <returns>True, if a ring was found</returns>
        public bool TryGetPlanetRing(out MySystemAsteroids ring)
        {
            foreach(var child in ChildObjects)
            {
                if(child is MySystemAsteroids)
                {
                    var asteroid = child as MySystemAsteroids;
                    if (asteroid.AsteroidTypeName != MyAsteroidRingProvider.TYPE_NAME) continue;

                    ring = child as MySystemAsteroids;

                    return true;
                }
            }

            ring = null;
            return false;
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
    /// Class for all asteroid objects.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySystemAsteroids : MySystemObject
    {
        /// <summary>
        /// Name of the type this asteroid consists of.
        /// </summary>
        [ProtoMember(4)]
        public string AsteroidTypeName;

        /// <summary>
        /// The minimum and maximum size of the asteroids in this object in meters.
        /// </summary>
        [ProtoMember(5)]
        public MySerializableMinMax AsteroidSize;

        public MySystemAsteroids()
        {
            Type = MySystemObjectType.ASTEROIDS;
            DisplayName = "";
            CenterPosition = Vector3D.Zero;
            AsteroidSize = new MySerializableMinMax(32, 1024);
        }
    }

    /// <summary>
    /// Enum for the body type of an object in the solar system.
    /// </summary>
    public enum MySystemObjectType
    {
        PLANET,
        MOON,
        ASTEROIDS,
        EMPTY
    }
}
