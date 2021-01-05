namespace SEWorldGenPlugin.Utilities
{
    /// <summary>
    /// Utility class to verify numerical values by checking min and max,
    /// and setting it to a default value, if it is out of bounds
    /// </summary>
    public class MyValueVerifier
    {
        /// <summary>
        /// Verifies an integer and sets it to def, if it is outside of the min max bounds.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="def">Default value</param>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Reference to the value to check</param>
        public static void VerifyInt(int min, int max, int def, string name, ref int value)
        {
            if (value < min || value > max)
            {
                value = def;

                MyPluginLog.Log("Config value " + name + " of was invalid and will be set to default.", LogLevel.WARNING);
            }
        }

        /// <summary>
        /// Verifies a long and sets it to def, if it is outside of the min max bounds.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="def">Default value</param>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Reference to the value to check</param>
        public static void VerifyLong(long min, long max, long def, string name, ref long value)
        {
            if (value < min || value > max)
            {
                value = def;

                MyPluginLog.Log("Config value " + name + " of was invalid and will be set to default.", LogLevel.WARNING);
            }
        }

        /// <summary>
        /// Verifies a float and sets it to def, if it is outside of the min max bounds.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="def">Default value</param>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Reference to the value to check</param>
        public static void VerifyFloat(float min, float max, float def, string name, ref float value)
        {
            if (value < min || value > max)
            {
                value = def;

                MyPluginLog.Log("Config value " + name + " of was invalid and will be set to default.", LogLevel.WARNING);
            }
        }

        /// <summary>
        /// Verifies a double and sets it to def, if it is outside of the min max bounds.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="def">Default value</param>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Reference to the value to check</param>
        public static void VerifyDouble(double min, double max, double def, string name, ref double value)
        {
            if (value < min || value > max)
            {
                value = def;

                MyPluginLog.Log("Config value " + name + " of was invalid and will be set to default.", LogLevel.WARNING);
            }
        }
    }
}