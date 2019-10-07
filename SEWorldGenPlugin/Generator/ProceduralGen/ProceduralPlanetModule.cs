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
                string name = (planet.DisplayName + " - " + definition.Id.SubtypeId).Replace(" ", "_");
                MyPlanet generatedPlanet = MyWorldGenerator.AddPlanet(name, planet.DisplayName, planet.DefName, planet.OffsetPosition, m_seed, planet.Size, true, id, false, false);
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
                    string storageNameMoon = ("Moon " + moon.DisplayName + " - " + moonDef.Id.SubtypeId).Replace(" ", "_");
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

        private bool ObstructedPlace(Vector3D position, List<Vector3D> other, float minDistance, MyPlanetRingItem ring)
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
    }
}
