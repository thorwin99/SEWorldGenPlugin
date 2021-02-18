namespace SEWorldGenPlugin.Utilities
{
    /// <summary>
    /// Simple class to have a list of all static texts for the plugin.
    /// </summary>
    public class MyPluginTexts
    {
        public static ToolTips TOOLTIPS = new ToolTips();
        public static Messages MESSAGES = new Messages();
    }
    /// <summary>
    /// Class, that contains all tooltips
    /// </summary>
    public class ToolTips
    {
        public readonly string SYS_GEN_MODE_COMBO = "The mode of the star system generator for this world.";
        public readonly string ASTEROID_GEN_MODE_COMBO = "The mode of the asteroid generator for this world.";
        public readonly string VANILLA_PLANETS_CHECK = "If vanilla planets can be used for system generation.";
        public readonly string PLANET_COUNT_SLIDER = "The minimum and maximum possible amount of planets in this world.";
        public readonly string ASTEROID_COUNT_SLIDER = "The minimum and maximum possible amount of asteroid objects in this world.";
        public readonly string ORBIT_DISTANCE_SLIDER = "The range for orbit distances between the system objects. In KM";
        public readonly string ASTEROID_DENS_SLIDER = "The density for asteroid generation. 1 is the densest.";
        public readonly string WORLD_SIZE_SLIDER = "The max size of the system. After that nothing will be generated. In KM";
        public readonly string PLANET_SIZE_CAP_SLIDER = "The max size a planet can have in this system. In metres";
        public readonly string PLANET_SIZE_MULT = "The multiplier for the planets size. 1G planet equals to 120km * this value. A gas giant will be double the size.";
        public readonly string PLANET_MOON_PROP = "The base probability for a planet to generate moons. Scales with gravity, so higher gravity means higher probability.";
        public readonly string PLANET_RING_PROP = "The base probability for a planet to generate a ring. Scales with gravity, so higher gravity means higher probability.";
        public readonly string PLANET_MOON_COUNT = "The minimum and maximum limits for the amount of moons around a planet.";
        public readonly string PLANET_GPS_COMBO = "The generation mode for planet gpss. Discovery means a player needs to be within 50k km to see the dynamic gps.";
        public readonly string MOON_GPS_COMBO = "The generation mode for moons gpss. Discovery means a player needs to be within 50k km to see the dynamic gps.";
        public readonly string ASTEROID_GPS_COMBO = "The generation mode for asteroid gpss. Discovery means a player needs to be within 50k km to see the dynamic gps.";

        public readonly string ADMIN_RING_DISTANCE = "The distance of the ring to the planet in meters";
        public readonly string ADMIN_RING_WIDTH = "The width of the ring in meters";
        public readonly string ADMIN_RING_ANGLE = "The angle of the ring around the wanted axis in degrees";
        public readonly string ADMIN_RING_ROID_SIZE = "The minimum size of the asteroids in the ring in meters";
        public readonly string ADMIN_RING_ROID_SIZE_MAX = "The maximum size of the asteroids in the ring in meters.";
        public readonly string ADMIN_ADD_RING_BUTTON = "Adds the ring to the planet";
        public readonly string ADMIN_REMOVE_RING_BUTTON = "Removes the ring from the planet. All previously generated Asteroids will stay.";
        public readonly string ADMIN_TP_RING_BUTTON = "Teleports your player to the ring of the planet";
        public readonly string ADMIN_PLANET_SIZE = "The size of the planet in kilometers";
        public readonly string ADMIN_SPAWN_PLANET = "Spawns the planet with given size";
    }

    /// <summary>
    /// Class that contains all messages
    /// </summary>
    public class Messages
    {
        public readonly string UPDATE_AVAILABLE_TITLE = "SEWorldGenPlugin Update available";
        public readonly string UPDATE_AVAILABLE_BOX = "A new version of the SEWorldGenPlugin is a available. Do you want to visit the download page now?";
    }

}
