using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.Voxels;
using VRage.Library.Utils;
using VRage.Network;
using VRage.Profiler;
using VRage.Utils;
using VRageMath;
using VRageRender.Messages;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    public class ProceduralPlanetModule
    {
        private int m_seed;

        public ProceduralPlanetModule(int seed)
        {
            m_seed = seed;
        }

        public void GeneratePlanets(MyEntity e)
        {
            GeneratePlanets();
        }

        public void GeneratePlanets()
        {
            foreach(var obj in SystemGenerator.Static.m_objects)
            {
                if (obj.Type != SystemObjectType.PLANET) continue;

                MyPlanetItem planet = (MyPlanetItem)obj;

                if (planet.Generated) continue;

                MyPlanetGeneratorDefinition definition = GetDefinition(planet.DefName);
                if (definition == null) continue;
                long id = MyRandom.Instance.NextLong();
                MyPlanet generatedPlanet = MyWorldGenerator.AddPlanet(definition.Id.SubtypeId + "_" + planet.Size + id, planet.DisplayName, planet.DefName, planet.OffsetPosition, m_seed, planet.Size, true, id, false, false);
                planet.CenterPosition = generatedPlanet.PositionComp.GetPosition();
                generatedPlanet.DisplayNameText = planet.DisplayName;
                generatedPlanet.AsteroidName = planet.DisplayName;

                if (planet.PlanetRing != null)
                    planet.PlanetRing.Center = planet.CenterPosition;
                List<Vector3D> spawnedMoons = new List<Vector3D>();

                for(int i = 0; i < planet.PlanetMoons.Length; i++)
                {
                    MyPlanetMoonItem moon = planet.PlanetMoons[i];
                    MyPlanetGeneratorDefinition moonDef = GetDefinition(moon.DefName);
                    if (moonDef == null) continue;
                    var position = new Vector3D(0, 0, 0);
                    long mId = MyRandom.Instance.NextLong();
                    string storageNameMoon = moon.DefName + "_" + moon.Size + mId;
                    var threshold = 0;
                    do
                    {

                        double angle = MyRandom.Instance.GetRandomFloat(0, (float)Math.PI * 2f);
                        position = new Vector3D(moon.Distance * Math.Sin(angle), moon.Distance * Math.Cos(angle), moon.Distance * Math.Sin(MyRandom.Instance.GetRandomFloat((float)-Math.PI / 2, (float)Math.PI / 2)));
                        position = Vector3D.Add(planet.CenterPosition, position);
                        threshold++;

                    } while (ObstructedPlace(position, spawnedMoons, planet.Size, planet.PlanetRing) && threshold < 10000);
                    MyPlanet spawnedMoon = MyWorldGenerator.AddPlanet(storageNameMoon, moon.DisplayName, moon.DefName, position, m_seed, moon.Size, true, mId, false, true);
                    spawnedMoons.Add(spawnedMoon.PositionComp.GetPosition());

                    if(SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.ShowMoonGPS)
                        GlobalGpsManager.Static.AddGps(moon.DisplayName, Color.Aqua, spawnedMoon.PositionComp.GetPosition());
                }
                planet.Generated = true;
                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.ShowPlanetGPS)
                    GlobalGpsManager.Static.AddGps(planet.DisplayName, Color.Aqua, generatedPlanet.PositionComp.GetPosition());
            }
        }

        private MyPlanetGeneratorDefinition GetDefinition(string name)
        {
            return MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(name));
        }

        private bool ObstructedPlace(Vector3D position, List<Vector3D> other, int minDistance, MyPlanetRingItem ring)
        {

            foreach (var obj in other)
            {

                if (Vector3D.Subtract(position, obj).Length() < minDistance)
                {
                    return true;
                }
                if (ring != null)
                {
                    AsteroidRingShape shape = AsteroidRingShape.CreateFromRingItem(ring);
                    return shape.Contains(position) != ContainmentType.Disjoint;
                }
            }

            return false;
        }

        private MyPlanet CreatePlanet(Vector3D? position, float? size, ref MyPlanetGeneratorDefinition generatorDef)
        {
            if (MyFakes.ENABLE_PLANETS == false)
            {
                MyLog.Default.Error("Planets Not Enabled, Enable them");
                return null;
            }

            var random = MyRandom.Instance;
            using (random.PushSeed(random.CreateRandomSeed()))
            {
                MyPlanetStorageProvider provider = new MyPlanetStorageProvider();
                provider.Init(m_seed, generatorDef, size.Value / 2f);

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
                planetInitArguments.AddGps = false;
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

        private long GetPlanetEntityId(string storageName)
        {
            long hash = storageName.GetHashCode64();
            return hash & 0x00FFFFFFFFFFFFFF | ((long)MyEntityIdentifier.ID_OBJECT_TYPE.ASTEROID << 56);
        }
    }
}
