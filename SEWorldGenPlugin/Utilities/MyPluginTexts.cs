namespace SEWorldGenPlugin.Utilities
{
    public class MyPluginTexts
    {
        public static ToolTips TOOLTIPS = new ToolTips();
        public static Messages MESSAGES = new Messages();
    }
    public class ToolTips
    {
        public readonly string SYS_OBJ_SLIDER = "Objects that should be generated in the system";
        public readonly string ORB_DIST_SLIDER = "Average distance between Orbits in kilometres.";
        public readonly string SIZE_MUL_SLIDER = "The diameter of each planet is scaled by this value";
        public readonly string SIZE_CAP_SLIDER = "The largest diameter a planet can have, if some planet would be larger, it will get this diameter. Value in kilometers";
        public readonly string MOON_PROB_SLIDER = "The probability a planet has moons";
        public readonly string RING_WIDTH_SLIDER = "Average width of a ring around a planet in meters";
        public readonly string RING_PROB_SLIDER = "Probability an asteroid ring spawn around a planet. Gets multiplied with the surface gravity of the planet, so larger planets have a higher chance to spawn a ring.";
        public readonly string BELT_HEIGHT_SLIDER = "Average height of an asteroid belt in meters.";
        public readonly string BELT_PROB_SLIDER = "The probability a belt will spawn every 6th object.";
        public readonly string USE_GLOBAL_CHECK = "If the settings for this world are based on the global configuration file.";
        public readonly string PLANET_GPSL_CHECK = "If a GPS signal should be generated for each planet on world generation.";
        public readonly string MOON_GPS_CHECK = "If a GPS signal should be generated for each moon on world generation.";
        public readonly string BELT_GPS_CHECK = "If a GPS signal should be generated for each asteroid belt.";
        public readonly string ROID_GEN_COMBO = "Which asteroid generator should be used. If plugin, belts and rings will be generated, but nothing else. If vanilla, the default generation will be used, but no rings or belts are generated.";
        public readonly string ROID_DENS_SLIDER = "The density of the asteroids, where 1 is the most dense.";

        public readonly string ADMIN_RING_DISTANCE = "The distance of the ring to the planet in meters";
        public readonly string ADMIN_RING_WIDTH = "The width of the ring in meters";
        public readonly string ADMIN_RING_ANGLE = "The angle of the ring around the wanted axis in degrees";
        public readonly string ADMIN_RING_ROID_SIZE = "The size of the asteroids in the ring in meters";
        public readonly string ADMIN_ADD_RING_BUTTON = "Adds the ring to the planet";
        public readonly string ADMIN_REMOVE_RING_BUTTON = "Removes the ring from the planet. All previously generated Asteroids will stay.";
        public readonly string ADMIN_TP_RING_BUTTON = "Teleports your player to the ring of the planet";
        public readonly string ADMIN_PLANET_SIZE = "The size of the planet in kilometers";
        public readonly string ADMIN_SPAWN_PLANET = "Spawns the planet with given size";
    }

    public class Messages
    {
        public readonly string UPDATE_AVAILABLE_TITLE = "SEWorldGenPlugin Update available";
        public readonly string UPDATE_AVAILABLE_BOX = "A new version of the SEWorldGenPlugin is a available. Do you want to visit the download page now?";
    }

}
