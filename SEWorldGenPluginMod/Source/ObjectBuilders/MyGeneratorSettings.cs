using ProtoBuf;

namespace SEWorldGenPluginMod.Source.ObjectBuilders
{
    /// <summary>
    /// Class representing plugin specific generator settings of the server, the client needs
    /// </summary>
    [ProtoContract]
    public class MyGeneratorSettings
    {
        /// <summary>
        /// Whether to even generate anything
        /// </summary>
        [ProtoMember(1)]
        public bool UsePluginGenerator;

        /// <summary>
        /// Density of asteroids
        /// </summary>
        [ProtoMember(2)]
        public float AsteroidDensity;

        /// <summary>
        /// World size
        /// </summary>
        [ProtoMember(3)]
        public long WorldSize;
    }
}