using VRage.Utils;

namespace SEWorldGenPlugin.Utilities
{
    /// <summary>
    /// Enum to represent the log level of a log message
    /// </summary>
    public enum LogLevel
    {
        INFO,
        WARNING,
        ERROR
    };

    /// <summary>
    /// Utility class to easily log message to the logfile with different log levels.
    /// </summary>
    public class PluginLog
    {

        /// <summary>
        /// Writes a message to the log file with an optional log level
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Optional log level</param>
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
