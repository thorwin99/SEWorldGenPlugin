﻿using ProtoBuf;
using SEWorldGenPlugin.Utilities;
using System;

namespace SEWorldGenPlugin.ObjectBuilders
{

    /// <summary>
    /// Serializable ObjectBuilder used for the worlds settings for the plugin. Contains 
    /// wheter it is enabled or not, and the Generator settings.
    /// </summary>
    [ProtoContract]
    public class MyObjectBuilder_WorldSettings : MyAbstractConfigObjectBuilder
    {

        /// <summary>
        /// If the plugin should be enabled for the world, this OB belongs to.
        /// </summary>
        [ProtoMember(1)]
        public bool Enabled = false;

        /// <summary>
        /// The general generation settings for the world, this OB belongs to.
        /// </summary>
        [ProtoMember(2)]
        public MyObjectBuilder_GeneratorSettings GeneratorSettings = new MyObjectBuilder_GeneratorSettings();

        public override MyAbstractConfigObjectBuilder copy()
        {
            MyObjectBuilder_WorldSettings copy = new MyObjectBuilder_WorldSettings();
            copy.Enabled = Enabled;
            copy.GeneratorSettings = GeneratorSettings == null ? null : GeneratorSettings.copy() as MyObjectBuilder_GeneratorSettings;

            return new MyObjectBuilder_WorldSettings();
        }

        public override void Verify()
        {
            GeneratorSettings?.Verify();
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
        /// <summary>
        /// The type of System generation method the plugin uses.
        /// </summary>
        [ProtoMember(1)]
        public SystemGenerationMethod SystemGenerator = SystemGenerationMethod.FULL_RANDOM;

        /// <summary>
        /// The type of asteroid generator the plugin uses.
        /// </summary>
        [ProtoMember(2)]
        public AsteroidGenerationMethod AsteroidGenerator = AsteroidGenerationMethod.PLUGIN;

        /// <summary>
        /// If Vanilla planets are allowed in the generation
        /// </summary>
        [ProtoMember(3)]
        public bool AllowVanillaPlanets = true;

        /// <summary>
        /// The minimum and maximum amount of planets in the system.
        /// </summary>
        [ProtoMember(4)]
        public MySerializableMinMax MinMaxPlanets = new MySerializableMinMax(5, 15);

        /// <summary>
        /// The minimum and maximum amount of asteroid objects in the system. Example being belts.
        /// Asteroid rings around planets dont count.
        /// </summary>
        [ProtoMember(5)]
        public MySerializableMinMax MinMaxAsteroidObjects = new MySerializableMinMax(5, 15);

        /// <summary>
        /// The minimum and maximum distance between orbits in metres
        /// </summary>
        [ProtoMember(6)]
        public MySerializableMinMax MinMaxOrbitDistance = new MySerializableMinMax(4000000, 100000000);

        /// <summary>
        /// The density of asteroid fields generated by the plugin.
        /// </summary>
        [ProtoMember(7)]
        public float AsteroidDensity = 0.6f;

        /// <summary>
        /// The max distance objects will still be generated at, if value is > 0.
        /// If it is < 0, then no limit is set. In meters
        /// </summary>
        [ProtoMember(8)]
        public long WorldSize = -1;

        /// <summary>
        /// Specific settings used to generate a planet in the system.
        /// </summary>
        [ProtoMember(9)]
        public MyObjectBuilder_PlanetGenerationSettings PlanetSettings = new MyObjectBuilder_PlanetGenerationSettings();

        /// <summary>
        /// Specific settings used to generate the gpss for the system objects
        /// </summary>
        [ProtoMember(10)]
        public MyObjectBuilder_GPSGenerationSettings GPSSettings = new MyObjectBuilder_GPSGenerationSettings();

        public override MyAbstractConfigObjectBuilder copy()
        {
            return new MyObjectBuilder_GeneratorSettings();
        }

        public override void Verify()
        {
            MinMaxPlanets.Verify();
            MinMaxAsteroidObjects.Verify();
            MinMaxOrbitDistance.Verify();

            MyValueVerifier.VerifyFloat(0f, 1f, 0.6f, "AsteroidDensity", ref AsteroidDensity);
            MyValueVerifier.VerifyLong(-1, long.MaxValue, -1, "WorldSize", ref WorldSize);

            PlanetSettings.Verify();
            GPSSettings.Verify();
            return;
        }
    }

