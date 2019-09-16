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
            Enable = true;
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
        [ProtoMember(1)]
        [DefaultValue(true)]
        public int MinObjectsInSystem = 5;

        [ProtoMember(2)]
        [DefaultValue(true)]
        public int MaxObjectsInSystem = 25;

        [ProtoMember(3)]
        [DefaultValue(true)]
        public int MinOrbitDistance = 4000000;

        [ProtoMember(4)]
        [DefaultValue(true)]
        public int MaxOrbitDistance = 10000000;

        [ProtoMember(5)]
        [DefaultValue(true)]
        public PlanetSettings PlanetSettings = new PlanetSettings();

        [ProtoMember(6)]
        [DefaultValue(true)]
        public BeltSettings BeltSettings = new BeltSettings();

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
        [ProtoMember(1)]
        [DefaultValue(true)]
        public float SizeMultiplier = 2;

        [ProtoMember(2)]
        [DefaultValue(true)]
        public double PlanetSizeCap = 1200000;

        [ProtoMember(3)]
        [DefaultValue(true)]
        public float MoonProbability = 0.5f;

        [ProtoMember(4)]
        [DefaultValue(true)]
        public PlanetRingSettings RingSettings = new PlanetRingSettings();

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
        [ProtoMember(1)]
        [DefaultValue(true)]
        public int MinPlanetRingWidth = 10000;

        [ProtoMember(2)]
        [DefaultValue(true)]
        public int MaxPlanetRingWidth = 100000;

        public void Verify()
        {
            Verifier.VerifyInt(0, MaxPlanetRingWidth, 10000, "PlanetRingSettings.MinPlanetRingWidth", ref MinPlanetRingWidth);
            Verifier.VerifyInt(MinPlanetRingWidth, int.MaxValue, 100000, "PlanetRingSettings.MaxPlanetRingWidth", ref MaxPlanetRingWidth);
        }
    }

    [ProtoContract]
    public class BeltSettings
    {
        [ProtoMember(1)]
        [DefaultValue(true)]
        public int MinBeltHeight = 4000;

        [ProtoMember(2)]
        [DefaultValue(true)]
        public int MaxBeltHeight = 40000;

        [ProtoMember(3)]
        [DefaultValue(true)]
        public float BeltProbability = 0.4f;

        public void Verify()
        {
            Verifier.VerifyInt(0, MaxBeltHeight, 4000, "BeltSettings.MinBeltHeight", ref MinBeltHeight);
            Verifier.VerifyInt(MinBeltHeight, int.MaxValue, 40000, "BeltSettings.MaxBeltHeight", ref MaxBeltHeight);
            Verifier.VerifyFloat(0, 1, 0.4f, "BeltSettings.BeltProbability", ref BeltProbability);
        }
    }
}