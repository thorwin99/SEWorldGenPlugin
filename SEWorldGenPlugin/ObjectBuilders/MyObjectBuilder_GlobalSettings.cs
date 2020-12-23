using ProtoBuf;
using System;
using System.Collections.Generic;

namespace SEWorldGenPlugin.ObjectBuilders
{
    /// <summary>
    /// Serializable object builder used to store the global settings of the plugin.
    /// Those are valid for all worlds.
    /// </summary>
    [ProtoContract]
    public class MyObjectBuilder_GlobalSettings : MyAbstractConfigObjectBuilder
    {
        public MyObjectBuilder_GlobalSettings() : base()
        {
        }

        [ProtoMember(1)]
        public HashSet<string> MoonDefinitions = new HashSet<string>();

        [ProtoMember(2)]
        public HashSet<string> GasGiantDefinitions = new HashSet<string>();

        [ProtoMember(3)]
        public HashSet<string> SunDefinitions = new HashSet<string>();

        [ProtoMember(4)]
        public HashSet<string> BlacklistedPlanetDefinitions = new HashSet<string>();

        [ProtoMember(5)]
        public HashSet<string> MandatoryPlanetDefinitions = new HashSet<string>();

        [ProtoMember(6)]
        public string PlanetNameFormat = "Planet [ObjectNumber]";

        [ProtoMember(7)]
        public string MoonNameFormat = "Moon [ObjectNumber]";

        [ProtoMember(8)]
        public string BeltNameFormat = "Belt [ObjectNumberGreek]";

        public override MyAbstractConfigObjectBuilder copy()
        {
            var copy = new MyObjectBuilder_GlobalSettings();

            copy.MoonDefinitions = MoonDefinitions;
            copy.GasGiantDefinitions = GasGiantDefinitions;
            copy.BlacklistedPlanetDefinitions = BlacklistedPlanetDefinitions;
            copy.MandatoryPlanetDefinitions = MandatoryPlanetDefinitions;
            copy.PlanetNameFormat = PlanetNameFormat;
            copy.MoonNameFormat = MoonNameFormat;
            copy.BeltNameFormat = BeltNameFormat;

            return copy;
        }

        public override void Verify()
        {
        }
    }
}
