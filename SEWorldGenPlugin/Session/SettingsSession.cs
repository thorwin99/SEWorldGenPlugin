using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace SEWorldGenPlugin.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 1000)]
    public class SettingsSession : MySessionComponentBase
    {
        public const string FILE_NAME = "worldSettings.xml";

        public static SettingsSession Static;

        public MyObjectBuilder_PluginSettings Settings
        {
            get;
            set;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            
        }

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
                if(MySettings.Static.SessionSettings != null)
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

        public override void SaveData()
        {
            if(Sync.IsServer)
                FileUtils.WriteXmlFileToWorld(Settings, FILE_NAME, typeof(SettingsSession));
        }

        protected override void UnloadData()
        {
            FileUtils.WriteXmlFileToWorld(Settings, FILE_NAME, typeof(SettingsSession));
            Settings = null;
        }
    }
}
