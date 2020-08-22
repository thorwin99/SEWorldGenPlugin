using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;

namespace SEWorldGenPlugin
{
    public class MySettings
    {
        private const string FILENAME = "SEWorldGenPlugin.xml";

        public static MySettings Static
        {
            get;
            private set;
        }

        public MyObjectBuilder_PluginSettings Settings
        {
            get;
            private set;
        }

        public MyObjectBuilder_PluginSettings SessionSettings
        {
            get;
            set;
        }

        public MySettings()
        {
            Static = this;
            Settings = new MyObjectBuilder_PluginSettings();
        }

        public void LoadSettings()
        {
            PluginLog.Log("Loading the global config file");
            if (FileUtils.FileExistsInGlobalStorage(FILENAME))
            {
                try
                {
                    using (var reader = FileUtils.ReadFileInGlobalStorage(FILENAME))
                    {
                        MyObjectBuilder_PluginSettings saveFile = FileUtils.SerializeFromXml<MyObjectBuilder_PluginSettings>(reader.ReadToEnd());
                        if (saveFile != null)
                            Settings = saveFile;
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Log("Couldnt load Plugin config file.", LogLevel.ERROR);
                    PluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);
                    FileUtils.DeleteFileInGlobalStorage(FILENAME);
                    Settings = new MyObjectBuilder_PluginSettings();
                }
            }
            else
            {
                PluginLog.Log("Config does not exist, creating default one");
                Settings = new MyObjectBuilder_PluginSettings();
                Settings.GeneratorSettings.PlanetSettings.Moons.Add("Moon");
                Settings.GeneratorSettings.PlanetSettings.Moons.Add("Titan");
                Settings.GeneratorSettings.PlanetSettings.Moons.Add("Europa");
                Settings.GeneratorSettings.PlanetSettings.Moons.Add("Triton");
            }
            Settings.Verify();
            PluginLog.Log("Config loaded");
        }

        public void SaveSettings()
        {
            FileUtils.DeleteFileInGlobalStorage(FILENAME);

            string xml = FileUtils.SerializeToXml(Settings);

            PluginLog.Log("Saving global SEWorldGenPlugin config file: " + xml);

            using (var writer = FileUtils.WriteFileInGlobalStorage(FILENAME))
            {
                writer.Write(xml);
                writer.Close();
            }
        }


    }
}
