using Sandbox.Definitions;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Library.Utils;
using VRage.Utils;

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

            MyLog.Default.WriteLine("BeforeStart Definitions are ");
            foreach (var d in defs) MyLog.Default.WriteLine(d.ToString());
            MyLog.Default.WriteLine("BeforeStart Folder is " + Path.Combine(MySession.Static.CurrentPath, "Storage", "SEWorldGenPlugin", STORAGE_FILE));
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

        public override void LoadData()
        {
            defs = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions().ToList();
            MyLog.Default.WriteLine("LoadData Folder is " + Path.Combine(MySession.Static.CurrentPath, "Storage", "SEWorldGenPlugin", STORAGE_FILE));
            MyLog.Default.WriteLine("LoadData Definitions are ");
            foreach (var d in defs) MyLog.Default.WriteLine(d.ToString());
        }
    }
}
