using VRage.Library.Utils;

namespace SEWorldGenPlugin.Utilities
{
    static class MyRandomExtension
    {

        public static long Next(this MyRandom r, long min, long max)
        {
            return (long)(r.NextDouble() * (max - min) + min);
        }
    }
}
