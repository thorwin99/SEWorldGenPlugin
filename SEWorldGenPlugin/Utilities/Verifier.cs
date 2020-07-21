namespace SEWorldGenPlugin.Utilities
{
    public class Verifier
    {
        public static void VerifyInt(int min, int max, int def, string name, ref int value)
        {
            if (value < min || value > max)
            {
                value = def;

                PluginLog.Log("Config value " + name + " of was invalid and will be set to default.", LogLevel.WARNING);
            }
        }

        public static void VerifyLong(long min, long max, long def, string name, ref long value)
        {
            if (value < min || value > max)
            {
                value = def;

                PluginLog.Log("Config value " + name + " of was invalid and will be set to default.", LogLevel.WARNING);
            }
        }

        public static void VerifyFloat(float min, float max, float def, string name, ref float value)
        {
            if (value < min || value > max)
            {
                value = def;

                PluginLog.Log("Config value " + name + " of was invalid and will be set to default.", LogLevel.WARNING);
            }
        }

        public static void VerifyDouble(double min, double max, double def, string name, ref double value)
        {
            if (value < min || value > max)
            {
                value = def;

                PluginLog.Log("Config value " + name + " of was invalid and will be set to default.", LogLevel.WARNING);
            }
        }
    }
}