    /// <summary>
    /// Serializable object builder for planet specific generation settings.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyObjectBuilder_PlanetGenerationSettings : MyAbstractConfigObjectBuilder
    {
        /// <summary>
        /// Planets will be scaled up from their default size by this modifier.
        /// The default size of a planet is calculated from its gravity, where 120km equals 1g.
        /// For gas giants, it is 240km equals 1g.
        /// </summary>
        [ProtoMember(1)]
        public float PlanetSizeMultiplier = 2.0f;

        /// <summary>
        /// The maximum diameter a planet can have in meters. No planet will be larger than
        /// the value set here, if it was generated by the plugin.
        /// In meters
        /// </summary>
        [ProtoMember(2)]
        public int PlanetSizeCap = 1200000;

        public override MyAbstractConfigObjectBuilder copy()
        {
            var copy = new MyObjectBuilder_PlanetGenerationSettings();
            copy.PlanetSizeCap = PlanetSizeCap;
            copy.PlanetSizeMultiplier = PlanetSizeMultiplier;
            return copy;
        }

        public override void Verify()
        {
            MyValueVerifier.VerifyFloat(0.1f, 100f, 2.0f, "PlanetSizeMultiplier", ref PlanetSizeMultiplier);
            MyValueVerifier.VerifyInt(1, int.MaxValue, 1200000, "PlanetSizeCap", ref PlanetSizeCap);
        }
    }

    /// <summary>
    /// Class used to set the gps settings for the world.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyObjectBuilder_GPSGenerationSettings : MyAbstractConfigObjectBuilder
    {
        /// <summary>
        /// The gps generation method used for planets.
        /// </summary>
        [ProtoMember(1)]
        public MyGPSGenerationMode PlanetGPSMode = MyGPSGenerationMode.PERSISTENT;

        /// <summary>
        /// The gps generation method used for moons.
        /// </summary>
        [ProtoMember(2)]
        public MyGPSGenerationMode MoonGPSMode = MyGPSGenerationMode.NONE;

        /// <summary>
        /// The gps generation method used for rings.
        /// </summary>
        [ProtoMember(3)]
        public MyGPSGenerationMode AsteroidGPSMode = MyGPSGenerationMode.DISCOVERY;

        public override MyAbstractConfigObjectBuilder copy()
        {
            var copy = new MyObjectBuilder_GPSGenerationSettings();
            copy.PlanetGPSMode = PlanetGPSMode;
            copy.MoonGPSMode = MoonGPSMode;
            copy.AsteroidGPSMode = AsteroidGPSMode;

            return copy;
        }

        public override void Verify()
        {
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
    /// Unique means, that the system generator tries to keep planets and moons
    /// unique, but duplicates can occour, if there are less planets loaded with the world,
    /// than the minimum amount of planets defined.
    /// Mandatory first, that first, all available mandatory planets get generated,
    /// and only after that full random happens,
    /// Mandatory only means that only mandatory planets are used.
    /// </summary>
    public enum SystemGenerationMethod
    {
        FULL_RANDOM,
        UNIQUE,
        MANDATORY_FIRST,
        MANDATORY_ONLY
    }

    /// <summary>
    /// An enum used to set how gpss are generated by the plugin. They can either be generated once, on discovery of the object,
    /// or no gpss are generated.
    /// </summary>
    public enum MyGPSGenerationMode
    {
        DISCOVERY,
        PERSISTENT,
        NONE
    }
}
