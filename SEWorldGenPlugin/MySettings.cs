﻿using SEWorldGenPlugin.ObjectBuilders;
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

        static MySettings()
        {
            Static = new MySettings();
            Static.LoadSettings();
        }

        /// <summary>
        /// Global configuration settings
        /// </summary>
        public MyObjectBuilder_GlobalSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// To be loaded session settings.
        /// </summary>
        public MyObjectBuilder_WorldSettings SessionSettings
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
            Settings = new MyObjectBuilder_GlobalSettings();
        }

        /// <summary>
        /// Loads the global configuration file if it exists, otherwise creates a new one.
        /// </summary>
        public void LoadSettings()
        {
            MyPluginLog.Log("Loading the global config file");
            if (MyFileUtils.FileExistsInGlobalStorage(FILENAME))
            {
                try
                {
                    using (var reader = MyFileUtils.ReadFileInGlobalStorage(FILENAME))
                    {
                        MyPluginLog.Log("Reading global config file");
                        MyObjectBuilder_GlobalSettings saveFile = MyFileUtils.SerializeFromXml<MyObjectBuilder_GlobalSettings>(reader.ReadToEnd());
                        if (saveFile != null)
                        {
                            MyPluginLog.Log("Serialized global config file");
                            Settings = saveFile;
                        }
                    }
                }
                catch (Exception e)
                {
                    MyPluginLog.Log("Couldnt load Plugin config file.", LogLevel.ERROR);
                    MyPluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);
                    MyFileUtils.DeleteFileInGlobalStorage(FILENAME);
                    Settings = new MyObjectBuilder_GlobalSettings();
                    Settings.MoonDefinitions.Add("Moon");
                    Settings.MoonDefinitions.Add("Titan");
                    Settings.MoonDefinitions.Add("Europa");
                    Settings.MoonDefinitions.Add("Triton");
                    Settings.BlacklistedPlanetDefinitions.Add("EarthLikeTutorial");
                    Settings.BlacklistedPlanetDefinitions.Add("MarsTutorial");
                    Settings.BlacklistedPlanetDefinitions.Add("MoonTutorial");
                    Settings.BlacklistedPlanetDefinitions.Add("SystemTestMap");
                    Settings.BlacklistedPlanetDefinitions.Add("EarthLikeModExample");
                    Settings.FixedPlanetSizes.Add(new PlanetSizeDefinition("Example", 1000000));
                }
            }
            else
            {
                MyPluginLog.Log("Config does not exist, creating default one");
                Settings = new MyObjectBuilder_GlobalSettings();
                Settings.MoonDefinitions.Add("Moon");
                Settings.MoonDefinitions.Add("Titan");
                Settings.MoonDefinitions.Add("Europa");
                Settings.MoonDefinitions.Add("Triton");
                Settings.BlacklistedPlanetDefinitions.Add("EarthLikeTutorial");
                Settings.BlacklistedPlanetDefinitions.Add("MarsTutorial");
                Settings.BlacklistedPlanetDefinitions.Add("MoonTutorial");
                Settings.BlacklistedPlanetDefinitions.Add("SystemTestMap");
                Settings.BlacklistedPlanetDefinitions.Add("EarthLikeModExample");
                Settings.FixedPlanetSizes.Add(new PlanetSizeDefinition("Example", 1000000));
            }
            Settings.Verify();
            MyPluginLog.Log("Config loaded");
        }

        /// <summary>
        /// Saves the global configuration file
        /// </summary>
        public void SaveSettings()
        {
            MyFileUtils.DeleteFileInGlobalStorage(FILENAME);

            string xml = MyFileUtils.SerializeToXml(Settings);

            MyPluginLog.Log("Saving global SEWorldGenPlugin config file: " + xml);

            using (var writer = MyFileUtils.WriteFileInGlobalStorage(FILENAME))
            {
                writer.Write(xml);
                writer.Close();
            }
        }
    }
}
