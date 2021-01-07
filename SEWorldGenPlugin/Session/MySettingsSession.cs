using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using VRage.Game.Components;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Sessioncomponent that manages the current sessions plugin settings.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 1000)]
    public class MySettingsSession : MySessionComponentBase
    {
        /// <summary>
        /// Name of the save file in the world folder for the plugin settings.
        /// </summary>
        public const string FILE_NAME = "WorldSettings.xml";

        /// <summary>
        /// Singleton instance of this session component
        /// </summary>
        public static MySettingsSession Static;

        /// <summary>
        /// The current sessions plugin settings.
        /// </summary>
        public MyObjectBuilder_WorldSettings Settings
        {
            get;
            set;
        }

        /// <summary>
        /// If the plugin is enabled in this session.
        /// If run on a client connected to a server, with active plugin,
        /// it will still return false.
        /// </summary>
        /// <returns>True, if the plugin is enabled in this session.</returns>
        public bool IsEnabled()
        {
            return (Sync.IsServer && Settings.GeneratorSettings.WorldSize > 0 && Settings.Enabled);
        }

        /// <summary>
        /// Loads the savefile of the plugin settings of the world and stores it
        /// int the Settings member variable
        /// </summary>
        public override void LoadData()
        {
            Static = this;
            if (MyFileUtils.FileExistsInWorldStorage(FILE_NAME, typeof(MySettingsSession)))
            {
                Settings = MyFileUtils.ReadXmlFileFromWorld<MyObjectBuilder_WorldSettings>(FILE_NAME, typeof(MySettingsSession));
            }
            else
            {
                if (MySettings.Static == null)
                {
                    var s = new MySettings();
                    s.LoadSettings();
                }
                if (MySettings.Static.SessionSettings != null)
                {
                    Settings = MySettings.Static.SessionSettings;
                }
                else
                {
                    Settings = new MyObjectBuilder_WorldSettings();
                }

                MySettings.Static.SessionSettings = null;
            }
        }

        /// <summary>
        /// Saves the current sessions plugin settings to the world folder.
        /// </summary>
        public override void SaveData()
        {
            if(Sync.IsServer)
                MyFileUtils.WriteXmlFileToWorld(Settings, FILE_NAME, typeof(MySettingsSession));
        }

        /// <summary>
        /// Unloads the used data of this session component.
        /// </summary>
        protected override void UnloadData()
        {
            if (Sync.IsServer)
                MyFileUtils.WriteXmlFileToWorld(Settings, FILE_NAME, typeof(MySettingsSession));
            Settings = null;
        }
    }
}
