using ProtoBuf;
using System;
using VRage;

namespace SEWorldGenPlugin.ObjectBuilders
{

    /// <summary>
    /// Serializable ObjectBuilder used for the worlds settings for the plugin. Contains 
    /// wheter it is enabled or not, and the Generator settings.
    /// </summary>
    [ProtoContract]
    public class MyObjectBuilder_WorldSettings : MyAbstractConfigObjectBuilder
    {

        [ProtoMember(1)]
        public bool Enabled = false;

        [ProtoMember(2)]
        public MyObjectBuilder_GeneratorSettings GeneratorSettings = null;

        public override MyAbstractConfigObjectBuilder copy()
        {
            MyObjectBuilder_WorldSettings copy = new MyObjectBuilder_WorldSettings();
            copy.Enabled = Enabled;
            copy.GeneratorSettings = GeneratorSettings == null ? null : GeneratorSettings.copy() as MyObjectBuilder_GeneratorSettings;

            return new MyObjectBuilder_WorldSettings();
        }

        public override void Verify()
        {
            return;
        }
    }

    /// <summary>
    /// Serializable object builder for the system generator settings. This object builder
    /// defines the parameters, the systemgenerator uses to generate the worlds system.
    /// </summary>
    [ProtoContract]
    public class MyObjectBuilder_GeneratorSettings : MyAbstractConfigObjectBuilder
    {
        [ProtoMember(1)]
        public SystemGenerationMethod SystemGenerator = SystemGenerationMethod.FULL_RANDOM;

        [ProtoMember(2)]
        public AsteroidGenerationMethod AsteroidGenerator = AsteroidGenerationMethod.PLUGIN;

        [ProtoMember(3)]
        public SerializableMinMax MinMaxPlanets = new SerializableMinMax(5, 15);

        [ProtoMember(4)]
        public SerializableMinMax MinMaxAsteroidObjects = new SerializableMinMax(5, 15);

        public override MyAbstractConfigObjectBuilder copy()
        {
            return new MyObjectBuilder_GeneratorSettings();
        }

        public override void Verify()
        {
            return;
        }
    }

    /// <summary>
    /// Serializable struct used to represent a min max pair of values.
    /// The values are automatically sorted in the constructor, so
    /// min is always min and max is always max.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public struct SerializableMinMax
    {
        [ProtoMember(1)]
        int Min;

        [ProtoMember(2)]
        int Max;

        public SerializableMinMax(int v1, int v2)
        {
            Min = Math.Min(v1, v2);
            Max = Math.Max(v1, v2);
        }
    }

    /// <summary>
    /// Enum to set which asteroid generator the plugin uses.
    /// </summary>
    public enum AsteroidGenerationMethod
    {
        PLUGIN,
        VANILLA,
        BOTH
    }

    /// <summary>
    /// Enum to set which system generator the plugin uses.
    /// Full random means, every planet has the same chance to be generated,
    /// Mandatory first, that first, all available mandatory planets get generated,
    /// and only after that full random happens,
    /// Mandatory only means that only mandatory planets are used.
    /// </summary>
    public enum SystemGenerationMethod
    {
        FULL_RANDOM,
        MANDATORY_FIRST,
        MANDATORY_ONLY
    }
}
