using Sandbox.Definitions;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 600)]
    [EventOwner]
    public partial class SystemGenerator : MySessionComponentBase
    {
        private string[] greek_letters = new string[] {"Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "My", "Ny", "Xi", "Omikron", "Pi", "Rho", "Sigma", "Tau", "Ypsilon", "Phi", "Chi", "Psi", "Omega"};

        private const string STORAGE_FILE = "SystemData.xml";

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

        private int m_seed;
        private GeneratorSettings m_settings;

        public static SystemGenerator Static;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            InitNet();

            if (!Sync.IsServer || !SettingsSession.Static.Settings.Enable || MySession.Static.Settings.WorldSizeKm != 0) return;

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
            Static = this;

            LoadNet();

            m_planetDefinitions = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions().ToList();
            m_mandatoryPlanets = new List<MyPlanetGeneratorDefinition>();
            m_moonDefinitions = new List<MyPlanetGeneratorDefinition>();
            FilterDefinitions();
        }

        public override void SaveData()
        {
            if (!Sync.IsServer || !SettingsSession.Static.Settings.Enable) return;

            SaveConfig();
        }

        protected override void UnloadData()
        {
            UnloadNet();
            m_objects?.Clear();
            m_planetDefinitions?.Clear();
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

                    int distToPrev = MyRandom.Instance.Next(m_settings.MinOrbitDistance, m_settings.MaxOrbitDistance);
                    tmp_distance += distToPrev;

                    if(MyRandom.Instance.NextDouble()/* * ((i % 6) * (i % 6) / 12.5)*/ < 1 - m_settings.BeltSettings.BeltProbability && m_planetDefinitions.Count != 0){
                        GeneratePlanet(i, tmp_distance, numberPlanets, ref totalPlanets);
                    }
                    else
                    {
                        GenerateBelt(tmp_distance, ref totalBelts);
                    }
                }

                if(m_mandatoryPlanets.Count != 0)
                {
                    int length = m_mandatoryPlanets.Count;
                    for (int i = totalPlanets; i < length; i++)
                    {
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

            belt.DisplayName = "Belt " + greek_letters[beltIndex++ % greek_letters.Length];
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

            var def = GetPlanetDefinition((int)(m_settings.PlanetSettings.PlanetSizeCap * Math.Sin(index * Math.PI / totalObjects)));

            var angle = MyRandom.Instance.GetRandomFloat(0, (float)(2 * Math.PI));
            var height = MyRandom.Instance.GetRandomFloat((float)Math.PI / 180 * -5, (float)Math.PI / 180 * 5);
            Vector3D pos = new Vector3D(distance * Math.Sin(angle), distance * Math.Cos(angle), distance * Math.Sin(height));

            planet.DisplayName = "Planet " + (++planetIndex);
            planet.Type = SystemObjectType.PLANET;
            planet.DefName = def.Id.SubtypeId.String;
            planet.Size = SizeByGravity(def.SurfaceGravity);
            planet.PlanetRing = GenerateRing(def.SurfaceGravity, planet.Size);
            planet.OffsetPosition = pos;
            planet.CenterPosition = pos;
            planet.PlanetMoons = GenerateMoons(planet.Size, def.SurfaceGravity, planet.DisplayName);
            planet.Generated = false;

            m_objects.Add(planet);
        }

        private MyPlanetMoonItem[] GenerateMoons(float planetSize, float surfaceGravity, string planetName)
        {
            if (MyRandom.Instance.NextFloat() > m_settings.PlanetSettings.MoonProbability) return new MyPlanetMoonItem[0];

            int numMoons = MyRandom.Instance.Next(0, GetMaxMoonCount(surfaceGravity));
            MyPlanetMoonItem[] moons = new MyPlanetMoonItem[numMoons];

            for(int i = 0; i < numMoons; i++)
            {
                var dist = planetSize * (i + 1) + planetSize / 2 * MyRandom.Instance.GetRandomFloat(0.5f, 1.5f);
                var def = GetPlanetMoonDefinition(planetSize * 0.8f);

                MyPlanetMoonItem item = new MyPlanetMoonItem();
                item.Type = SystemObjectType.MOON;
                item.DefName = def.Id.SubtypeName.ToString();
                item.Distance = dist;
                item.Size = SizeByGravity(def.SurfaceGravity);
                item.DisplayName = planetName + " " + (char)('A' + i);

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

        private MyPlanetGeneratorDefinition GetPlanetDefinition(float maximumSize)
        {
            int tries = 0;
            float size;
            MyPlanetGeneratorDefinition def;

            if(m_mandatoryPlanets.Count != 0)
            {
                def = m_mandatoryPlanets[0];
                m_mandatoryPlanets.RemoveAt(0);
                return def;
            }

            do
            {
                def = m_planetDefinitions[MyRandom.Instance.Next(0, m_planetDefinitions.Count - 1)];
                size = SizeByGravity(def.SurfaceGravity);
                tries++;

            } while (size >= maximumSize && tries < 10000);

            return def;
        }

        private MyPlanetGeneratorDefinition GetPlanetMoonDefinition(float maximumSize)
        {
            int tries = 0;
            float size;
            MyPlanetGeneratorDefinition def;

            if (m_moonDefinitions.Count == 0) return GetPlanetDefinition(maximumSize);

            do
            {
                def = m_moonDefinitions[MyRandom.Instance.Next(0, (m_moonDefinitions.Count - 1) * 2) % m_moonDefinitions.Count];

                size = SizeByGravity(def.SurfaceGravity);
                tries++;

            } while (size >= maximumSize && tries < 10000);

            return def;
        }

        private float SizeByGravity(float gravity)
        {
           return (float)Math.Min(Math.Sqrt(gravity * 120000 * 120000 * m_settings.PlanetSettings.SizeMultiplier * m_settings.PlanetSettings.SizeMultiplier), m_settings.PlanetSettings.PlanetSizeCap);
        }

        private int GetMaxMoonCount(float gravity)
        {
            return (int)Math.Floor(gravity * 5 * MyRandom.Instance.NextFloat());
        }

        private void ShuffleMandatoryPlanets()
        {
            int n = m_mandatoryPlanets.Count;
            while(n > 1)
            {
                n--;
                int index = MyRandom.Instance.Next(n - 1);
                var value = m_mandatoryPlanets[index];
                m_mandatoryPlanets[index] = m_mandatoryPlanets[n];
                m_mandatoryPlanets[n] = value;
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
                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.BlacklistedPlanets.Contains(p.Id.SubtypeId.String))
                {
                    toRemovePlanets.Add(p);
                }
                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.Moons.Contains(p.Id.SubtypeId.String))
                {
                    toRemovePlanets.Add(p);
                    m_moonDefinitions.Add(p);
                }
                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.MandatoryPlanets.Contains(p.Id.SubtypeId.String) || SettingsSession.Static.Settings.GeneratorSettings.SemiRandomizedGeneration)
                {
                    m_mandatoryPlanets.Add(p);
                }
            }

            foreach(var r in toRemovePlanets)
            {
                m_planetDefinitions.Remove(r);
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
                    MyLog.Default.Error("Couldnt load Starsystem save file.");
                    MyLog.Default.Error(e.Message + "\n" + e.StackTrace);
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
