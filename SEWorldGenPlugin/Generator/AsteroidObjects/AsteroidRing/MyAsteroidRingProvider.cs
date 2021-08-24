using ProtoBuf;
using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using VRage;
using VRage.Library.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing
{
    /// <summary>
    /// The provider class for the asteroid ring object
    /// </summary>
    public class MyAsteroidRingProvider : MyAbstractAsteroidObjectProvider<MyAsteroidRingData>
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
        /// Creates new asteroid ring provider instance, replaces the old one
        /// </summary>
        public MyAsteroidRingProvider()
        {
            Static = this;
        }

        public override MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject parent, double objectOrbitRadius)
        {
            if (!Sync.IsServer)
            {
                MyPluginLog.Log("Tried to generate an Instance on the client, aborting...");
                return null;
            }

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
                ring.Width = MyRandom.Instance.NextDouble() * (planet.Diameter / 10 - planet.Diameter / 5) + planet.Diameter / 5;
                ring.Height = MyRandom.Instance.NextDouble() * (ring.Width / 10 - ring.Width / 20) + ring.Width / 20;
                ring.Radius = MyRandom.Instance.NextDouble() * (planet.Diameter * 2 - planet.Diameter * 0.75) + planet.Diameter * 0.75;
                ring.CenterPosition = asteroidObject.CenterPosition;

                m_savedData.Add(asteroidObject.Id, ring);

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

                m_savedData.Add(asteroidObject.Id, belt);

                return asteroidObject;

            }
        }

        public override bool IsInstanceGeneratable()
        {
            return true;
        }

        public override IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance)
        {
            if(m_savedData.ContainsKey(instance.Id))
                return MyAsteroidObjectShapeRing.CreateFromRingItem(m_savedData[instance.Id] as MyAsteroidRingData);
            return null;
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

            string name = namingScheme.SetProperty(MyNamingUtils.PROP_OBJ_NUMBER, beltIndex + 1)
                                      .SetProperty(MyNamingUtils.PROP_OBJ_NUMBER_GREEK, MyNamingUtils.GREEK_LETTERS[beltIndex % MyNamingUtils.GREEK_LETTERS.Length])
                                      .SetProperty(MyNamingUtils.PROP_OBJ_NUMBER_ROMAN, MyNamingUtils.ConvertNumberToRoman(beltIndex + 1))
                                      .SetProperty(MyNamingUtils.PROP_OBJ_LETTER_LOWER, (char)('a' + (beltIndex % 26)))
                                      .SetProperty(MyNamingUtils.PROP_OBJ_LETTER_UPPER, (char)('A' + (beltIndex % 26)));

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

        public override MyStarSystemDesignerObjectMenu CreateStarSystemDesignerEditMenu(MySystemAsteroids instance)
        {
            return new MyStarSystemDesignerAsteroidRingMenu(instance);
        }
    }

    /// <summary>
    /// Class representing additional data needed for a ring or belt
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyAsteroidRingData : IMyAsteroidData
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
