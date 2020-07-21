using VRage.Utils;

namespace SEWorldGenPlugin.Utilities
{
    public enum LogLevel
    {
        INFO,
        WARNING,
        ERROR
    };

    public class PluginLog
    {
        public static void Log(string message, LogLevel level = LogLevel.INFO)
        {
            switch (level)
            {
                case LogLevel.INFO:
                    MyLog.Default.WriteLine("SEWorldGenPlugin - " + level.ToString() + " " + message);
                    break;
                case LogLevel.WARNING:
                    MyLog.Default.Warning("SEWorldGenPlugin - " + level.ToString() + " " + message);
                    break;
                case LogLevel.ERROR:
                    MyLog.Default.Error("SEWorldGenPlugin - " + level.ToString() + " " + message);
                    break;
            }
        }
    }
}
