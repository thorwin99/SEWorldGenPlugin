using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using VRage.Utils;

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

        private MyObjectBuilder_PluginSettings m_sessonSettings;
        public MyObjectBuilder_PluginSettings SessionSettings
        {
            get { return m_sessonSettings == null ? Settings : m_sessonSettings; }
            set { m_sessonSettings = value; }
        }

        public MySettings()
        {
            Static = this;
            Settings = new MyObjectBuilder_PluginSettings();
        }

        public void LoadSettings()
        {
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
                    MyLog.Default.Error("Couldnt load Plugin config file.");
                    MyLog.Default.Error(e.Message + "\n" + e.StackTrace);
                    FileUtils.DeleteFileInGlobalStorage(FILENAME);
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
            FileUtils.DeleteFileInGlobalStorage(FILENAME);

            string xml = FileUtils.SerializeToXml(Settings);

            MyLog.Default.WriteLine("Saving SEWorldGenPlugin config file: " + xml);

            using (var writer = FileUtils.WriteFileInGlobalStorage(FILENAME))
            {
                writer.Write(xml);
                writer.Close();
            }
        }


    }
}
