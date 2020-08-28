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
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 600)]
    [EventOwner]
    public partial class SystemGenerator : MySessionComponentBase
    {
        private string[] greek_letters = new string[] {"Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "My", "Ny", "Xi", "Omikron", "Pi", "Rho", "Sigma", "Tau", "Ypsilon", "Phi", "Chi", "Psi", "Omega"};

        private const string STORAGE_FILE = "SystemData.xml";

        private List<string> vanilla_planets = new List<string> { "EarthLike", "Mars", "Triton", "Alien", "Europa", "Titan", "Moon" };

        public HashSet<MySystemItem> m_objects
        {
            get;
            private set;
        }

        public List<MyPlanetGeneratorDefinition> m_planetDefinitions
        {
            get;
            private set;
        }

        public List<MyPlanetGeneratorDefinition> m_moonDefinitions
        {
            get;
            private set;
        }

        public List<MyPlanetGeneratorDefinition> m_mandatoryPlanets
        {
            get;
            private set;
        }

        public List<MyPlanetGeneratorDefinition> m_gasGiants
        {
            get;
            private set;
        }

        public List<MyPlanetGeneratorDefinition> m_availablePlanets
        {
            get;
            private set;
        }


        public List<MyPlanetGeneratorDefinition> m_availableMoons
        {
            get;
            private set;
        }

        private int m_seed;
        private GeneratorSettings m_settings;

        public static SystemGenerator Static;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            PluginLog.Log("Initializing system generator");

            InitNet();

            if (!Sync.IsServer || !SettingsSession.Static.Settings.Enable || MySession.Static.Settings.WorldSizeKm > 0) return;

            MyObjectBuilder_StarSystem b = GetConfig();
            m_objects = b.SystemObjects;

            m_seed = MySession.Static.Settings.ProceduralSeed + MyRandom.Instance.CreateRandomSeed();

            m_settings = SettingsSession.Static.Settings.GeneratorSettings;
            //FilterDefinitions();

            if (b == null || m_objects == null || m_objects.Count == 0)
            {
                GenerateSystem();
            }

            MySession.Static.OnReady += delegate
            {
                if(SettingsSession.Static.Settings.GeneratorSettings.BeltSettings.ShowBeltGPS)
                    AddBeltsGpss();
            };
        } 

        public override void LoadData()
        {
            PluginLog.Log("Loading definitions and network data");

            Static = this;

            LoadNet();

            m_planetDefinitions = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions().ToList();
            m_mandatoryPlanets = new List<MyPlanetGeneratorDefinition>();
            m_moonDefinitions = new List<MyPlanetGeneratorDefinition>();
            m_gasGiants = new List<MyPlanetGeneratorDefinition>();
            m_availableMoons = new List<MyPlanetGeneratorDefinition>();
            m_availablePlanets = new List<MyPlanetGeneratorDefinition>();
            FilterDefinitions();
        }

        public override void SaveData()
        {
            if (!Sync.IsServer || !SettingsSession.Static.Settings.Enable) return;

            PluginLog.Log("Saving system data");

            SaveConfig();
        }

        protected override void UnloadData()
        {
            PluginLog.Log("Unloading system generator data");

            UnloadNet();
            m_objects?.Clear();
            m_planetDefinitions?.Clear();
            m_moonDefinitions?.Clear();
            m_mandatoryPlanets?.Clear();
            m_gasGiants?.Clear();
            m_settings = null;
            Static = null;
        }

        private bool TryGetObject(string name, out MySystemItem obj)
        {
            foreach(var o in m_objects)
            {
                if (o.DisplayName.Equals(name))
                {
                    obj = o;
                    return true;
                }
            }
            obj = null;
            return false;
        }

        private void GenerateSystem()
        {
            PluginLog.Log("Generating new solar system");

            m_objects = new HashSet<MySystemItem>();

            using (MyRandom.Instance.PushSeed(m_seed))
            {
                int numberPlanets = MyRandom.Instance.Next(m_settings.MinObjectsInSystem, m_settings.MaxObjectsInSystem);
                long tmp_distance = 0;
                int totalBelts = 0;
                int totalPlanets = 0;

                for (int i = 0; i < numberPlanets; i++)
                {
                    if (tmp_distance >= m_settings.WorldSize && m_settings.WorldSize > 0) return;

                    if (i != 0 || !m_settings.FirstPlanetCenter)
                    {
                        int distToPrev = MyRandom.Instance.Next(m_settings.MinOrbitDistance, m_settings.MaxOrbitDistance);
                        tmp_distance += distToPrev;
                    }


                    if(MyRandom.Instance.NextDouble()/* * ((i % 6) * (i % 6) / 12.5)*/ < 1 - m_settings.BeltSettings.BeltProbability && m_planetDefinitions.Count > 0){
                        GeneratePlanet(i, tmp_distance, numberPlanets, ref totalPlanets);
                    }
                    else
                    {
                        GenerateBelt(tmp_distance, ref totalBelts);
                    }
                }

                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetsOnlyOnce) return;

                if(m_mandatoryPlanets.Count != 0)
                {
                    int length = m_mandatoryPlanets.Count;
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

        private void GenerateBelt(long distance, ref int beltIndex)
        {
            MySystemBeltItem belt = new MySystemBeltItem();

            string name = SettingsSession.Static.Settings.GeneratorSettings.BeltSettings.BeltNameFormat
                .SetProperty("ObjectNumber", beltIndex + 1)
                .SetProperty("ObjectNumberGreek", greek_letters[beltIndex])
                .SetProperty("ObjectLetterLower", (char)('a' + (beltIndex % 26)))
                .SetProperty("ObjectLetterUpper", (char)('A' + (beltIndex % 26)));

            ++beltIndex;

            belt.DisplayName = name;
            belt.Type = SystemObjectType.BELT;
            belt.Height = MyRandom.Instance.Next(m_settings.BeltSettings.MinBeltHeight, m_settings.BeltSettings.MaxBeltHeight);
            belt.Radius = distance;
            belt.Width = MyRandom.Instance.Next(belt.Height * 10, belt.Height * 100);
            belt.RoidSize = MyRandom.Instance.Next(256, 512);

            m_objects.Add(belt);
        }

        private void GeneratePlanet(int index, long distance, int totalObjects, ref int planetIndex)
        {
            MyPlanetItem planet = new MyPlanetItem();

            double mod = distance == 0 && m_settings.FirstPlanetCenter ? 1 : Math.Sin(index * Math.PI / totalObjects);

            var def = GetPlanetDefinition((int)(m_settings.PlanetSettings.PlanetSizeCap * mod));
            bool isGasGiant = m_gasGiants.Contains(def);
            var size = SizeByGravity(def.SurfaceGravity, isGasGiant);

            var angle = MyRandom.Instance.GetRandomFloat(0, (float)(2 * Math.PI));
            var height = MyRandom.Instance.GetRandomFloat((float)Math.PI / 180 * -5, (float)Math.PI / 180 * 5);
            Vector3D pos = new Vector3D(distance * Math.Sin(angle), distance * Math.Cos(angle), distance * Math.Sin(height));

            string name = SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.PlanetNameFormat
                .SetProperty("ObjectNumber", planetIndex + 1)
                .SetProperty("ObjectNumberGreek", greek_letters[planetIndex])
                .SetProperty("ObjectLetterLower", (char)('a' + (planetIndex % 26)))
                .SetProperty("ObjectLetterUpper", (char)('A' + (planetIndex % 26)))
                .SetProperty("ObjectId", def.Id.SubtypeId.String);

            ++planetIndex;

            planet.DisplayName = name;
            planet.Type = SystemObjectType.PLANET;
            planet.DefName = def.Id.SubtypeId.String;
            planet.Size = size;
            planet.PlanetRing = GenerateRing(def.SurfaceGravity, planet.Size);
            planet.OffsetPosition = pos;
            planet.CenterPosition = pos;
            planet.PlanetMoons = GenerateMoons(planet.Size, def.SurfaceGravity, planet.DisplayName, distance);
            planet.Generated = false;

            m_objects.Add(planet);
        }

        private MyPlanetMoonItem[] GenerateMoons(float planetSize, float surfaceGravity, string planetName, long distance)
        {
            if (MyRandom.Instance.NextFloat() > m_settings.PlanetSettings.MoonProbability) return new MyPlanetMoonItem[0];

            int maxMoons = GetMaxMoonCount(surfaceGravity);
            int numMoons = MyRandom.Instance.Next(maxMoons > 0 ? 1 : 0, maxMoons);
            MyPlanetMoonItem[] moons = new MyPlanetMoonItem[numMoons];

            for(int i = 0; i < numMoons; i++)
            {
                var dist = planetSize * (i + 1) + planetSize * MyRandom.Instance.GetRandomFloat(0.5f, 1.5f);
                var def = GetPlanetMoonDefinition(planetSize * 0.8f);
                bool isGasGiant = m_gasGiants.Contains(def);

                if (dist + distance > m_settings.WorldSize && m_settings.WorldSize > 0) return moons;

                string name = SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.MoonNameFormat
                .SetProperty("ObjectNumber", i + 1)
                .SetProperty("ObjectNumberGreek", greek_letters[i])
                .SetProperty("ObjectLetterLower", (char)('a' + (i % 26)))
                .SetProperty("ObjectLetterUpper", (char)('A' + (i % 26)))
                .SetProperty("ObjectId", def.Id.SubtypeId.String)
                .SetProperty("MoonPlanetName", planetName);

                MyPlanetMoonItem item = new MyPlanetMoonItem();
                item.Type = SystemObjectType.MOON;
                item.DefName = def.Id.SubtypeName.ToString();
                item.Distance = dist;
                item.Size = SizeByGravity(def.SurfaceGravity, isGasGiant);
                item.DisplayName = name;

                moons[i] = item;
            }

            return moons;
        }

        private MyPlanetRingItem GenerateRing(float surfaceGravity, float planetSize)
        {
            if (MyRandom.Instance.NextFloat() > m_settings.PlanetSettings.RingSettings.PlanetRingProbability * surfaceGravity) return null;

            MyPlanetRingItem ring = new MyPlanetRingItem();

            ring.Type = SystemObjectType.RING;
            ring.RoidSize = MyRandom.Instance.Next(64, Math.Min((int)(Math.Max(surfaceGravity * 0.5 * 128, 64)), 512));
            ring.Width = MyRandom.Instance.Next(m_settings.PlanetSettings.RingSettings.MinPlanetRingWidth, m_settings.PlanetSettings.RingSettings.MaxPlanetRingWidth);
            ring.Height = MyRandom.Instance.Next(m_settings.PlanetSettings.RingSettings.MinPlanetRingWidth / 10, ring.Width / 10);
            ring.AngleDegrees = MyRandom.Instance.Next(-20, 20);
            ring.Radius = MyRandom.Instance.Next((int)(planetSize * 0.75), (int)(planetSize * 2));
            ring.DisplayName = "";

            return ring;
        }

        bool m_usedAllPlanets = false;

        private MyPlanetGeneratorDefinition GetPlanetDefinition(float maximumSize, bool ignoreMandatory = false)
        {
            int tries = 0;
            float size;
            MyPlanetGeneratorDefinition def;

            if(m_mandatoryPlanets.Count != 0 && !ignoreMandatory)
            {
                def = m_mandatoryPlanets[0];
                m_mandatoryPlanets.RemoveAt(0);
            }
            else
            {
                do
                {
                    def = m_availablePlanets[MyRandom.Instance.Next(0, m_availablePlanets.Count - 1)];
                    size = SizeByGravity(def.SurfaceGravity);
                    tries++;

                } while (size > maximumSize && tries < 10000);
            }
            

            if (SettingsSession.Static.Settings.GeneratorSettings.PlanetsOnlyOnce && !m_usedAllPlanets)
            {
                m_availablePlanets.Remove(def);
                if(m_availablePlanets.Count <= 0)
                {
                    m_availablePlanets = m_planetDefinitions;
                    m_usedAllPlanets = true;
                }
            }

            return def;
        }

        bool m_usedAllMoons = false;

        private MyPlanetGeneratorDefinition GetPlanetMoonDefinition(float maximumSize)
        {
            int tries = 0;
            float size;
            MyPlanetGeneratorDefinition def;

            if (m_moonDefinitions.Count == 0) return GetPlanetDefinition(maximumSize, true);

            do
            {
                def = m_availableMoons[MyRandom.Instance.Next(0, (m_availableMoons.Count - 1) * 2) % m_availableMoons.Count];

                size = SizeByGravity(def.SurfaceGravity);
                tries++;

            } while (size >= maximumSize && tries < 10000);

            if (SettingsSession.Static.Settings.GeneratorSettings.MoonsOnlyOnce && !m_usedAllMoons)
            {
                m_availableMoons.Remove(def);
                if (m_availableMoons.Count <= 0)
                {
                    m_availableMoons = m_moonDefinitions;
                    m_usedAllMoons = true;
                }
            }

            return def;
        }

        private float SizeByGravity(float gravity, bool isGasGiant = false)
        {
            float multiplier = isGasGiant ? m_settings.PlanetSettings.SizeMultiplier * 2.0f : m_settings.PlanetSettings.SizeMultiplier;

           return (float)Math.Min(Math.Sqrt(gravity * 120000 * 120000 * multiplier * multiplier), m_settings.PlanetSettings.PlanetSizeCap);
        }

        private int GetMaxMoonCount(float gravity, bool isGasGiant = false)
        {
            float m = isGasGiant ? 2 : 1;
            float o = isGasGiant ? 0.3f : 0;
            return (int)Math.Floor(gravity * m * 5 * (MyRandom.Instance.NextFloat() + o));
        }

        private void ShuffleMandatoryPlanets()
        {
            int n = m_mandatoryPlanets.Count - 1;
            if (n <= 0) return;
            while(n > 1)
            {
                int index = MyRandom.Instance.Next(1, n - 1);
                var value = m_mandatoryPlanets[index];
                m_mandatoryPlanets[index] = m_mandatoryPlanets[n];
                m_mandatoryPlanets[n] = value;
                n--;
            }
        }

        private void FilterDefinitions()
        {
            List<MyPlanetGeneratorDefinition> toRemovePlanets = new List<MyPlanetGeneratorDefinition>();
            foreach (var p in m_planetDefinitions)
            {
                if (p.Id.SubtypeId.String.Contains("Tutorial") || p.Id.SubtypeId.String.Contains("TestMap") || p.Id.SubtypeId.String.Contains("ModExample"))
                {
                    toRemovePlanets.Add(p);
                    continue;
                }
                if (!SettingsSession.Static.Settings.GeneratorSettings.UseVanillaPlanets && vanilla_planets.Contains(p.Id.SubtypeId.String))
                {
                    toRemovePlanets.Add(p);
                    continue;
                }
                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.BlacklistedPlanets.Contains(p.Id.SubtypeId.String))
                {
                    toRemovePlanets.Add(p);
                }
                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.Moons.Contains(p.Id.SubtypeId.String))
                {
                    toRemovePlanets.Add(p);
                    m_moonDefinitions.Add(p);
                    m_availableMoons.Add(p);
                }
                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.MandatoryPlanets.Contains(p.Id.SubtypeId.String) || SettingsSession.Static.Settings.GeneratorSettings.SemiRandomizedGeneration)
                {
                    m_mandatoryPlanets.Add(p);
                }
                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.GasGiants.Contains(p.Id.SubtypeId.String))
                {
                    m_gasGiants.Add(p);
                }
            }

            foreach(var r in toRemovePlanets)
            {
                m_planetDefinitions.Remove(r);
            }

            foreach(var p in m_planetDefinitions)
            {
                m_availablePlanets.Add(p);
            }

            ShuffleMandatoryPlanets();
        }

        private MyObjectBuilder_StarSystem GetConfig()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(STORAGE_FILE, typeof(SystemGenerator)))
            {
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(STORAGE_FILE, typeof(SystemGenerator)))
                    {
                        MyObjectBuilder_StarSystem saveFile = MyAPIGateway.Utilities.SerializeFromXML<MyObjectBuilder_StarSystem>(reader.ReadToEnd());
                        return saveFile;
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Log("Couldnt load Starsystem save file.", LogLevel.ERROR);
                    PluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);

                    MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(SystemGenerator));
                    return null;
                }
            }
            else
            {
                return new MyObjectBuilder_StarSystem();
            }
        }

        private void SaveConfig()
        {
            MyObjectBuilder_StarSystem conf = new MyObjectBuilder_StarSystem();

            conf.SystemObjects = m_objects;

            MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(SystemGenerator));

            string xml = MyAPIGateway.Utilities.SerializeToXML(conf);
            using(var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(STORAGE_FILE, typeof(SystemGenerator)))
            {
                writer.Write(xml);
                writer.Close();
            }
        }

        private void AddBeltsGpss()
        {
            foreach (var obj in m_objects)
            {
                if (obj.Type != SystemObjectType.BELT) continue;

                Vector3D pos = new Vector3D(((MySystemBeltItem)obj).Radius + ((MySystemBeltItem)obj).Width / 2, 0, 0); ;

                GlobalGpsManager.Static.AddGps(obj.DisplayName, Color.Aqua, pos);
            }
        }
    }
}
