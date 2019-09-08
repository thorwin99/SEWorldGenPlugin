using Sandbox.Definitions;
using Sandbox.Engine.Utils;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SEWorldGenPlugin.SaveItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.Game.Voxels;
using VRage.Library.Utils;
using VRage.ObjectBuilders;
using VRage.Serialization;
using VRage.Utils;
using VRage.Voxels;
using VRageMath;
using VRageRender.Messages;
using Sandbox.Game.World.Generator;
using Sandbox.Game.World;
using SEWorldGenPlugin.Generator.Asteroids;

namespace SEWorldGenPlugin.Generator
{
    class StarSystemGenerator
    {
        private const int MAX_PLANETS = 25;
        private const int MIN_PLANETS = 5;
        private const int MAX_PLANET_SIZE = 1200000;
        private const int MIN_PLANET_SIZE = 10000;
        private const int MIN_PLANET_DISTANCE = 8000000;
        private const int MAX_PLANET_DISTANCE = 20000000;
        private const int MIN_RING_WIDTH = 10000;
        private const int MAX_RING_WIDTH = 100000;
        private const int MIN_RING_HEIGHT = 1000;

        public List<MyPlanetGeneratorDefinition> Planets { private set; get; }
        public ObjectBuilder_GeneratorSave SaveData { set; get; }
        public StarSystemGenerator(List<MyPlanetGeneratorDefinition> planets)
        {
            this.Planets = planets;
            SaveData = null;
        }

        public void GeneratePossiblePlanets(Vector3D playerPos)
        {
            List<PlanetItem> planets = ((StarSystemItem)SaveData.Components[0]).Planets;

            foreach(var planet in planets)
            {
                if (planet.Generated) continue;
                if (Vector3D.Subtract(playerPos, planet.OffsetPosition).Length() > 100000000) continue;
                MyPlanetGeneratorDefinition def = GetDefByName(planet.DefName);
                if (def == null) continue;

                MyPlanet p = CreatePlanet(planet.OffsetPosition, planet.Size, ref def);

                int moonCount = planet.PlanetMoons.Length;
                planet.CenterPosition = p.PositionComp.GetPosition();
                if(planet.PlanetRing != null)
                    planet.PlanetRing.Center = planet.CenterPosition;

                //SpawnRing(planet.CenterPosition, planet.PlanetRing);

                List<Vector3D> spawnedMoonPos = new List<Vector3D>();

                for(int i = 0; i < moonCount; i++)
                {
                    PlanetMoonItem moon = planet.PlanetMoons[i];
                    MyPlanetGeneratorDefinition moonDef = GetDefByName(moon.DefName);
                    if (moonDef == null) continue;
                    var position = new Vector3D(0, 0, 0);
                    var threshold = 0;
                    do
                    {

                        double angle = MyRandom.Instance.GetRandomFloat(0, (float)Math.PI * 2f);
                        position = new Vector3D(moon.Distance * Math.Sin(angle), moon.Distance * Math.Cos(angle), moon.Distance * Math.Sin(MyRandom.Instance.GetRandomFloat((float)-Math.PI / 2, (float)Math.PI / 2)));
                        position = Vector3D.Add(planet.CenterPosition, position);
                        threshold++;

                    } while (ObstructedPlace(position, spawnedMoonPos, planet.Size, planet.PlanetRing) && threshold < 10000);
                    spawnedMoonPos.Add(CreatePlanet(position, moon.Size, ref moonDef).PositionComp.GetPosition());
                }
                planet.Generated = true;
            }
        }

        private void SpawnRing(Vector3D center, PlanetRingItem ring)
        {
            for(long i = 0; i < ring.AsteroidCount; i++)
            {
                double angle = 2 * Math.PI / 360 * ring.AngleDegrees;
                double horAngle = 2 * Math.PI * i / ring.AsteroidCount;
                Vector3D normalizedPosition = new Vector3D(Math.Cos(angle * Math.Cos(horAngle)), Math.Sin(horAngle), Math.Sin(angle * Math.Cos(horAngle)));
                Vector3D relPosition = Vector3D.Multiply(normalizedPosition, ring.Radius);
                Vector3D offsetPosition = new Vector3D(MyRandom.Instance.Next(0, ring.Width), MyRandom.Instance.Next(0, ring.Width), MyRandom.Instance.Next(-ring.Height / 2, ring.Height / 2));
                Vector3D finalPosition = Vector3D.Add(Vector3D.Multiply(normalizedPosition, offsetPosition), relPosition);
                float size = MyRandom.Instance.GetRandomFloat(ring.RoidSize * 0.5f, ring.RoidSize * 1.5f);
                CreateProceduralAsteroid(MyRandom.Instance.CreateRandomSeed(), size / 2, finalPosition);
            }
        }

