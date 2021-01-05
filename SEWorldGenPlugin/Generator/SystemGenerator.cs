using Sandbox.Definitions;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Library.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator
{
    /// <summary>
    /// Session component that generates the solar system data for the solar system of
    /// the current game session / world. Is a singleton class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 600)]
    [EventOwner]
    public partial class SystemGenerator : MySessionComponentBase
    {
        private string[] greek_letters = new string[] {"Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "My", "Ny", "Xi", "Omikron", "Pi", "Rho", "Sigma", "Tau", "Ypsilon", "Phi", "Chi", "Psi", "Omega"};

        private const string STORAGE_FILE = "SystemData.xml";

        private List<string> vanilla_planets = new List<string> { "EarthLike", "Mars", "Triton", "Alien", "Europa", "Titan", "Moon" };

        /// <summary>
        /// All celestial bodies present in the solar system
        /// </summary>
        public HashSet<MySystemItem> Objects
        {
            get;
            private set;
        }

        /// <summary>
        /// All planetary defintions that are loaded to be used as planets
        /// </summary>
        public List<MyPlanetGeneratorDefinition> PlanetDefinitions
        {
            get;
            private set;
        }

        /// <summary>
        /// All planetary defintions that are loaded to be used as moons
        /// </summary>
        public List<MyPlanetGeneratorDefinition> m_moonDefinitions
        {
            get;
            private set;
        }

        /// <summary>
        /// All planetary defintions that are mandatory to be generated as planets
        /// </summary>
        public List<MyPlanetGeneratorDefinition> MandatoryPlanets
        {
            get;
            private set;
        }

        /// <summary>
        /// All planetary defintions that are defined as gas Giants and therefore double the size
        /// </summary>
        public List<MyPlanetGeneratorDefinition> GasGiants
        {
            get;
            private set;
        }

        /// <summary>
        /// All planetary definitions that available for generation as planets
        /// </summary>
        public List<MyPlanetGeneratorDefinition> AvailablePlanets
        {
            get;
            private set;
        }


        /// <summary>
        /// All planetary definitions that are available for generation as moons
        /// </summary>
        public List<MyPlanetGeneratorDefinition> AvailableMoons
        {
            get;
            private set;
        }

        private int m_seed;
        private GeneratorSettings m_settings;

        /// <summary>
        /// Singleton instance of the SystemGenerator
        /// </summary>
        public static SystemGenerator Static;

        /// <summary>
        /// Executed after loading and initializes the generator.
        /// It generates the system, if it should.
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            MyPluginLog.Log("Initializing system generator");

            InitNet();

            if (!Sync.IsServer || !MySettingsSession.Static.Settings.Enable || MySession.Static.Settings.WorldSizeKm > 0) return;

            LegacyMyObjectBuilder_StarSystem b = GetConfig();
            Objects = b.SystemObjects;

            m_seed = MySession.Static.Settings.ProceduralSeed + Guid.NewGuid().GetHashCode();

            m_settings = MySettingsSession.Static.Settings.GeneratorSettings;

            if (b == null || Objects == null || Objects.Count == 0)
            {
                GenerateSystem();
            }

            MySession.Static.OnReady += delegate
            {
                if(MySettingsSession.Static.Settings.GeneratorSettings.BeltSettings.ShowBeltGPS)
                    AddBeltsGpss();
            };
        } 

        /// <summary>
        /// Loads required Planetary definitions and filters them, also initializes networking.
        /// </summary>
        public override void LoadData()
        {
            MyPluginLog.Log("Loading definitions and network data");

            Static = this;

            LoadNet();

            PlanetDefinitions = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions().ToList();
            MandatoryPlanets = new List<MyPlanetGeneratorDefinition>();
            m_moonDefinitions = new List<MyPlanetGeneratorDefinition>();
            GasGiants = new List<MyPlanetGeneratorDefinition>();
            AvailableMoons = new List<MyPlanetGeneratorDefinition>();
            AvailablePlanets = new List<MyPlanetGeneratorDefinition>();
            FilterDefinitions();
        }

        /// <summary>
        /// Saves the systemdata configuration file, if plugin is enabled.
        /// </summary>
        public override void SaveData()
        {
            if (!Sync.IsServer || !MySettingsSession.Static.Settings.Enable) return;

            MyPluginLog.Log("Saving system data");

            SaveConfig();
        }

        /// <summary>
        /// Unloads all data used by this session component
        /// </summary>
        protected override void UnloadData()
        {
            MyPluginLog.Log("Unloading system generator data");

            UnloadNet();
            Objects?.Clear();
            PlanetDefinitions?.Clear();
            m_moonDefinitions?.Clear();
            MandatoryPlanets?.Clear();
            GasGiants?.Clear();
            m_settings = null;
            Static = null;
        }

        /// <summary>
        /// Tries to get the system object with the given name
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="obj">Output value if return is true</param>
        /// <returns>True if object exists in the system</returns>
        private bool TryGetObject(string name, out MySystemItem obj)
        {
            foreach(var o in Objects)
            {
                if (o.DisplayName.Equals(name))
                {
                    obj = o;
                    return true;
                }
                else if(o.Type == LegacySystemObjectType.PLANET)
                {
                    MyPlanetItem p = (MyPlanetItem)o;
                    foreach(var moon in p.PlanetMoons)
                    {
                        if (moon.DisplayName.Equals(name))
                        {
                            obj = moon;
                            return true;
                        }
                    }
                    if(p.PlanetRing != null)
                    {
                        if (p.PlanetRing.DisplayName.Equals(name))
                        {
                            obj = p.PlanetRing;
                            return true;
                        }
                    }
                }
            }
            obj = null;
            return false;
        }

        /// <summary>
        /// Generates the solar system by randomly generating planets or asteroid belts,
        /// probabilities and settings are defined in the MyObjectBuilder_PluginSettings loaded with the world and set in
        /// OnInit().
        /// </summary>
        private void GenerateSystem()
        {
            MyPluginLog.Log("Generating new solar system");

            Objects = new HashSet<MySystemItem>();

            using (MyRandom.Instance.PushSeed(m_seed))
            {
                int numberPlanets = MyRandom.Instance.Next(m_settings.MinObjectsInSystem, m_settings.MaxObjectsInSystem + 1);
                long tmp_distance = 0;
                int totalBelts = 0;
                int totalPlanets = 0;

                for (int i = 0; i < numberPlanets; i++)
                {
                    if (tmp_distance >= m_settings.WorldSize && m_settings.WorldSize > -1) return;

                    if (i != 0 || !m_settings.FirstPlanetCenter)
                    {
                        int distToPrev = MyRandom.Instance.Next(m_settings.MinOrbitDistance, m_settings.MaxOrbitDistance);
                        tmp_distance += distToPrev;
                    }


                    if(MyRandom.Instance.NextDouble()/* * ((i % 6) * (i % 6) / 12.5)*/ < 1 - m_settings.BeltSettings.BeltProbability && PlanetDefinitions.Count > 0){
                        GeneratePlanet(i, tmp_distance, numberPlanets, ref totalPlanets);
                    }
                    else
                    {
                        GenerateBelt(tmp_distance, ref totalBelts);
                    }
                }

                if (MySettingsSession.Static.Settings.GeneratorSettings.PlanetsOnlyOnce) return;

                if(MandatoryPlanets.Count != 0)
                {
                    int length = MandatoryPlanets.Count + totalPlanets;
                    for (int i = totalPlanets; i < length; i++)
                    {
                        if (tmp_distance > m_settings.WorldSize && m_settings.WorldSize != -1) return;

                        int distToPrev = MyRandom.Instance.Next(m_settings.MinOrbitDistance, m_settings.MaxOrbitDistance);
                        tmp_distance += distToPrev;

                        GeneratePlanet(i, tmp_distance, numberPlanets, ref totalPlanets);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a asteroid belts data at distance from the center of the map. It is the beltIndex+1 -th belt
        /// </summary>
        /// <param name="distance">Distance of the belt to the center of the map</param>
        /// <param name="beltIndex">The index of the belt</param>
        private void GenerateBelt(long distance, ref int beltIndex)
        {
            MySystemBeltItem belt = new MySystemBeltItem();

            string name = MySettingsSession.Static.Settings.GeneratorSettings.BeltSettings.BeltNameFormat
                .SetProperty("ObjectNumber", beltIndex + 1)
                .SetProperty("ObjectNumberGreek", greek_letters[beltIndex % greek_letters.Length])
                .SetProperty("ObjectNumberRoman", ConvertNumberToRoman(beltIndex + 1))
                .SetProperty("ObjectLetterLower", (char)('a' + (beltIndex % 26)))
                .SetProperty("ObjectLetterUpper", (char)('A' + (beltIndex % 26)));

            ++beltIndex;

            belt.DisplayName = name;
            belt.Type = LegacySystemObjectType.BELT;
            belt.Height = MyRandom.Instance.Next(m_settings.BeltSettings.MinBeltHeight, m_settings.BeltSettings.MaxBeltHeight);
            belt.Radius = distance;
            belt.Width = MyRandom.Instance.Next(belt.Height * 10, belt.Height * 100);
            belt.RoidSize = MyRandom.Instance.Next(256, 512);

            Objects.Add(belt);
        }

        /// <summary>
        /// Generates a planets data and possible moons or an asteroid ring for the planet
        /// </summary>
        /// <param name="index">Index of the planet in the system</param>
        /// <param name="distance">Distance from the world center</param>
        /// <param name="totalObjects">Total number of objects in the system</param>
        /// <param name="planetIndex">Index of the planet of the planets (n-th planet)</param>
        private void GeneratePlanet(int index, long distance, int totalObjects, ref int planetIndex)
        {
            MyPlanetItem planet = new MyPlanetItem();

            double mod = distance == 0 && m_settings.FirstPlanetCenter ? 1 : Math.Sin(index * Math.PI / totalObjects);

            var def = GetPlanetDefinition((int)(m_settings.PlanetSettings.PlanetSizeCap * mod));
            bool isGasGiant = GasGiants.Contains(def);
            var size = SizeByGravity(def.SurfaceGravity, isGasGiant);

            var angle = MyRandom.Instance.GetRandomFloat(0, (float)(2 * Math.PI));
            var height = MyRandom.Instance.GetRandomFloat((float)Math.PI / 180 * -5, (float)Math.PI / 180 * 5);
            Vector3D pos = new Vector3D(distance * Math.Sin(angle), distance * Math.Cos(angle), distance * Math.Sin(height));

            string name = MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.PlanetNameFormat
                .SetProperty("ObjectNumber", planetIndex + 1)
                .SetProperty("ObjectNumberGreek", greek_letters[planetIndex % greek_letters.Length])
                .SetProperty("ObjectNumberRoman", ConvertNumberToRoman(planetIndex + 1))
                .SetProperty("ObjectLetterLower", (char)('a' + (planetIndex % 26)))
                .SetProperty("ObjectLetterUpper", (char)('A' + (planetIndex % 26)))
                .SetProperty("ObjectId", def.Id.SubtypeId.String);

            ++planetIndex;

            planet.DisplayName = name;
            planet.Type = LegacySystemObjectType.PLANET;
            planet.DefName = def.Id.SubtypeId.String;
            planet.Size = size;
            planet.PlanetRing = GenerateRing(def.SurfaceGravity, planet.Size);
            planet.OffsetPosition = pos;
            planet.CenterPosition = pos;
            planet.PlanetMoons = GenerateMoons(planet.Size, def.SurfaceGravity, planet.DisplayName, distance);
            planet.Generated = false;

            Objects.Add(planet);
        }

        /// <summary>
        /// Generates moon datas for a planet of the given size, gravity, name and distance from the world center.
        /// The moon has to have a size smaller than the planet
        /// </summary>
        /// <param name="planetSize">Size of the parent planet</param>
        /// <param name="surfaceGravity">Surface gravity of the parent planet</param>
        /// <param name="planetName">Name of the planet, used for naming the moon</param>
        /// <param name="distance">Distance</param>
        /// <returns>An array of all generated moons</returns>
        private MyPlanetMoonItem[] GenerateMoons(float planetSize, float surfaceGravity, string planetName, long distance)
        {
            if (MyRandom.Instance.NextFloat() > m_settings.PlanetSettings.MoonProbability) return new MyPlanetMoonItem[0];

            int maxMoons = GetMaxMoonCount(surfaceGravity);
            int numMoons = MyRandom.Instance.Next(maxMoons > 0 ? 1 : 0, maxMoons + 1);
            MyPlanetMoonItem[] moons = new MyPlanetMoonItem[numMoons];

            for(int i = 0; i < numMoons; i++)
            {
                var dist = planetSize * (i + 1) + planetSize * MyRandom.Instance.GetRandomFloat(0.5f, 1.5f);
                var def = GetPlanetMoonDefinition(planetSize * 0.8f);
                bool isGasGiant = GasGiants.Contains(def);

                if (dist + distance > m_settings.WorldSize && m_settings.WorldSize > 0) return moons;

                string name = MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.MoonNameFormat
                .SetProperty("ObjectNumber", i + 1)
                .SetProperty("ObjectNumberGreek", greek_letters[i % greek_letters.Length])
                .SetProperty("ObjectNumberRoman", ConvertNumberToRoman(i + 1))
                .SetProperty("ObjectLetterLower", (char)('a' + (i % 26)))
                .SetProperty("ObjectLetterUpper", (char)('A' + (i % 26)))
                .SetProperty("ObjectId", def.Id.SubtypeId.String)
                .SetProperty("MoonPlanetName", planetName);

                MyPlanetMoonItem item = new MyPlanetMoonItem();
                item.Type = LegacySystemObjectType.MOON;
                item.DefName = def.Id.SubtypeName.ToString();
                item.Distance = dist;
                item.Size = SizeByGravity(def.SurfaceGravity, isGasGiant);
                item.DisplayName = name;

                moons[i] = item;
            }

            return moons;
        }

        /// <summary>
        /// Generates an asteroid rings data for a planet with given size and surface gravity
        /// </summary>
        /// <param name="surfaceGravity">Surface gravity of the parent planet</param>
        /// <param name="planetSize">Size of the parent planet</param>
        /// <returns>The generated asteroid ring, if generated</returns>
        private MyPlanetRingItem GenerateRing(float surfaceGravity, float planetSize)
        {
            if (MyRandom.Instance.NextFloat() > m_settings.PlanetSettings.RingSettings.PlanetRingProbability * surfaceGravity) return null;

            MyPlanetRingItem ring = new MyPlanetRingItem();

            ring.Type = LegacySystemObjectType.RING;
            ring.RoidSize = MyRandom.Instance.Next(64, Math.Min((int)(Math.Max(surfaceGravity * 0.5 * 128, 64)), 512));
            ring.Width = MyRandom.Instance.Next(m_settings.PlanetSettings.RingSettings.MinPlanetRingWidth, m_settings.PlanetSettings.RingSettings.MaxPlanetRingWidth);
            ring.Height = MyRandom.Instance.Next(m_settings.PlanetSettings.RingSettings.MinPlanetRingWidth / 10, ring.Width / 10);
            ring.AngleDegrees = MyRandom.Instance.Next(-20, 20);
            ring.Radius = MyRandom.Instance.Next((int)(planetSize * 0.75), (int)(planetSize * 2));
            ring.DisplayName = "";

            return ring;
        }

        bool m_usedAllPlanets = false;

        /// <summary>
        /// Gets a fit planetary definition which should be smaller than maximum size, if all mandatory planets have been used.
        /// Mandatory planet will always fit. If no definition could be found after 10000 tries, it will disregard the restrictions
        /// </summary>
        /// <param name="maximumSize">Maximum size of the planet</param>
        /// <param name="ignoreMandatory">Whether to ignore mandatory planets</param>
        /// <returns>The MyPlanetGeneratorDefinition of the planet</returns>
        private MyPlanetGeneratorDefinition GetPlanetDefinition(float maximumSize, bool ignoreMandatory = false)
        {
            int tries = 0;
            float size;
            MyPlanetGeneratorDefinition def;

            if(MandatoryPlanets.Count > 0 && !ignoreMandatory)
            {
                def = MandatoryPlanets[0];
                MandatoryPlanets.RemoveAt(0);
            }
            else
            {
                do
                {
                    def = AvailablePlanets[MyRandom.Instance.Next(0, AvailablePlanets.Count)];
                    size = SizeByGravity(def.SurfaceGravity);
                    tries++;

                } while (size > maximumSize && tries < 10000);
            }
            

            if (MySettingsSession.Static.Settings.GeneratorSettings.PlanetsOnlyOnce && !m_usedAllPlanets)
            {
                AvailablePlanets.Remove(def);
                if(AvailablePlanets.Count <= 0)
                {
                    AvailablePlanets = PlanetDefinitions;
                    m_usedAllPlanets = true;
                }
            }

            return def;
        }

        bool m_usedAllMoons = false;

        /// <summary>
        /// Gets a fit planetary definition which should be smaller than maximum size.
        /// If no definition could be found after 10000 tries, it will disregard the restrictions
        /// </summary>
        /// <param name="maximumSize">Maximum size of the planet</param>
        /// <returns>The MyPlanetGeneratorDefinition of the moon</returns>
        private MyPlanetGeneratorDefinition GetPlanetMoonDefinition(float maximumSize)
        {
            int tries = 0;
            float size;
            MyPlanetGeneratorDefinition def;

            if (m_moonDefinitions.Count == 0) return GetPlanetDefinition(maximumSize, true);

            do
            {
                def = AvailableMoons[MyRandom.Instance.Next(0, AvailableMoons.Count * 2) % AvailableMoons.Count];

                size = SizeByGravity(def.SurfaceGravity);
                tries++;

            } while (size >= maximumSize && tries < 10000);

            if (MySettingsSession.Static.Settings.GeneratorSettings.MoonsOnlyOnce && !m_usedAllMoons)
            {
                AvailableMoons.Remove(def);
                if (AvailableMoons.Count <= 0)
                {
                    AvailableMoons = m_moonDefinitions;
                    m_usedAllMoons = true;
                }
            }

            return def;
        }

        /// <summary>
        /// Gets a diameter for a planet with given gravity.
        /// </summary>
        /// <param name="gravity">Gravity of the planet</param>
        /// <param name="isGasGiant">Whether or not it is a gas giant and should use double the size</param>
        /// <returns>The diameter of the planet</returns>
        private float SizeByGravity(float gravity, bool isGasGiant = false)
        {
            float multiplier = isGasGiant ? m_settings.PlanetSettings.SizeMultiplier * 2.0f : m_settings.PlanetSettings.SizeMultiplier;

           return (float)Math.Min(Math.Sqrt(gravity * 120000 * 120000 * multiplier * multiplier), m_settings.PlanetSettings.PlanetSizeCap);
        }

        /// <summary>
        /// Gets the maximum amount of moons a planet with given gravity can have. Gas giants can have more.
        /// </summary>
        /// <param name="gravity">Gravity of the planet</param>
        /// <param name="isGasGiant">Whether or not it is a gas giant</param>
        /// <returns></returns>
        private int GetMaxMoonCount(float gravity, bool isGasGiant = false)
        {
            float m = isGasGiant ? 2 : 1;
            float o = isGasGiant ? 0.3f : 0;
            return (int)Math.Floor(gravity * m * 5 * (MyRandom.Instance.NextFloat() + o));
        }

        /// <summary>
        /// Shuffles the list of mandatory planets
        /// </summary>
        private void ShuffleMandatoryPlanets()
        {
            int n = MandatoryPlanets.Count - 1;
            if (n <= 0) return;
            while(n > 1)
            {
                int index = MyRandom.Instance.Next(1, n);
                var value = MandatoryPlanets[index];
                MandatoryPlanets[index] = MandatoryPlanets[n];
                MandatoryPlanets[n] = value;
                n--;
            }
        }

        /// <summary>
        /// Filters all planetary definitions based on settings set in the Plugin settings file and puts them in their respective lists.
        /// </summary>
        private void FilterDefinitions()
        {
            List<MyPlanetGeneratorDefinition> toRemovePlanets = new List<MyPlanetGeneratorDefinition>();
            foreach (var p in PlanetDefinitions)
            {
                if (p.Id.SubtypeId.String.Contains("Tutorial") || p.Id.SubtypeId.String.Contains("TestMap") || p.Id.SubtypeId.String.Contains("ModExample"))
                {
                    toRemovePlanets.Add(p);
                    continue;
                }
                if (!MySettingsSession.Static.Settings.GeneratorSettings.UseVanillaPlanets && vanilla_planets.Contains(p.Id.SubtypeId.String))
                {
                    toRemovePlanets.Add(p);
                    continue;
                }
                if (MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.BlacklistedPlanets.Contains(p.Id.SubtypeId.String))
                {
                    toRemovePlanets.Add(p);
                }
                if (MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.Moons.Contains(p.Id.SubtypeId.String))
                {
                    toRemovePlanets.Add(p);
                    m_moonDefinitions.Add(p);
                    AvailableMoons.Add(p);
                }
                if (MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.MandatoryPlanets.Contains(p.Id.SubtypeId.String) || MySettingsSession.Static.Settings.GeneratorSettings.SemiRandomizedGeneration)
                {
                    MandatoryPlanets.Add(p);
                }
                if (MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.GasGiants.Contains(p.Id.SubtypeId.String))
                {
                    GasGiants.Add(p);
                }
            }

            foreach(var r in toRemovePlanets)
            {
                PlanetDefinitions.Remove(r);
            }

            foreach(var p in PlanetDefinitions)
            {
                AvailablePlanets.Add(p);
            }

            ShuffleMandatoryPlanets();
        }

        /// <summary>
        /// Loads the systemData.xml of the world, if it exist
        /// </summary>
        /// <returns>MyObjectBuilder_StarSystem system data</returns>
        private LegacyMyObjectBuilder_StarSystem GetConfig()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(STORAGE_FILE, typeof(SystemGenerator)))
            {
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(STORAGE_FILE, typeof(SystemGenerator)))
                    {
                        LegacyMyObjectBuilder_StarSystem saveFile = MyAPIGateway.Utilities.SerializeFromXML<LegacyMyObjectBuilder_StarSystem>(reader.ReadToEnd());
                        return saveFile;
                    }
                }
                catch (Exception e)
                {
                    MyPluginLog.Log("Couldnt load Starsystem save file.", LogLevel.ERROR);
                    MyPluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);

                    MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(SystemGenerator));
                    return null;
                }
            }
            else
            {
                return new LegacyMyObjectBuilder_StarSystem();
            }
        }

        /// <summary>
        /// Saves the system objects data to systemData.xml
        /// </summary>
        private void SaveConfig()
        {
            LegacyMyObjectBuilder_StarSystem conf = new LegacyMyObjectBuilder_StarSystem();

            conf.SystemObjects = Objects;

            MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(SystemGenerator));

            string xml = MyAPIGateway.Utilities.SerializeToXML(conf);
            using(var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(STORAGE_FILE, typeof(SystemGenerator)))
            {
                writer.Write(xml);
                writer.Close();
            }
        }

        /// <summary>
        /// Adds an asteroid belt gps to the world
        /// </summary>
        private void AddBeltsGpss()
        {
            foreach (var obj in Objects)
            {
                if (obj.Type != LegacySystemObjectType.BELT) continue;

                Vector3D pos = new Vector3D(((MySystemBeltItem)obj).Radius + ((MySystemBeltItem)obj).Width / 2, 0, 0); ;

                GlobalGpsManager.Static.AddGps(obj.DisplayName, Color.Aqua, pos);
            }
        }

        /// <summary>
        /// Converts an integer to roman numerals
        /// </summary>
        /// <param name="number">Number to convert</param>
        /// <returns>The converted number in roman numerals.</returns>
        private string ConvertNumberToRoman(int number)
        {
            if (number < 0) return "";
            if (number >= 1000) return "M" + ConvertNumberToRoman(number - 1000);
            if (number >= 900) return "CM" + ConvertNumberToRoman(number - 900);
            if (number >= 500) return "D" + ConvertNumberToRoman(number - 500);
            if (number >= 400) return "CD" + ConvertNumberToRoman(number - 400);
            if (number >= 100) return "C" + ConvertNumberToRoman(number - 100);
            if (number >= 90) return "XC" + ConvertNumberToRoman(number - 90);
            if (number >= 50) return "L" + ConvertNumberToRoman(number - 50);
            if (number >= 40) return "XL" + ConvertNumberToRoman(number - 40);
            if (number >= 10) return "X" + ConvertNumberToRoman(number - 10);
            if (number >= 9) return "IX" + ConvertNumberToRoman(number - 9);
            if (number >= 5) return "V" + ConvertNumberToRoman(number - 5);
            if (number >= 4) return "IV" + ConvertNumberToRoman(number - 4);
            if (number >= 1) return "I" + ConvertNumberToRoman(number - 1);
            return "";
        }
    }
}
