using ProtoBuf;
using SEWorldGenPlugin.Utilities;
using System.ComponentModel;

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
            PlanetSettings = new PlanetSettings();
            BeltSettings = new BeltSettings();
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
        public PlanetSettings PlanetSettings;

        [ProtoMember(6)]
        public BeltSettings BeltSettings;

        public void Verify()
        {
            Verifier.VerifyInt(0, MaxObjectsInSystem, 5, "MinObjectsInSystem", ref MinObjectsInSystem);
            Verifier.VerifyInt(MinObjectsInSystem, int.MaxValue, 25, "MaxObjectsInSystem", ref MaxObjectsInSystem);
            Verifier.VerifyInt(0, MinOrbitDistance, 4000000, "MinOrbitDistance", ref MinOrbitDistance);
            Verifier.VerifyInt(MinOrbitDistance, int.MaxValue, 10000000, "MaxOrbitDistance", ref MaxOrbitDistance);

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
        }

        [ProtoMember(1)]
        public float SizeMultiplier;

        [ProtoMember(2)]
        public double PlanetSizeCap;

        [ProtoMember(3)]
        public float MoonProbability;

        [ProtoMember(4)]
        public PlanetRingSettings RingSettings;

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
        }

        [ProtoMember(1)]
        public int MinPlanetRingWidth;

        [ProtoMember(2)]
        public int MaxPlanetRingWidth;

        public void Verify()
        {
            Verifier.VerifyInt(0, MaxPlanetRingWidth, 10000, "PlanetRingSettings.MinPlanetRingWidth", ref MinPlanetRingWidth);
            Verifier.VerifyInt(MinPlanetRingWidth, int.MaxValue, 100000, "PlanetRingSettings.MaxPlanetRingWidth", ref MaxPlanetRingWidth);
        }
    }

    [ProtoContract]
    public class BeltSettings
    {
        public BeltSettings()
        {
            MinBeltHeight = 4000;
            MaxBeltHeight = 40000;
            BeltProbability = 0.4f;
        }

        [ProtoMember(1)]
        public int MinBeltHeight;

        [ProtoMember(2)]
        public int MaxBeltHeight;

        [ProtoMember(3)]
        public float BeltProbability;

        public void Verify()
        {
            Verifier.VerifyInt(0, MaxBeltHeight, 4000, "BeltSettings.MinBeltHeight", ref MinBeltHeight);
            Verifier.VerifyInt(MinBeltHeight, int.MaxValue, 40000, "BeltSettings.MaxBeltHeight", ref MaxBeltHeight);
            Verifier.VerifyFloat(0, 1, 0.4f, "BeltSettings.BeltProbability", ref BeltProbability);
        }
    }
}