        public ObjectBuilder_GeneratorSave GenerateSystem(int seed)
        {
            if (SaveData != null) return SaveData;
            MyLog.Default.WriteLine("Generating System Savedata");
            SaveData = new ObjectBuilder_GeneratorSave();
            SaveData.Components = new List<Ob_GeneratorSaveItem>();
            var starSystem = new StarSystemItem();
            using (MyRandom.Instance.PushSeed(seed))
            {
                int amountPlanets = MyRandom.Instance.Next(MIN_PLANETS, MAX_PLANETS);
                int currentDistance = 0;
                for(int i = 0; i < amountPlanets; i++)
                {
                    int dist = MyRandom.Instance.Next(MIN_PLANET_DISTANCE, MAX_PLANET_DISTANCE);
                    starSystem.Planets.Add(GeneratePlanetItem(currentDistance + dist, GetMaxSize(i, amountPlanets)));
                    currentDistance += dist;
                }
            }

            starSystem.Planets.Add(GeneratePlanetItem(0, MIN_PLANET_SIZE));
            SaveData.Components.Add(starSystem);

            return SaveData;
        }

        private PlanetItem GeneratePlanetItem(int distance, int maxSize)
        {
            PlanetItem item = new PlanetItem();

            MyPlanetGeneratorDefinition def;
            int threshold = 0;
            int size;
            do
            {
                def = Planets[MyRandom.Instance.Next(0, Planets.Count - 1)];
                size = GetSizeByGrav(def.SurfaceGravity);
                threshold++;
            } while (size > maxSize && threshold < 10000);

            var angle = MyRandom.Instance.GetRandomFloat(0, (float)(2 * Math.PI));
            var height = MyRandom.Instance.GetRandomFloat((float)Math.PI / 180 * -20, (float)Math.PI / 180 * 20);
            Vector3D pos = new Vector3D(distance * Math.Sin(angle), distance * Math.Cos(angle), distance * Math.Tan(height));
            PlanetRingItem planetRing;
            if(MyRandom.Instance.NextFloat() * def.SurfaceGravity > 0.5 || true)
            {
                planetRing = GeneratePlanetRing(size, def);
            }
            else
            {
                planetRing = new PlanetRingItem()
                {
                    AsteroidCount = 0,
                    Radius = 0,
                    Width = 0,
                    Height = 0,
                    AngleDegrees = 0,
                    RoidSize = 0
                };
            }

            List<PlanetMoonItem> moons = new List<PlanetMoonItem>();
            int numMoons = MyRandom.Instance.Next(0, GetMaxMoonNumberByGravity(def.SurfaceGravity));
            for(int i = 0; i < numMoons; i++)
            {
                moons.Add(GenerateMoonItem(size, i));
            }

            item.DefName = def.Id.SubtypeId.String;
            item.CenterPosition = new Vector3D(0, 0, 0);
            item.OffsetPosition = pos;
            item.PlanetRing = planetRing;
            item.PlanetMoons = moons.ToArray();
            item.Generated = false;
            item.Size = size;

            return item;
        }

        private PlanetRingItem GeneratePlanetRing(float planetSize, MyPlanetGeneratorDefinition def)
        {
            int roidSize = MyRandom.Instance.Next(100, Math.Min((int)(def.SurfaceGravity * 0.5 * 1000), 2000));
            PlanetRingItem ring = new PlanetRingItem();
            ring.Radius = MyRandom.Instance.Next((int)(planetSize * 0.5 * 0.75), (int)(planetSize));
            ring.Width = MyRandom.Instance.Next(MIN_RING_WIDTH, MAX_RING_WIDTH);
            ring.Height = MyRandom.Instance.Next(MIN_RING_HEIGHT, ring.Width / 10);
            ring.AngleDegrees = MyRandom.Instance.Next(-180, 180);
            ring.Density = MyRandom.Instance.NextDouble();
            ring.RoidSize = roidSize;
            ring.AsteroidCount = (double)((((ring.Radius + ring.Width) * (ring.Radius + ring.Width) * Math.PI) - (ring.Radius * ring.Radius * Math.PI)) * ring.Height / roidSize / roidSize / roidSize / 27);
            return ring;
        }

