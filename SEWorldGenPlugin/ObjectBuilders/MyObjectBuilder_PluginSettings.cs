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
            if(MySettings.Static != null)
                if(MySettings.Static.Settings != null)
                {
                    GeneratorSettings = MySettings.Static.Settings.GeneratorSettings.copy();
                }
                else
                {
                    GeneratorSettings = new GeneratorSettings();
                }
        }

        [ProtoMember(1)]
        public bool Enable;

        [ProtoMember(2)]
        public GeneratorSettings GeneratorSettings;

        public MyObjectBuilder_PluginSettings copy()
        {
            MyObjectBuilder_PluginSettings s = new MyObjectBuilder_PluginSettings();
            s.Enable = Enable;
            s.GeneratorSettings = GeneratorSettings.copy();
            return s;
        }

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

        public GeneratorSettings copy()
        {
            GeneratorSettings g = new GeneratorSettings();
            g.MinObjectsInSystem = MinObjectsInSystem;
            g.MaxObjectsInSystem = MaxObjectsInSystem;
            g.MinOrbitDistance = MinOrbitDistance;
            g.MaxOrbitDistance = MaxOrbitDistance;
            g.AsteroidGenerator = AsteroidGenerator;
            g.AsteroidDensity = AsteroidDensity;
            g.PlanetSettings = PlanetSettings.copy();
            g.BeltSettings = BeltSettings.copy();
            g.SemiRandomizedGeneration = SemiRandomizedGeneration;
            g.WorldSize = WorldSize;
            g.FirstPlanetCenter = FirstPlanetCenter;
            g.UseVanillaPlanets = UseVanillaPlanets;
            g.PlanetsOnlyOnce = PlanetsOnlyOnce;
            g.MoonsOnlyOnce = MoonsOnlyOnce;

            return g;
        }

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
            PlanetNameFormat = "Planet [ObjectNumber]";
            MoonNameFormat = "Moon [ObjectNumber]";

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
        public string PlanetNameFormat;

        [ProtoMember(8)]
        public string MoonNameFormat;

        [ProtoMember(9)]
        public HashSet<string> BlacklistedPlanets;

        [ProtoMember(10)]
        public HashSet<string> MandatoryPlanets;

        [ProtoMember(11)]
        public HashSet<string> Moons;

        [ProtoMember(12)]
        public HashSet<string> GasGiants;

        public PlanetSettings copy()
        {
            PlanetSettings p = new PlanetSettings();
            p.SizeMultiplier = SizeMultiplier;
            p.PlanetSizeCap = PlanetSizeCap;
            p.MoonProbability = MoonProbability;
            p.RingSettings = RingSettings.copy();
            p.ShowPlanetGPS = ShowPlanetGPS;
            p.ShowMoonGPS = ShowMoonGPS;
            p.PlanetNameFormat = PlanetNameFormat;
            p.MoonNameFormat = MoonNameFormat;
            p.BlacklistedPlanets = BlacklistedPlanets;
            p.MandatoryPlanets = MandatoryPlanets;
            p.Moons = Moons;
            p.GasGiants = GasGiants;

            return p;
        }

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

        public PlanetRingSettings copy()
        {
            PlanetRingSettings r = new PlanetRingSettings();
            r.MinPlanetRingWidth = MinPlanetRingWidth;
            r.MaxPlanetRingWidth = MaxPlanetRingWidth;
            r.PlanetRingProbability = PlanetRingProbability;
            r.ShowRingGPS = ShowRingGPS;

            return r;
        }

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
            BeltNameFormat = "Belt [ObjectNumberGreek]";
        }

        [ProtoMember(1)]
        public int MinBeltHeight;

        [ProtoMember(2)]
        public int MaxBeltHeight;

        [ProtoMember(3)]
        public float BeltProbability;

        [ProtoMember(4)]
        public bool ShowBeltGPS;

        [ProtoMember(5)]
        public string BeltNameFormat;

        public BeltSettings copy()
        {
            BeltSettings b = new BeltSettings();
            b.MinBeltHeight = MinBeltHeight;
            b.MaxBeltHeight = MaxBeltHeight;
            b.BeltProbability = BeltProbability;
            b.ShowBeltGPS = ShowBeltGPS;
            b.BeltNameFormat = BeltNameFormat;
            return b;
        }

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