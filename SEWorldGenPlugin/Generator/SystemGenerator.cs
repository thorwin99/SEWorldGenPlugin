using Sandbox.Definitions;
using Sandbox.Game.World;
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
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 600, typeof(MyObjectBuilder_StarSystem))]
    public class SystemGenerator : MySessionComponentBase
    {
        private const int MAX_PLANETS_COUNT = 25;
        private const int MIN_PLANETS_COUNT = 5;
        private const int MIN_PLANET_DISTANCE = 8000000;
        private const int MAX_PLANET_DISTANCE = 20000000;
        private const int MAX_PLANET_SIZE = 1200000;
        private const int MIN_RING_WIDTH = 10000;
        private const int MAX_RING_WIDTH = 100000;
        private const int MIN_RING_HEIGHT = 1000;

        public HashSet<MyPlanetItem> m_planets
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
            MyObjectBuilder_StarSystem builder = (MyObjectBuilder_StarSystem)base.GetObjectBuilder();

            builder.SystemPlanets = new HashSet<MyPlanetItem>(m_planets);

            return builder;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            MyObjectBuilder_StarSystem b = (MyObjectBuilder_StarSystem)sessionComponent;
            m_planets = b.SystemPlanets;

            if (m_planets == null || m_planets.Count == 0)
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
            base.SaveData();
        }

        protected override void UnloadData()
        {
            base.UnloadData();
        }

        private void GenerateSystem()
        {
            m_planets = new HashSet<MyPlanetItem>();

            using (MyRandom.Instance.PushSeed(m_seed))
            {
                int numberPlanets = MyRandom.Instance.Next(MIN_PLANETS_COUNT, MAX_PLANETS_COUNT);
                long tmp_distance = 0;

                for (int i = 0; i < numberPlanets; i++)
                {
                    int distToPrev = MyRandom.Instance.Next(MIN_PLANET_DISTANCE, MAX_PLANET_DISTANCE);
                    tmp_distance += distToPrev;
                    GeneratePlanet(i, tmp_distance);
                }
            }
        }

        private void GeneratePlanet(int maxSize, long distance)
        {
            MyPlanetItem planet = new MyPlanetItem();

            var def = GetPlanetDefinition(maxSize);

            var angle = MyRandom.Instance.GetRandomFloat(0, (float)(2 * Math.PI));
            var height = MyRandom.Instance.GetRandomFloat((float)Math.PI / 180 * -20, (float)Math.PI / 180 * 20);
            Vector3D pos = new Vector3D(distance * Math.Sin(angle), distance * Math.Cos(angle), distance * Math.Tan(height));

            planet.DefName = def.Id.SubtypeId.String;
            planet.Size = SizeByGravity(def.SurfaceGravity);
            planet.PlanetRing = GenerateRing(def.SurfaceGravity, planet.Size);
            planet.OffsetPosition = pos;
            planet.CenterPosition = Vector3D.Zero;
            planet.PlanetMoons = GenerateMoons(planet.Size, def.SurfaceGravity);
            planet.Generated = false;

            m_planets.Add(planet);
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
    }
}