        private PlanetMoonItem GenerateMoonItem(float planetSize, int index)
        {
            var dist = planetSize * (index + 1) + planetSize / 2 * MyRandom.Instance.GetRandomFloat(0.5f, 1.5f);
            MyPlanetGeneratorDefinition def;
            var threshold = 0;
            int size;
            do
            {
                def = Planets[MyRandom.Instance.Next(0, Planets.Count - 1)];
                size = GetSizeByGrav(def.SurfaceGravity);
                threshold++;
            } while (size > planetSize * 0.8 && threshold < 10000);

            PlanetMoonItem item = new PlanetMoonItem();
            item.DefName = def.Id.SubtypeId.String;
            item.Distance = dist;
            item.Size = size;

            return item;
        }

        private int GetMaxMoonNumberByGravity(float gravity)
        {
            return (int) Math.Pow(2, gravity * 2);
        }

        private int GetSizeByGrav(float gravity)
        {
            return (int) Math.Min(Math.Sqrt(gravity * 240000 * 240000), MAX_PLANET_SIZE);
        }

        private bool ObstructedPlace(Vector3D position, List<Vector3D> other, int minDistance, PlanetRingItem ring)
        {

            foreach(var obj in other)
            {
                if (Vector3D.Subtract(position, obj).Length() < minDistance)
                    return true;
            }

            return false;
        }

        public static ObjectBuilder_GeneratorSave GetDefaultSystem()
        {
            ObjectBuilder_GeneratorSave save = new ObjectBuilder_GeneratorSave();
            save.Components = new List<Ob_GeneratorSaveItem>();

            save.Components.Add(new StarSystemItem()
            {
                Planets = new List<PlanetItem>()
            });

            return save;
        }

        private MyPlanetGeneratorDefinition GetDefByName(string name)
        {
            foreach(MyPlanetGeneratorDefinition def in Planets)
            {
                if (def.Id.SubtypeId.String.Equals(name)) return def;
            }
            return null;
        }

        private int GetMaxSize(int index, int maxAmount)
        {
            return (int)((MAX_PLANET_SIZE - MIN_PLANET_SIZE) * Math.Sin(index * Math.PI / maxAmount));
        }

