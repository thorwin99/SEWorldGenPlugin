using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Library.Utils;
using VRage.Profiler;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 600)]
    public class SystemGenerator : MySessionComponentBase
    {
        private const int MAX_PLANETS_COUNT = 25;
        private const int MIN_PLANETS_COUNT = 5;
        private const int MIN_PLANET_DISTANCE = 4000000;
        private const int MAX_PLANET_DISTANCE = 10000000;
        private const int MAX_PLANET_SIZE = 1200000;
        private const int MIN_RING_WIDTH = 10000;
        private const int MAX_RING_WIDTH = 100000;
        private const int MIN_RING_HEIGHT = 1000;

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

        private int m_seed;

        public static SystemGenerator Static;

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            return base.GetObjectBuilder();
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (!Sync.IsServer) return;

            MyObjectBuilder_StarSystem b = GetConfig();
            m_objects = b.SystemObjects;

            m_seed = MySession.Static.Settings.ProceduralSeed;

            if (b == null || m_objects == null || m_objects.Count == 0)
            {
                GenerateSystem();
            }
        } 

        public override void LoadData()
        {
            Static = this;

            m_planetDefinitions = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions().ToList();
            FilterDefinitions();

            m_seed = MySession.Static.Settings.ProceduralSeed;
        }

        public override void SaveData()
        {
            if (!Sync.IsServer) return;

            SaveConfig();
        }

        protected override void UnloadData()
        {
            m_objects = new HashSet<MySystemItem>();
        }

        private void GenerateSystem()
        {
            m_objects = new HashSet<MySystemItem>();

            using (MyRandom.Instance.PushSeed(m_seed))
            {
                int numberPlanets = MyRandom.Instance.Next(MIN_PLANETS_COUNT, MAX_PLANETS_COUNT);
                long tmp_distance = 0;

                for (int i = 0; i < numberPlanets; i++)
                {
                    int distToPrev = MyRandom.Instance.Next(MIN_PLANET_DISTANCE, MAX_PLANET_DISTANCE);
                    tmp_distance += distToPrev;

                    if(MyRandom.Instance.NextDouble() * ((i % 6) * (i & 6) / 12.5) < 0.5){
                        GeneratePlanet(i, tmp_distance);
                    }
                    else
                    {
                        GenerateBelt(tmp_distance);
                    }
                }
            }
        }

        private void GenerateBelt(long distance)
        {
            MySystemBeltItem belt = new MySystemBeltItem();

            belt.Type = SystemObjectType.BELT;
            belt.Height = MyRandom.Instance.Next(4000, 40000);
            belt.Radius = distance;
            belt.Width = MyRandom.Instance.Next(belt.Height * 10, belt.Height * 100);
            belt.RoidSize = MyRandom.Instance.Next(256, 512);

            m_objects.Add(belt);
        }

        private void GeneratePlanet(int maxSize, long distance)
        {
            MyPlanetItem planet = new MyPlanetItem();

            var def = GetPlanetDefinition(maxSize);

            var angle = MyRandom.Instance.GetRandomFloat(0, (float)(2 * Math.PI));
            var height = MyRandom.Instance.GetRandomFloat((float)Math.PI / 180 * -5, (float)Math.PI / 180 * 5);
            Vector3D pos = new Vector3D(distance * Math.Sin(angle), distance * Math.Cos(angle), distance * Math.Sin(height));

            planet.Type = SystemObjectType.PLANET;
            planet.DefName = def.Id.SubtypeId.String;
            planet.Size = SizeByGravity(def.SurfaceGravity);
            planet.PlanetRing = GenerateRing(def.SurfaceGravity, planet.Size);
            planet.OffsetPosition = pos;
            planet.CenterPosition = Vector3D.Zero;
            planet.PlanetMoons = GenerateMoons(planet.Size, def.SurfaceGravity);
            planet.Generated = false;

            m_objects.Add(planet);
        }

        private MyPlanetMoonItem[] GenerateMoons(float planetSize, float surfaceGravity)
        {
            int numMoons = MyRandom.Instance.Next(0, GetMaxMoonCount(surfaceGravity));
            MyPlanetMoonItem[] moons = new MyPlanetMoonItem[numMoons];

            for(int i = 0; i < numMoons; i++)
            {
                var dist = planetSize * (i + 1) + planetSize / 2 * MyRandom.Instance.GetRandomFloat(0.5f, 1.5f);
                var def = GetPlanetDefinition(planetSize * 0.8f);

                MyPlanetMoonItem item = new MyPlanetMoonItem();
                item.Type = SystemObjectType.MOON;
                item.DefName = def.Id.SubtypeName.ToString();
                item.Distance = dist;
                item.Size = SizeByGravity(def.SurfaceGravity);

                moons[i] = item;
            }

            return moons;
        }

        private MyPlanetRingItem GenerateRing(float surfaceGravity, float planetSize)
        {
            if (MyRandom.Instance.NextFloat() / surfaceGravity > 0.3 && false) return null;

            MyPlanetRingItem ring = new MyPlanetRingItem();

            ring.Type = SystemObjectType.RING;
            ring.RoidSize = MyRandom.Instance.Next(64, Math.Min((int)(Math.Max(surfaceGravity * 0.5 * 128, 64)), 512));
            ring.Width = MyRandom.Instance.Next(MIN_RING_WIDTH, MAX_RING_WIDTH);
            ring.Height = MyRandom.Instance.Next(MIN_RING_HEIGHT, ring.Width / 10);
            ring.AngleDegrees = MyRandom.Instance.Next(-180, 180);
            ring.Radius = MyRandom.Instance.Next((int)(planetSize * 0.5 * 0.75), (int)(planetSize));

            return ring;
        }

        private MyPlanetGeneratorDefinition GetPlanetDefinition(float maximumSize)
        {
            int tries = 0;
            int size;
            MyPlanetGeneratorDefinition def = null;

            do
            {
                def = m_planetDefinitions[MyRandom.Instance.Next(0, m_planetDefinitions.Count - 1)];
                size = SizeByGravity(def.SurfaceGravity);
                tries++;

            } while (size >= maximumSize && tries < 10000);

            return def;
        }

        private int SizeByGravity(float gravity)
        {
            return (int)Math.Min(Math.Sqrt(gravity * 240000 * 240000), MAX_PLANET_SIZE);
        }

        private int GetMaxMoonCount(float gravity)
        {
            return (int)Math.Floor(gravity * 5);
        }

        private void FilterDefinitions()
        {
            List<MyPlanetGeneratorDefinition> toRemove = new List<MyPlanetGeneratorDefinition>();
            foreach (var p in m_planetDefinitions)
            {
                if (p.Id.SubtypeId.String.Contains("Tutorial") || p.Id.SubtypeId.String.Contains("TestMap") || p.Id.SubtypeId.String.Contains("ModExample"))
                {
                    toRemove.Add(p);
                }
            }

            foreach(var r in toRemove)
            {
                m_planetDefinitions.Remove(r);
            }
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
    }
}
