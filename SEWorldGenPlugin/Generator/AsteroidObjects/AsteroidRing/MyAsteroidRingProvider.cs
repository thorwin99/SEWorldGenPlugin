using ProtoBuf;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
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
    public class MyAsteroidRingProvider : MyAbstractAsteroidObjectProvider
    {
        public static readonly string TYPE_NAME = "AsteroidRing";

        private Dictionary<string, MySystemRing> m_loadedRings;

        private MyAsteroidRingAdminMenu m_adminMenuCreator;

        public MyAsteroidRingProvider()
        {
            m_loadedRings = new Dictionary<string, MySystemRing>();
        }

        public override MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject parent, double objectOrbitRadius)
        {
            if (parent != null && parent.Type == MySystemObjectType.PLANET)
            {
                MyPluginLog.Debug("Generating ring for planet " + parent.DisplayName);

                MySystemPlanet planet = parent as MySystemPlanet;

                MySystemAsteroids asteroidObject = new MySystemAsteroids();
                asteroidObject.DisplayName = GetRingName(parent.DisplayName);
                asteroidObject.ParentName = parent.DisplayName;
                asteroidObject.CenterPosition = parent.CenterPosition;

                MySystemRing ring = new MySystemRing();
                ring.AngleDegrees = new Vector3D(MyRandom.Instance.Next(-20, 20), MyRandom.Instance.Next(-20, 20), MyRandom.Instance.Next(-20, 20));
                ring.AsteroidSize = new MySerializableMinMax(64, 512);
                ring.Width = MyRandom.Instance.Next((int)planet.Diameter / 10, (int)planet.Diameter / 5);
                ring.Height = MyRandom.Instance.NextDouble() * (ring.Width / 10 - ring.Width / 20) + ring.Width / 20;
                ring.Radius = MyRandom.Instance.NextDouble() * (planet.Diameter * 2 - planet.Diameter * 0.75) + planet.Diameter * 0.75;
                ring.CenterPosition = asteroidObject.CenterPosition;

                m_loadedRings.Add(asteroidObject.DisplayName, ring);

                return asteroidObject;
            }
            else
            {
                MyPluginLog.Debug("Generating new Asteroid belt");

                var settings = MySettingsSession.Static.Settings.GeneratorSettings;

                MySystemAsteroids asteroidObject = new MySystemAsteroids();
                asteroidObject.DisplayName = GetBeltName(systemIndex);
                asteroidObject.CenterPosition = Vector3D.Zero;

                MySystemRing belt = new MySystemRing();
                belt.AngleDegrees = Vector3D.Zero;
                belt.AsteroidSize = new MySerializableMinMax(256, 1024);
                belt.Width = settings.MinMaxOrbitDistance.Min;
                belt.Height = belt.Width / 100;
                belt.Radius = objectOrbitRadius;
                belt.CenterPosition = asteroidObject.CenterPosition;

                m_loadedRings.Add(asteroidObject.DisplayName, belt);

                return asteroidObject;

            }
        }


        public override bool IsSystemGeneratable()
        {
            return true;
        }

        public override IMyAsteroidAdminMenuCreator GetAdminMenuCreator()
        {
            if(m_adminMenuCreator == null)
            {
                m_adminMenuCreator = new MyAsteroidRingAdminMenu();
            }
            return m_adminMenuCreator;
        }

        public override IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance)
        {
            return MyAsteroidObjectShapeRing.CreateFromRingItem(m_loadedRings[instance.DisplayName]);
        }

        public override bool TryLoadObject(MySystemAsteroids asteroid)
        {
            if (m_loadedRings.ContainsKey(asteroid.DisplayName)) return false;
            var ring = MyFileUtils.ReadXmlFileFromWorld<MySystemRing>(GetFileName(asteroid.DisplayName), typeof(MyAsteroidRingProvider));

            m_loadedRings.Add(asteroid.DisplayName, ring);

            return true;
        }

        public override void OnSave()
        {
            foreach(var ringName in m_loadedRings.Keys)
            {
                MyFileUtils.WriteXmlFileToWorld(m_loadedRings[ringName], GetFileName(ringName), typeof(MyAsteroidRingProvider));
            }
        }

        public override string GetTypeName()
        {
            return TYPE_NAME;
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
    public class MySystemRing
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

        /// <summary>
        /// The minimum and maximum size of the asteroids in this ring in meters.
        /// </summary>
        [ProtoMember(6)]
        public MySerializableMinMax AsteroidSize;

        public MySystemRing()
        {
            Radius = 0;
            Width = 0;
            Height = 0;
            AngleDegrees = Vector3D.Zero;
            AsteroidSize = new MySerializableMinMax(0, 0);
        }
    }
}
