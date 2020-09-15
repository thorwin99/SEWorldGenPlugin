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
    public class SettingsSession : MySessionComponentBase
    {
        /// <summary>
        /// Name of the save file in the world folder for the plugin settings.
        /// </summary>
        public const string FILE_NAME = "worldSettings.xml";

        /// <summary>
        /// Singleton instance of this session component
        /// </summary>
        public static SettingsSession Static;

        /// <summary>
        /// The current sessions plugin settings.
        /// </summary>
        public MyObjectBuilder_PluginSettings Settings
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the savefile of the plugin settings of the world and stores it
        /// int the Settings member variable
        /// </summary>
        public override void LoadData()
        {
            Static = this;
            if (FileUtils.FileExistsInWorldStorage(FILE_NAME, typeof(SettingsSession)))
            {
                Settings = FileUtils.ReadXmlFileFromWorld<MyObjectBuilder_PluginSettings>(FILE_NAME, typeof(SettingsSession));
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
                    Settings = MySettings.Static.Settings;
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
                FileUtils.WriteXmlFileToWorld(Settings, FILE_NAME, typeof(SettingsSession));
        }

        /// <summary>
        /// Unloads the used data of this session component.
        /// </summary>
        protected override void UnloadData()
        {
            FileUtils.WriteXmlFileToWorld(Settings, FILE_NAME, typeof(SettingsSession));
            Settings = null;
        }
    }
}
