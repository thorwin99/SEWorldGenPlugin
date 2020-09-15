using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;

namespace SEWorldGenPlugin
{
    /// <summary>
    /// Class that holds the global plugin settings, aswell as the settings to load for the current
    /// session, if it is a new one.
    /// </summary>
    public class MySettings
    {
        /// <summary>
        /// Name of the global config file
        /// </summary>
        private const string FILENAME = "SEWorldGenPlugin.xml";

        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        public static MySettings Static
        {
            get;
            private set;
        }

        /// <summary>
        /// Global configuration settings
        /// </summary>
        public MyObjectBuilder_PluginSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// To be loaded session settings
        /// </summary>
        public MyObjectBuilder_PluginSettings SessionSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new singleton instance of this class
        /// </summary>
        public MySettings()
        {
            Static = this;
            Settings = new MyObjectBuilder_PluginSettings();
        }

        /// <summary>
        /// Loads the global configuration file if it exists, otherwise creates a new one.
        /// </summary>
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

        /// <summary>
        /// Saves the global configuration file
        /// </summary>
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