        private static MyPlanet CreatePlanet(Vector3D? position, float? size, ref MyPlanetGeneratorDefinition generatorDef)
        {
            if (MyFakes.ENABLE_PLANETS == false)
            {
                MyLog.Default.Error("Planets Not Enabled, Enable them");
                return null;
            }

            var random = MyRandom.Instance;
            var seed = MyRandom.Instance.CreateRandomSeed();
            using (MyRandom.Instance.PushSeed(seed))
            {

                MyPlanetStorageProvider provider = new MyPlanetStorageProvider();
                provider.Init(seed, generatorDef, size.Value / 2f);

                IMyStorage storage = new MyOctreeStorage(provider, provider.StorageSize);
                float minHillSize = provider.Radius * generatorDef.HillParams.Min;
                float maxHillSize = provider.Radius * generatorDef.HillParams.Max;

                float averagePlanetRadius = provider.Radius;

                float outerRadius = averagePlanetRadius + maxHillSize;
                float innerRadius = averagePlanetRadius + minHillSize;

                float atmosphereRadius = generatorDef.AtmosphereSettings.HasValue && generatorDef.AtmosphereSettings.Value.Scale > 1f ? 1 + generatorDef.AtmosphereSettings.Value.Scale : 1.75f;
                atmosphereRadius *= provider.Radius;

                float redAtmosphereShift = random.NextFloat(generatorDef.HostileAtmosphereColorShift.R.Min, generatorDef.HostileAtmosphereColorShift.R.Max);
                float greenAtmosphereShift = random.NextFloat(generatorDef.HostileAtmosphereColorShift.G.Min, generatorDef.HostileAtmosphereColorShift.G.Max);
                float blueAtmosphereShift = random.NextFloat(generatorDef.HostileAtmosphereColorShift.B.Min, generatorDef.HostileAtmosphereColorShift.B.Max);

                Vector3 atmosphereWavelengths = new Vector3(0.650f + redAtmosphereShift, 0.570f + greenAtmosphereShift, 0.475f + blueAtmosphereShift);

                atmosphereWavelengths.X = MathHelper.Clamp(atmosphereWavelengths.X, 0.1f, 1.0f);
                atmosphereWavelengths.Y = MathHelper.Clamp(atmosphereWavelengths.Y, 0.1f, 1.0f);
                atmosphereWavelengths.Z = MathHelper.Clamp(atmosphereWavelengths.Z, 0.1f, 1.0f);

                var planet = new MyPlanet();
                planet.EntityId = random.NextLong();

                MyPlanetInitArguments planetInitArguments = new MyPlanetInitArguments();
                planetInitArguments.StorageName = generatorDef.Id.SubtypeId + "_" + size + "_" + planet.EntityId;
                planetInitArguments.Storage = storage;
                planetInitArguments.PositionMinCorner = position.Value;
                planetInitArguments.Radius = provider.Radius;
                planetInitArguments.AtmosphereRadius = atmosphereRadius;
                planetInitArguments.MaxRadius = outerRadius;
                planetInitArguments.MinRadius = innerRadius;
                planetInitArguments.HasAtmosphere = generatorDef.HasAtmosphere;
                planetInitArguments.AtmosphereWavelengths = atmosphereWavelengths;
                planetInitArguments.GravityFalloff = generatorDef.GravityFalloffPower;
                planetInitArguments.MarkAreaEmpty = true;
                planetInitArguments.AtmosphereSettings = generatorDef.AtmosphereSettings.HasValue ? generatorDef.AtmosphereSettings.Value : MyAtmosphereSettings.Defaults();
                planetInitArguments.SurfaceGravity = generatorDef.SurfaceGravity;
                planetInitArguments.AddGps = true;
                planetInitArguments.SpherizeWithDistance = true;
                planetInitArguments.Generator = generatorDef;
                planetInitArguments.UserCreated = false;
                planetInitArguments.InitializeComponents = true;

                planet.Init(planetInitArguments);

                Vector3 pos = planet.PositionComp.GetPosition();

                MyEntities.Add(planet);
                MyEntities.RaiseEntityCreated(planet);

                return planet;
            }
        }

        private static void CreateProceduralAsteroid(int seed, float radius, Vector3D position)
        {
            var storageNameBase = "ProcAsteroid" + "-" + seed + "r" + radius;
            var storageName = MakeStorageName(storageNameBase);

            var provider = MyCompositeShapeProvider.CreateAsteroidShape(seed, radius, MySession.Static.Settings.VoxelGeneratorVersion);
            var storage = new MyOctreeStorage(provider, GetAsteroidVoxelSize(radius));
            
            MyVoxelMap voxelMap = new MyVoxelMap();

            voxelMap.EntityId = MyRandom.Instance.NextLong();
            voxelMap.Init(storageName, storage, position - storage.Size * 0.5f);
            MyEntities.RaiseEntityCreated(voxelMap);
            MyEntities.Add(voxelMap);

            voxelMap.Save = false;
            MyVoxelBase.StorageChanged OnStorageRangeChanged = null;
            OnStorageRangeChanged = delegate (MyVoxelBase voxel, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData)
            {
                voxelMap.Save = true;
                voxelMap.RangeChanged -= OnStorageRangeChanged;
            };
            voxelMap.RangeChanged += OnStorageRangeChanged;
        }

        public static String MakeStorageName(String storageNameBase)
        {
            String storageName = storageNameBase;

            int i = 0;

            bool collision;
            do
            {
                collision = false;
                foreach (var voxelMap in MySession.Static.VoxelMaps.Instances)
                {
                    if (voxelMap.StorageName == storageName)
                    {
                        collision = true;
                        break;
                    }
                }

                if (collision)
                {
                    storageName = storageNameBase + "-" + i++;
                }
            }
            while (collision);

            return storageName;
        }

        private static Vector3I GetAsteroidVoxelSize(double asteroidRadius)
        {
            int radius = Math.Max(64, (int)Math.Ceiling(asteroidRadius));
            return new Vector3I(radius);
        }
    }
}