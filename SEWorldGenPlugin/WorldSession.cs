using Sandbox.Definitions;
using Sandbox.Engine.Utils;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.World.Generator;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.Generator.ProceduralWorld;
using SEWorldGenPlugin.SaveItems;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.Voxels;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;
using VRageRender.Messages;

namespace SEWorldGenPlugin
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    class WorldSession : MySessionComponentBase
    {
        public const string STORAGE_FILE = "WorldData.xml";

        List<MyPlanetGeneratorDefinition> defs;
        StarSystemGenerator gen;
        bool loaded = false;
        public override void BeforeStart()
        {
            MyLog.Default.WriteLine("Loaded Session");
            defs = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions().ToList();
            CleanPlanetDefs();
            MyLog.Default.WriteLine(defs.Count + " Definitionen an Planeten");
        }

        public override void UpdateBeforeSimulation()
        {
            if (!loaded)
            {
                LoadedWorld();
            }
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach(IMyPlayer p in players)
            {
                if (p == null) continue;
                if (p.GetPosition() == null) continue;
                gen.GeneratePossiblePlanets(p.GetPosition());
            }
        }

        public override void SaveData()
        {
            WriteStarSystem();
        }

        private void LoadedWorld()
        {
            if (gen != null) return;
            gen = new StarSystemGenerator(defs);

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(STORAGE_FILE, typeof(WorldSession)))
            {
                MyLog.Default.WriteLine("SaveFile exists");
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(STORAGE_FILE, typeof(WorldSession)))
                    {
                        MyLog.Default.WriteLine("Loading File");
                        ObjectBuilder_GeneratorSave saveFile = MyAPIGateway.Utilities.SerializeFromXML<ObjectBuilder_GeneratorSave>(reader.ReadToEnd());
                        gen.SaveData = saveFile;
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.Error("Couldnt load Starsystem save file.");
                    MyLog.Default.Error(e.Message + "\n" + e.StackTrace);
                    MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(WorldSession));
                }
            }
            else
            {
                MyLog.Default.WriteLine("Generating System");
                gen.GenerateSystem(MyRandom.Instance.CreateRandomSeed());
                WriteStarSystem();
            }
            loaded = true;
        }

        private void WriteStarSystem()
        {
            if (gen == null) return;
            if (gen.SaveData == null) return;
            MyAPIGateway.Utilities.DeleteFileInWorldStorage(STORAGE_FILE, typeof(WorldSession));
            string xml = MyAPIGateway.Utilities.SerializeToXML(gen.SaveData);
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(STORAGE_FILE, typeof(WorldSession)))
            {
                MyLog.Default.WriteLine(xml);
                writer.Write(xml);
                writer.Close();
            }
        }

        private void CleanPlanetDefs()
        {
            List<MyPlanetGeneratorDefinition> newDefs = new List<MyPlanetGeneratorDefinition>();
            foreach (var p in defs)
            {
                if (!p.Id.SubtypeId.String.Contains("Tutorial") && !p.Id.SubtypeId.String.Contains("TestMap") && !p.Id.SubtypeId.String.Contains("ModExample"))
                {
                    newDefs.Add(p);
                }
            }
            defs.Clear();
            defs = newDefs;
        }

        protected override void UnloadData()
        {
            defs = null;
            gen = null;
            loaded = false;
        }
    }
}
