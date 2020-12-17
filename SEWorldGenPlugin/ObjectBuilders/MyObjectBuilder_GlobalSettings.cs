using ProtoBuf;
using System;
using System.Collections.Generic;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [Serializable]
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
        public HashSet<string> BlacklistedPlanetDefinitions = new HashSet<string>();

        [ProtoMember(4)]
        public HashSet<string> MandatoryPlanetDefinitions = new HashSet<string>();

        public override MyAbstractConfigObjectBuilder copy()
        {
            var copy = new MyObjectBuilder_GlobalSettings();

            copy.MoonDefinitions = MoonDefinitions;
            copy.GasGiantDefinitions = GasGiantDefinitions;
            copy.BlacklistedPlanetDefinitions = BlacklistedPlanetDefinitions;
            copy.MandatoryPlanetDefinitions = MandatoryPlanetDefinitions;

            return copy;
        }

        public override void Verify()
        {
        }
    }
}
