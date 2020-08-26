using ProtoBuf;
using SEWorldGenPlugin.Utilities;
using System.Collections.Generic;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [ProtoContract]
    public class MyObjectBuilder_PluginSettings
    {
        public MyObjectBuilder_PluginSettings()
        {
            Enable = false;
            GeneratorSettings = new GeneratorSettings();
        }

        [ProtoMember(1)]
        public bool Enable;

        [ProtoMember(2)]
        public GeneratorSettings GeneratorSettings;

        public void Verify()
        {
            GeneratorSettings.Verify();
        }
    }

    [ProtoContract]
    public class GeneratorSettings
    {

        public GeneratorSettings()
        {
            MinObjectsInSystem = 5;
            MaxObjectsInSystem = 25;
            MinOrbitDistance = 4000000;
            MaxOrbitDistance = 10000000;
            AsteroidGenerator = AsteroidGenerator.PLUGIN;
            AsteroidDensity = 0.6f;
            PlanetSettings = new PlanetSettings();
            BeltSettings = new BeltSettings();
            SemiRandomizedGeneration = false;
            WorldSize = -1;
            FirstPlanetCenter = false;
            UseVanillaPlanets = true;
            PlanetsOnlyOnce = false;
            MoonsOnlyOnce = false;
        }

        [ProtoMember(1)]
        public int MinObjectsInSystem;

        [ProtoMember(2)]
        public int MaxObjectsInSystem;

        [ProtoMember(3)]
        public int MinOrbitDistance;

        [ProtoMember(4)]
        public int MaxOrbitDistance;

        [ProtoMember(5)]
        public AsteroidGenerator AsteroidGenerator;

        [ProtoMember(6)]
        public float AsteroidDensity;

        [ProtoMember(7)]
        public PlanetSettings PlanetSettings;

        [ProtoMember(8)]
        public BeltSettings BeltSettings;

        [ProtoMember(9)]
        public bool SemiRandomizedGeneration;

        [ProtoMember(10)]
        public long WorldSize;

        [ProtoMember(10)]
        public bool FirstPlanetCenter;

        [ProtoMember(11)]
        public bool UseVanillaPlanets;

        [ProtoMember(12)]
        public bool PlanetsOnlyOnce;

        [ProtoMember(13)]
        public bool MoonsOnlyOnce;

        public void Verify()
        {
            Verifier.VerifyInt(0, MaxObjectsInSystem, 5, "MinObjectsInSystem", ref MinObjectsInSystem);
            Verifier.VerifyInt(MinObjectsInSystem, int.MaxValue, 25, "MaxObjectsInSystem", ref MaxObjectsInSystem);
            Verifier.VerifyInt(0, MinOrbitDistance, 4000000, "MinOrbitDistance", ref MinOrbitDistance);
            Verifier.VerifyInt(MinOrbitDistance, int.MaxValue, 10000000, "MaxOrbitDistance", ref MaxOrbitDistance);
            Verifier.VerifyFloat(0f, 1f, 0.5f, "AsteroidDensity", ref AsteroidDensity);
            Verifier.VerifyLong(-1, long.MaxValue, 10000000, "MaxOrbitDistance", ref WorldSize);

            PlanetSettings.Verify();
            BeltSettings.Verify();
        }
    }

    [ProtoContract]
    public class PlanetSettings
    {
        public PlanetSettings()
        {
            SizeMultiplier = 2;
            PlanetSizeCap = 1200000;
            MoonProbability = 0.5f;
            RingSettings = new PlanetRingSettings();
            ShowPlanetGPS = true;
            ShowMoonGPS = false;
            BlacklistedPlanets = new HashSet<string>();
            BlacklistedPlanets.Add("MoonTutorial");

            MandatoryPlanets = new HashSet<string>();

            Moons = new HashSet<string>();

            GasGiants = new HashSet<string>();
        }

        [ProtoMember(1)]
        public float SizeMultiplier;

        [ProtoMember(2)]
        public double PlanetSizeCap;

        [ProtoMember(3)]
        public float MoonProbability;

        [ProtoMember(4)]
        public PlanetRingSettings RingSettings;

        [ProtoMember(5)]
        public bool ShowPlanetGPS;

        [ProtoMember(6)]
        public bool ShowMoonGPS;

        [ProtoMember(7)]
        public HashSet<string> BlacklistedPlanets;

        [ProtoMember(8)]
        public HashSet<string> MandatoryPlanets;

        [ProtoMember(9)]
        public HashSet<string> Moons;

        [ProtoMember(10)]
        public HashSet<string> GasGiants;

        public void Verify()
        {
            Verifier.VerifyFloat(0, float.MaxValue, 2, "PlanetSettings.SizeMultiplier", ref SizeMultiplier);
            Verifier.VerifyDouble(0, double.MaxValue, 1200000, "PlanetSettings.PlanetSizeCap", ref PlanetSizeCap);
            Verifier.VerifyFloat(0, 1, 0.5f, "PlanetSettings.MoonProbability", ref MoonProbability);

            RingSettings.Verify();
        }
    }

    [ProtoContract]
    public class PlanetRingSettings
    {
        public PlanetRingSettings()
        {
            MinPlanetRingWidth = 10000;
            MaxPlanetRingWidth = 100000;
            PlanetRingProbability = 0.5f;
            ShowRingGPS = true;
        }

        [ProtoMember(1)]
        public int MinPlanetRingWidth;

        [ProtoMember(2)]
        public int MaxPlanetRingWidth;

        [ProtoMember(3)]
        public float PlanetRingProbability;

        [ProtoMember(4)]
        public bool ShowRingGPS;


        public void Verify()
        {
            Verifier.VerifyInt(0, MaxPlanetRingWidth, 10000, "PlanetRingSettings.MinPlanetRingWidth", ref MinPlanetRingWidth);
            Verifier.VerifyInt(MinPlanetRingWidth, int.MaxValue, 100000, "PlanetRingSettings.MaxPlanetRingWidth", ref MaxPlanetRingWidth);
            Verifier.VerifyFloat(0, 1, 0.5f, "PlanetRingSettings.PlanetRingProbability", ref PlanetRingProbability);
        }
    }

    [ProtoContract]
    public class BeltSettings
    {
        public BeltSettings()
        {
            MinBeltHeight = 4000;
            MaxBeltHeight = 40000;
            BeltProbability = 0.2f;
            ShowBeltGPS = true;
        }

        [ProtoMember(1)]
        public int MinBeltHeight;

        [ProtoMember(2)]
        public int MaxBeltHeight;

        [ProtoMember(3)]
        public float BeltProbability;

        [ProtoMember(4)]
        public bool ShowBeltGPS;

        public void Verify()
        {
            Verifier.VerifyInt(0, MaxBeltHeight, 4000, "BeltSettings.MinBeltHeight", ref MinBeltHeight);
            Verifier.VerifyInt(MinBeltHeight, int.MaxValue, 40000, "BeltSettings.MaxBeltHeight", ref MaxBeltHeight);
            Verifier.VerifyFloat(0, 1, 0.4f, "BeltSettings.BeltProbability", ref BeltProbability);
        }
    }

    public enum AsteroidGenerator
    {
        PLUGIN,
        VANILLA,
        BOTH
    }
}