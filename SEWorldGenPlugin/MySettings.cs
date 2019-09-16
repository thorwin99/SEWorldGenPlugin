using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities.SEWorldGenPlugin.Utilities;
using System;
using VRage.Utils;

namespace SEWorldGenPlugin
{
    public class MySettings
    {
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

        public MySettings()
        {
            Static = this;
            Settings = new MyObjectBuilder_PluginSettings();
        }

        public void LoadSettings()
        {
            if (FileUtils.FileExistsInGlobalStorage("SEWorldGenPlugin.cfg"))
            {
                try
                {
                    using (var reader = FileUtils.ReadFileInGlobalStorage("SEWorldGenPlugin.cfg"))
                    {
                        MyObjectBuilder_PluginSettings saveFile = FileUtils.SerializeFromXml<MyObjectBuilder_PluginSettings>(reader.ReadToEnd());
                        if (saveFile != null)
                            Settings = saveFile;
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.Error("Couldnt load Plugin config file.");
                    MyLog.Default.Error(e.Message + "\n" + e.StackTrace);
                    FileUtils.DeleteFileInGlobalStorage("SEWorldGenPlugin.cfg");
                    Settings = new MyObjectBuilder_PluginSettings();
                }
            }
            else
            {
                Settings = new MyObjectBuilder_PluginSettings();
            }
            Settings.Verify();
        }

        public void SaveSettings()
        {
            FileUtils.DeleteFileInGlobalStorage("SEWorldGenPlugin.cfg");

            string xml = FileUtils.SerializeToXml(Settings);

            using (var writer = FileUtils.WriteFileInGlobalStorage("SEWorldGenPlugin.cfg"))
            {
                writer.Write(xml);
                writer.Close();
            }
        }


    }
}
