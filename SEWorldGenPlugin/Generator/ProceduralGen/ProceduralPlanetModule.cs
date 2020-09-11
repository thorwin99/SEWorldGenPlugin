using Sandbox.Definitions;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.Game.Voxels;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGen
{
    /// <summary>
    /// Class used to generate planets. Does not inherit SEWorldGenPlugin.Generator.ProceduralGen.ProceduralModule
    /// because planets can always be generated and not in cells.
    /// </summary>
    public class ProceduralPlanetModule
    {
        private int m_seed;

        /// <summary>
        /// Creates a new planet generator based on seed
        /// </summary>
        /// <param name="seed">The seed of the generator</param>
        public ProceduralPlanetModule(int seed)
        {
            m_seed = seed;
        }

        /// <summary>
        /// Generates all planet of the plugin, that have not been generated yet.
        /// </summary>
        public void GeneratePlanets()
        {
            foreach(var obj in SystemGenerator.Static.Objects)
            {
                if (obj == null) continue;
                if (obj.Type != SystemObjectType.PLANET) continue;

                MyPlanetItem planet = (MyPlanetItem)obj;

                if (planet.Generated) continue;

                MyPlanetGeneratorDefinition definition = GetDefinition(planet.DefName);

                if (definition == null) continue;

                long id = MyRandom.Instance.NextLong();
                string name = (planet.DisplayName + " - " + definition.Id.SubtypeId).Replace(" ", "_");

                if (planet.CenterPosition.Equals(Vector3D.Zero))
                {
                    planet.CenterPosition = planet.OffsetPosition;
                }

                MyPlanet generatedPlanet = MyWorldGenerator.AddPlanet(name, planet.DisplayName, planet.DefName, planet.CenterPosition - GetPlanetOffset(definition, planet.Size), m_seed, planet.Size, true, id, false, true);
                
                if(generatedPlanet == null)
                {
                    continue;
                }

                planet.CenterPosition = generatedPlanet.PositionComp.GetPosition();
                generatedPlanet.DisplayNameText = planet.DisplayName;
                generatedPlanet.AsteroidName = planet.DisplayName;

                if (planet.PlanetRing != null)
                    planet.PlanetRing.Center = planet.CenterPosition;

                List<Vector3D> spawnedMoons = new List<Vector3D>();

                for (int i = 0; i < planet.PlanetMoons.Length; i++)
                {
                    MyPlanetMoonItem moon = planet.PlanetMoons[i];

                    if(moon == null)
                    {
                        continue;
                    }

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

                    if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.ShowMoonGPS)
                        GlobalGpsManager.Static.AddGps(moon.DisplayName, Color.Aqua, spawnedMoon.PositionComp.GetPosition());
                }
                
                planet.Generated = true;

                if (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.ShowPlanetGPS)
                    GlobalGpsManager.Static.AddGps(planet.DisplayName, Color.Aqua, generatedPlanet.PositionComp.GetPosition());
            }
        }

        /// <summary>
        /// Gets the offset position of the planet based on its center position,
        /// since SE planets are generated based of the offset position.
        /// </summary>
        /// <param name="definition">The generator definition of the planet</param>
        /// <param name="size">The diameter of the planet in meters</param>
        /// <returns>The offset position for the planet</returns>
        private Vector3D GetPlanetOffset(MyPlanetGeneratorDefinition definition, float size)
        {
            MyPlanetStorageProvider myPlanetStorageProvider = new MyPlanetStorageProvider();
            myPlanetStorageProvider.Init(0, definition, size / 2f);
            IMyStorage myStorage = new MyOctreeStorage(myPlanetStorageProvider, myPlanetStorageProvider.StorageSize);

            return myStorage.Size / 2.0f;
        }

        /// <summary>
        /// Gets the planet generator definition of the planet with the given name
        /// </summary>
        /// <param name="name">Name of the planet</param>
        /// <returns>The planets MyPlanetGeneratorDefinition</returns>
        private MyPlanetGeneratorDefinition GetDefinition(string name)
        {
            return MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(name));
        }

        /// <summary>
        /// Checks whether a position is obstructed. It is obstructed if one position in other
        /// is closer to position than minDistance, or if the position is inside an asteroid ring
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="other">Other positions, that might obstruct</param>
        /// <param name="minDistance">Minimum distance from position to the other positions</param>
        /// <param name="ring">Possible asteroid ring to check</param>
        /// <returns>True if unobstructed, false otherwise</returns>
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
