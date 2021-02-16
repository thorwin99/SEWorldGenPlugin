using ProtoBuf;
using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Library.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing
{
    /// <summary>
    /// The provider class for the asteroid ring object
    /// </summary>
    [EventOwner]
    public class MyAsteroidRingProvider : MyAbstractAsteroidObjectProvider
    {
        /// <summary>
        /// The type name, for the system asteroid object, this class provides
        /// </summary>
        public static readonly string TYPE_NAME = "AsteroidRing";

        /// <summary>
        /// Static instance of this provider, so that there is only one provider existent ever.
        /// </summary>
        public static MyAsteroidRingProvider Static;

        /// <summary>
        /// The dictionary contains all currently loaded asteroid rings and belts
        /// </summary>
        private Dictionary<Guid, MyAsteroidRingData> m_loadedRings;

        /// <summary>
        /// Creates new asteroid ring provider instance, replaces the old one
        /// </summary>
        public MyAsteroidRingProvider()
        {
            if(Static == null)
            {
                m_loadedRings = new Dictionary<Guid, MyAsteroidRingData>();
                Static = this;
            }
            else
            {
                m_loadedRings = Static.m_loadedRings;
                Static = this;
            }
        }

        public override MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject parent, double objectOrbitRadius)
        {
            if (parent != null && parent.Type == MySystemObjectType.PLANET)
            {
                MyPluginLog.Debug("Generating ring for planet " + parent.DisplayName);

                MySystemPlanet planet = parent as MySystemPlanet;

                MySystemAsteroids asteroidObject = new MySystemAsteroids();
                asteroidObject.AsteroidTypeName = GetTypeName();
                asteroidObject.DisplayName = GetRingName(parent.DisplayName);
                asteroidObject.ParentId = parent.Id;
                asteroidObject.CenterPosition = parent.CenterPosition;
                asteroidObject.AsteroidSize = new MySerializableMinMax(64, 1024);

                MyAsteroidRingData ring = new MyAsteroidRingData();
                ring.AngleDegrees = new Vector3D(MyRandom.Instance.Next(-20, 20), MyRandom.Instance.Next(-20, 20), MyRandom.Instance.Next(-20, 20));
                ring.Width = MyRandom.Instance.Next((int)planet.Diameter / 10, (int)planet.Diameter / 5);
                ring.Height = MyRandom.Instance.NextDouble() * (ring.Width / 10 - ring.Width / 20) + ring.Width / 20;
                ring.Radius = MyRandom.Instance.NextDouble() * (planet.Diameter * 2 - planet.Diameter * 0.75) + planet.Diameter * 0.75;
                ring.CenterPosition = asteroidObject.CenterPosition;

                m_loadedRings.Add(asteroidObject.Id, ring);

                return asteroidObject;
            }
            else
            {
                MyPluginLog.Debug("Generating new Asteroid belt");

                var settings = MySettingsSession.Static.Settings.GeneratorSettings;

                MySystemAsteroids asteroidObject = new MySystemAsteroids();
                asteroidObject.DisplayName = GetBeltName(systemIndex);
                asteroidObject.CenterPosition = Vector3D.Zero;
                asteroidObject.AsteroidSize = new MySerializableMinMax(256, 1024);
                asteroidObject.AsteroidTypeName = GetTypeName();

                MyAsteroidRingData belt = new MyAsteroidRingData();
                belt.AngleDegrees = Vector3D.Zero;
                belt.Width = settings.MinMaxOrbitDistance.Min;
                belt.Height = belt.Width / 100;
                belt.Radius = objectOrbitRadius;
                belt.CenterPosition = asteroidObject.CenterPosition;

                m_loadedRings.Add(asteroidObject.Id, belt);

                return asteroidObject;

            }
        }

        public override bool IsSystemGeneratable()
        {
            return true;
        }

        protected override IMyAsteroidAdminMenuCreator CreateAdminMenuCreatorInstance()
        {
            return new MyAsteroidRingAdminMenu();
        }

        public override IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance)
        {
            if(m_loadedRings.ContainsKey(instance.Id))
                return MyAsteroidObjectShapeRing.CreateFromRingItem(m_loadedRings[instance.Id]);
            return null;
        }

        public override bool TryLoadObject(MySystemAsteroids asteroid)
        {
            if (m_loadedRings.ContainsKey(asteroid.Id)) return false;
            var ring = MyFileUtils.ReadXmlFileFromWorld<MyAsteroidRingData>(GetFileName(asteroid.DisplayName), typeof(MyAsteroidRingProvider));

            m_loadedRings.Add(asteroid.Id, ring);

            return true;
        }

        public override void OnSave()
        {
            foreach(var ringId in m_loadedRings.Keys)
            {
                var instance = MyStarSystemGenerator.Static.StarSystem.GetObjectById(ringId);
                if (instance == null) continue;
                MyFileUtils.WriteXmlFileToWorld(m_loadedRings[ringId], GetFileName(instance.DisplayName), typeof(MyAsteroidRingProvider));
            }
        }

        public override string GetTypeName()
        {
            return TYPE_NAME;
        }

        /// <summary>
        /// Adds a new instance of an asteroid ring to the star system.
        /// </summary>
        /// <param name="systemInstance">Instance of the system asteroid object to add</param>
        /// <param name="ringData">The asteroid ring data for the ring to add</param>
        /// <param name="successCallback">A callback that gets called, when the action was a success or not</param>
        public void AddInstance(MySystemAsteroids systemInstance, MyAsteroidRingData ringData, Action<bool> successCallback)
        {
            if (ringData == null || systemInstance == null)
            {
                successCallback?.Invoke(false);
            }
            MyStarSystemGenerator.Static.AddObjectToSystem(systemInstance, systemInstance.ParentId, delegate (bool success)
            {
                if (success)
                {
                    MyPluginLog.Debug("Successfully added asteroid object to system");

                    PluginEventHandler.Static.RaiseStaticEvent(AddRingServer, systemInstance, ringData);
                    successCallback?.Invoke(true);
                }
                else
                {
                    successCallback?.Invoke(false);
                }
            });
        }

        public override object GetInstanceData(MySystemAsteroids instance)
        {
            if (instance.AsteroidTypeName == GetTypeName())
            {
                return m_loadedRings[instance.Id];
            }
            return null;
        }

        public override bool RemoveInstance(MySystemAsteroids systemInstance)
        {
            MyPluginLog.Debug("Removing instance from asteroid ring provider");
            if (systemInstance.AsteroidTypeName != GetTypeName()) return false;
            if (!m_loadedRings.ContainsKey(systemInstance.Id)) return false;

            m_loadedRings.Remove(systemInstance.Id);

            MyFileUtils.DeleteFileInWorldStorage(GetFileName(systemInstance.DisplayName), typeof(MyAsteroidRingProvider));

            MyPluginLog.Debug("Successfully removed instance");

            return true;
        }

        /// <summary>
        /// Server event: Adds a new Asteroid ring on the server, if the provider is loaded on the server
        /// </summary>
        /// <param name="systemInstance">The instance of the object as an SystemAsteroids object</param>
        /// <param name="ringData">The custom data for the asteroid ring</param>
        [Event(100001)]
        [Server]
        private static void AddRingServer(MySystemAsteroids systemInstance, MyAsteroidRingData ringData)
        {
            Static?.m_loadedRings.Add(systemInstance.Id, ringData);
        }

        /// <summary>
        /// Generates a name for the asteroid belt based on the naming scheme
        /// defined in the global settings file
        /// </summary>
        /// <param name="beltIndex">Index of the belt in the system</param>
        /// <returns>Name for the belt</returns>
        private string GetBeltName(int beltIndex)
        {
            string namingScheme = MySettings.Static.Settings.BeltNameFormat;

            string name = namingScheme.SetProperty("ObjectNumber", beltIndex + 1)
                                      .SetProperty("ObjectNumberGreek", MyNamingUtils.GREEK_LETTERS[beltIndex % MyNamingUtils.GREEK_LETTERS.Length])
                                      .SetProperty("ObjectNumberRoman", MyNamingUtils.ConvertNumberToRoman(beltIndex + 1))
                                      .SetProperty("ObjectLetterLower", (char)('a' + (beltIndex % 26)))
                                      .SetProperty("ObjectLetterUpper", (char)('A' + (beltIndex % 26)));

            return name;
        }

        /// <summary>
        /// Generates the name for an asteroid ring
        /// </summary>
        /// <param name="parentPlanetName">Name of the parent planet</param>
        /// <returns>Name for the ring</returns>
        private string GetRingName(string parentPlanetName)
        {
            return parentPlanetName + " Ring";
        }
    }

    /// <summary>
    /// Class representing additional data needed for a ring or belt
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyAsteroidRingData
    {
        /// <summary>
        /// The radius of the asteroid ring to the center.
        /// This is the distance of the area, where asteroids spawn
        /// to the center. In meters
        /// </summary>
        [ProtoMember(1)]
        public double Radius;

        /// <summary>
        /// The position of the objects center
        /// </summary>
        [ProtoMember(2)]
        public SerializableVector3D CenterPosition;

        /// <summary>
        /// The width of the asteroid ring. This is the width of the area where
        /// asteroids will spawn. In meters
        /// </summary>
        [ProtoMember(3)]
        public double Width;

        /// <summary>
        /// The height of the area, where asteroids will spawn. In meters
        /// </summary>
        [ProtoMember(4)]
        public double Height;

        /// <summary>
        /// The angle of the asteroid ring around the planet. Needs to have 3 components.
        /// X is the angle around the x axis, y around the y axis and z around the z axis.
        /// The angles should be in degrees (0 to 360)
        /// </summary>
        [ProtoMember(5)]
        public SerializableVector3D AngleDegrees;

        public MyAsteroidRingData()
        {
            Radius = 0;
            Width = 0;
            Height = 0;
            AngleDegrees = Vector3D.Zero;
        }
    }
}
