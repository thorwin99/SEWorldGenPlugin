using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEWorldGenPlugin.Utilities
{
    public class MyPluginTexts
    {
        public static ToolTips TOOLTIPS = new ToolTips();
    }
    public class ToolTips
    {
        public readonly string SYS_OBJ_SLIDER = "Objects that should be generated in the system";
        public readonly string ORB_DIST_SLIDER = "Average distance between Orbits.";
        public readonly string SIZE_MUL_SLIDER = "The diameter of each planet is scaled by this value";
        public readonly string SIZE_CAP_SLIDER = "The largest diameter a planet can have, if some planet would be larger, it will get this diameter. Value in kilometers";
        public readonly string MOON_PROB_SLIDER = "The probability a planet has moons";
        public readonly string RING_WIDTH_SLIDER = "Average width of a ring around a planet in meters";
        public readonly string RING_PROB_SLIDER = "Probability an asteroid ring spawn around a planet. Gets multiplied with the surface gravity of the planet, so larger planets have a higher chance to spawn a ring.";
        public readonly string BELT_HEIGHT_SLIDER = "Average height of an asteroid belt in meters.";
        public readonly string BELT_PROB_SLIDER = "The probability a belt will spawn every 6th object.";
        public readonly string USE_GLOBAL_CHECK = "If the settings for this world are based on the global configuration file.";
    }

}
